using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Card_Service.Models;
using Card_Service.Services;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncTimeoutPolicy _timeoutPolicy;

    public UserServiceClient(
        HttpClient httpClient,
        ILogger<UserServiceClient> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;

        // Configure authentication header
        var authToken = _configuration["ServiceAuth:Token"];
        if (!string.IsNullOrEmpty(authToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);
        }

        // Configure retry policy
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, delay, retryCount, context) =>
                {
                    var message = outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString();
                    _logger.LogWarning($"Retry {retryCount} due to {message}");
                });

        // Configure timeout policy
        _timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10));
    }



    public async Task<bool> UserExistsAsync(int ClientId)
    {
        // Implementation depends on your user service
        var user = await GetUserInfoAsync(ClientId);
        return user != null;
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

    public async Task<bool> ValidateCardOwnershipAsync(
        int clientId,
        int cardId,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.BeginScope(new { ClientId = clientId, CardId = cardId });
        _logger.LogInformation("Validating card ownership");

        try
        {
            var response = await _retryPolicy
                .WrapAsync(_timeoutPolicy)
                .ExecuteAsync(async (ct) =>
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        $"/api/cards/validate-ownership?clientId={clientId}&cardId={cardId}");
                    return await _httpClient.SendAsync(request, ct);
                },
                cancellationToken);

            return await HandleResponse<bool>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to validate card ownership for client {clientId}, card {cardId}");
            throw;
        }
    }

    public async Task<bool> HasAccountPermissionAsync(
        int clientId,
        int accountId,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.BeginScope(new { ClientId = clientId, AccountId = accountId });
        _logger.LogInformation("Checking account permissions");

        try
        {
            var response = await _retryPolicy
                .WrapAsync(_timeoutPolicy)
                .ExecuteAsync(async (ct) =>
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        $"/api/accounts/has-permission?clientId={clientId}&accountId={accountId}");
                    return await _httpClient.SendAsync(request, ct);
                },
                cancellationToken);

            return await HandleResponse<bool>(response, cancellationToken) ;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to check account permissions for client {clientId}, account {accountId}");
            throw;
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
}

public class AccountDto
{
    public int AccountId { get; set; }
    public int ClientId { get; set; }
    public string AccountType { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; }
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