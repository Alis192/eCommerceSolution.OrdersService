using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class PollyPolicies : IPollyPolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;

    public PollyPolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking,
            durationOfBreak: durationOfBreak,
            onBreak: (result, timeSpan) =>
            {
                _logger.LogInformation($"Circuit breaker opened for {timeSpan.TotalMinutes} minutes due to consecutive 3 failure. The subsequent requests will be blocked");
            },
            onReset: () =>
            {
                _logger.LogInformation("Circuit breaker closed. The subsequent requests will be allowed");
            }
        );

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: retryCount,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (result, timeSpan, retryCount, context) =>
            {
                _logger.LogInformation($"Retry {retryCount} after {timeSpan} seconds");
            }
        );

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);

        return policy;        
    }
}

