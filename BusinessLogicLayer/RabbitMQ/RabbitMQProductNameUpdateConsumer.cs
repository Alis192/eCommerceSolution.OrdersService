using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersService.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateConsumer : IDisposable, IRabbitMQProductNameUpdateConsumer
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public RabbitMQProductNameUpdateConsumer(IConfiguration configuration)
    {
        _configuration = configuration;
        string hostName = _configuration["RabbitMQ_HostName"]!;
        string userName = _configuration["RabbitMQ_UserName"]!;
        string password = _configuration["RabbitMQ_Password"]!;
        string port = _configuration["RabbitMQ_Port"]!;

        ConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };

        _connection = connectionFactory.CreateConnection();

        _channel = _connection.CreateModel();
    }

    public void Consume()
    {
        string routingKey = "product.update.name";
        string queueName = "orders.product.name.update.queue";

        //Create exchange
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

        //Create message queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        //Bind queue to exchange
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}

