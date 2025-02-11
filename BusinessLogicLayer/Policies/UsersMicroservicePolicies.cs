using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
    private ILogger<UsersMicroservicePolicies> _logger;

    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
            onRetry: (result, timeSpan, retryCount, context) =>
            {
                _logger.LogInformation($"Retry {retryCount} after {timeSpan} seconds");
            }
        );

        return policy;
    }
}

