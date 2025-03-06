using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger;

    public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: 2, //Allow only up to 2 concurrent requests
            maxQueuingActions: 40, //Allow only up to 40 requests in the queue
            onBulkheadRejectedAsync: (context) =>
            {
                _logger.LogWarning("Bulkhead isolation policy triggered: The request is rejected due to the bulkhead isolation policy");
                throw new BulkheadRejectedException("Bulkhead queue is full");
            });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(async(context) =>
            {
                _logger.LogWarning("Fallback triggered: The request failed. Returning dummy data");

                ProductDTO product = new ProductDTO(
                    ProductID: Guid.Empty,
                    ProductName: "Temporarly Unavailable",
                    Category: "Temporarly Unavailable",
                    UnitPrice: 0,
                    QuantityInStock: 0);

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
                };

                return response;
            });

        return policy;
    }
}

