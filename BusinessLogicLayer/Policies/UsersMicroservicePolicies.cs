using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
    private ILogger<UsersMicroservicePolicies> _logger;

    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromMinutes(2),
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

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (result, timeSpan, retryCount, context) =>
            {
                _logger.LogInformation($"Retry {retryCount} after {timeSpan} seconds");
            }
        );

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMicroseconds(1500));

        return policy;        
    }

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = GetRetryPolicy();
        var circuitBreakerPolicy = GetCircuitBreakerPolicy();
        var timeoutPolicy = GetTimeoutPolicy();
        
        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

        return wrappedPolicy;
    }
}

