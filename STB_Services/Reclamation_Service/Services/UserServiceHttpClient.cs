using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Polly;
using Polly.Retry;
using System.Text.Json;
using Polly;
using Polly.Timeout;
using Azure.Core.Pipeline;

namespace Reclamation_Service.Services
{
    public interface IUserServiceHttpClient
    {
        Task<AccountDto?> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default);
        Task<UserInfoDto?> GetUserInfoAsync(int clientId, CancellationToken cancellationToken = default);
        Task<int?> AssignReclamationToAdminAsync(int reclamationId, CancellationToken cancellationToken);
        Task<int?> GetNextAvailableAdminIdAsync(CancellationToken cancellationToken);
        Task<bool> SetAdminAvailabilityAsync(int adminId, bool isAvailable);
        Task<UserInfoDto?> GetUserInfoByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
    }

    public class UserServiceHttpClient : IUserServiceHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceHttpClient> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IAsyncPolicy<HttpResponseMessage> _timeoutPolicy;
        private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;


        public UserServiceHttpClient(
            HttpClient httpClient,
            ILogger<UserServiceHttpClient> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
             IAsyncPolicy<HttpResponseMessage> timeoutPolicy,
        IAsyncPolicy<HttpResponseMessage> circuitBreakerPolicy)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            _httpClient.BaseAddress = new Uri(configuration["UserService:BaseUrl"]);
            _configuration = configuration;
            _timeoutPolicy = timeoutPolicy;
            _circuitBreakerPolicy = circuitBreakerPolicy;



            var accessToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken.Replace("Bearer ", ""));
            }


            _retryPolicy = Policy<HttpResponseMessage>
                   .Handle<HttpRequestException>()
                   .OrResult(r => (int)r.StatusCode >= 500)
                   .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

            // Assign the timeout policy passed in the constructor
            _timeoutPolicy = timeoutPolicy;
        }



        public async Task<AccountDto?> GetAccountByIdAsync(
      int accountId,
      CancellationToken cancellationToken = default)
        {
            using var _ = _logger.BeginScope(new { AccountId = accountId });
            _logger.LogInformation("Fetching account by ID");

            try
            {
                var response = await _retryPolicy
                    .WrapAsync(_timeoutPolicy)
                    .ExecuteAsync(async (ct) =>
                    {
                        var request = new HttpRequestMessage(
                            HttpMethod.Get,
                            $"/api/auth/account/{accountId}");

                        // (Optional) Add headers if needed (e.g., API keys)
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        return await _httpClient.SendAsync(request, ct);
                    },
                    cancellationToken);

                // Handle 404 explicitly (now logged + returns null)
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Account {accountId} not found (404)");
                    return null;
                }

                // Handle 401/403 explicitly
                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogError($"Auth failed for account {accountId} (HTTP {(int)response.StatusCode})");
                    throw new UnauthorizedAccessException("Access denied");
                }

                // Ensure success (throws for 5xx/other errors)
                response.EnsureSuccessStatusCode();

                // Deserialize response
                return await response.Content.ReadFromJsonAsync<AccountDto>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                    cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Redundant safety net (already handled above)
                _logger.LogWarning(ex, $"Account {accountId} not found");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch account {accountId}");
                throw; // Re-throw for the controller to handle
            }
        }



        public async Task<UserInfoDto?> GetUserInfoAsync(
      int clientId,
      CancellationToken cancellationToken = default)
        {
            using var _ = _logger.BeginScope(new { ClientId = clientId });
            _logger.LogInformation("Fetching user info by client ID");

            try
            {
                var response = await _retryPolicy
                    .WrapAsync(_timeoutPolicy)
                    .ExecuteAsync(async (ct) =>
                    {
                        var request = new HttpRequestMessage(
                            HttpMethod.Get,
                             $"/api/auth/user-info/{clientId}");
                        return await _httpClient.SendAsync(request, ct);
                    },
                    cancellationToken);

                return await HandleResponse<UserInfoDto>(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch user info for client {clientId}");
                throw;
            }
        }




        public async Task<UserInfoDto?> GetUserInfoByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            using var _ = _logger.BeginScope(new { AccountId = accountId });
            _logger.LogInformation("Fetching user info by account ID");

            try
            {
                // Ensure timeout policy is used
                var timeoutPolicy = Policy
                    .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

                // Wrap both the retry policy and timeout policy
                var response = await _retryPolicy
                    .WrapAsync(timeoutPolicy)  // Wrap the timeout policy here
                    .ExecuteAsync(async (ct) =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/auth/account-info/{accountId}");
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        return await _httpClient.SendAsync(request, ct);
                    }, cancellationToken);

                return await HandleResponse<UserInfoDto>(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch user info for account {accountId}");
                throw;
            }
        }






        public async Task<int?> AssignReclamationToAdminAsync(int reclamationId, CancellationToken cancellationToken)
        {
            try
            {
               

                // Step 1: Get the next available admin
                var adminId = await GetNextAvailableAdminIdAsync(cancellationToken);
                if (adminId == null)
                {
                    _logger.LogWarning("No available admin to assign reclamation {ReclamationId}", reclamationId);
                    return null;
                }

                // Step 2: Build the correct endpoint URL with adminId
                var url = $"/api/admins/{adminId}/assign";
                var request = new { ReclamationId = reclamationId };

                // Step 3: Call the API
                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsJsonAsync(url, request, cancellationToken)
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Assignment failed with status: {StatusCode}", response.StatusCode);
                    return null;
                }

                _logger.LogInformation("Successfully assigned reclamation {ReclamationId} to admin {AdminId}", reclamationId, adminId);

                return adminId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign reclamation {ReclamationId} to admin", reclamationId);
                return null;
            }
        }


        public async Task<int?> GetNextAvailableAdminIdAsync(CancellationToken cancellationToken)
        {
            try
            {
              

                var response = await _httpClient.GetAsync("/api/admins/available", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get available admin. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var adminId = await response.Content.ReadFromJsonAsync<int?>(cancellationToken: cancellationToken);
                if (adminId == null)
                {
                    _logger.LogWarning("No available admin found.");
                    return null;
                }

                return adminId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching available admin.");
                return null;
            }
        }



        private async Task<T?> HandleResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Resource not found");
                return default;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Authentication failed");
                throw new UnauthorizedAccessException("Service authentication failed");
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }

        public async Task<bool> SetAdminAvailabilityAsync(int adminId, bool isAvailable)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync(
                    $"/api/admins/{adminId}/availability",
                    new { IsAvailable = isAvailable });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to set availability for admin {AdminId}. Status: {StatusCode}",
                        adminId, response.StatusCode);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling admin service for admin {AdminId}", adminId);
                return false;
            }
        }

    }





}



public class AccountClientInfoDto
    {
        public int AccountId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
    }


public class AccountDto
{
    public int AccountId { get; set; }
    public int ClientId { get; set; }
    public long? Rib { get; set; }
    public string AccountType { get; set; } = null!;
    public DateTime? CreationDate { get; set; }
    public decimal? Balance { get; set; }
}
public class UserInfoDto
{
    public int ClientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Status { get; set; }
}