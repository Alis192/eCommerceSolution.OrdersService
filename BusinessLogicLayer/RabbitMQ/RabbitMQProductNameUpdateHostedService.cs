using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateHostedService : IHostedService
{
    private readonly IRabbitMQProductNameUpdateConsumer _productNameUpdateConsumer;

    public RabbitMQProductNameUpdateHostedService(IRabbitMQProductNameUpdateConsumer consumer)
    {
        _productNameUpdateConsumer = consumer;
    }

    //Will be called by the host when the application starts, no need to call manually
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _productNameUpdateConsumer.Consume();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productNameUpdateConsumer.Dispose();

        return Task.CompletedTask;
    }
}

