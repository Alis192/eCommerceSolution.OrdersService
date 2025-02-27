using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients
{
    public class UsersMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersMicroserviceClient> _logger;

        public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserDTO?> GetUserByUserID(Guid userID)
        {

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return null;
                    }

                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                    }

                    else
                    {
                        //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                        return new UserDTO(
                            PersonName: "Temporarly Unavailable",
                            Email: "Temporarly Unavailable",
                            Gender: "Temporarly Unavailable",
                            UserID: Guid.Empty);
                    }
                }

                UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

                if (user == null)
                {
                    throw new ArgumentException("Invalid User ID");
                }

                return user;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Request failed because of circuit breaker is in open status. Returning dummy data");
                return new UserDTO(
                            PersonName: "Temporarly Unavailable (circuit breaker)",
                            Email: "Temporarly Unavailable (circuit breaker)",
                            Gender: "Temporarly Unavailable (circuit breaker)",
                            UserID: Guid.Empty);
            }
            catch (TimeoutRejectedException ex)
            {
                _logger.LogError(ex, "Timeout occured while fetching user data. Returning dummy data");
                return new UserDTO(
                            PersonName: "Temporarly Unavailable (timeout)",
                            Email: "Temporarly Unavailable (timeout)",
                            Gender: "Temporarly Unavailable (timeout)",
                            UserID: Guid.Empty);
            }
        }
    }
}
