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
using Transaction_Service.Services;



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

                    return await _httpClient.SendAsync(request, ct);
                },
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Account {accountId} not found");
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Authentication failed when fetching account by ID");
                throw new UnauthorizedAccessException("Service authentication failed");
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AccountDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to fetch account {accountId}");
            throw;
        }
    }

    public async Task<AccountDto?> GetAccountByRibAsync(
    long rib,
    CancellationToken cancellationToken = default)
    {
        using var _ = _logger.BeginScope(new { Rib = rib });
        _logger.LogInformation("Fetching account by RIB");

        try
        {
            var response = await _retryPolicy
                .WrapAsync(_timeoutPolicy)
                .ExecuteAsync(async (ct) =>
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        $"/api/auth/account/by-rib/{rib}");

                    // JWT header is automatically added via _httpClient.DefaultRequestHeaders
                    return await _httpClient.SendAsync(request, ct);
                },
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Account with RIB {rib} not found");
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Authentication failed when fetching account by RIB");
                throw new UnauthorizedAccessException("Service authentication failed");
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AccountDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to fetch account by RIB {rib}");
            throw;
        }
    }

    public async Task<bool> UpdateBalanceAsync(
        UpdateBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        using var _ = _logger.BeginScope(new { request.AccountId });
        _logger.LogInformation("Updating account balance");

        try
        {
            var response = await _retryPolicy
                .WrapAsync(_timeoutPolicy)
                .ExecuteAsync(async (ct) =>
                {
                    var requestMessage = new HttpRequestMessage(
                        HttpMethod.Post,
                        "/api/auth/update-balance")
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(request),
                            Encoding.UTF8,
                            "application/json")
                    };

                    return await _httpClient.SendAsync(requestMessage, ct);
                },
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Account not found");
                return false;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Authentication failed when updating balance");
                throw new UnauthorizedAccessException("Service authentication failed");
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update balance");
            throw;
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

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {clientId} not found");
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Authentication failed when fetching user info");
                throw new UnauthorizedAccessException("Service authentication failed");
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserInfoDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to fetch user info for client {clientId}");
            throw;
        }
    }


    public async Task<UserInfoDto?> GetUserByEmailAsync(
    string email,
    CancellationToken cancellationToken = default)
    {
        using var _ = _logger.BeginScope(new { Email = email });
        _logger.LogInformation("Fetching user by email");

        try
        {
            var response = await _retryPolicy
                .WrapAsync(_timeoutPolicy)
                .ExecuteAsync(async (ct) =>
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        $"/api/auth/user-by-email/{WebUtility.UrlEncode(email)}");

                    return await _httpClient.SendAsync(request, ct);
                },
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User with email {email} not found");
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Authentication failed when fetching user by email");
                throw new UnauthorizedAccessException("Service authentication failed");
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserInfoDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to fetch user by email {email}");
            throw;
        }
    }


}

public class AccountDto
{
    public int AccountId { get; set; }
    public int ClientId { get; set; }
    public string AccountType { get; set; }
    public long Rib { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreationDate { get; set; }
}

public class UpdateBalanceRequest
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
}
public class UserInfoDto
{
    public int ClientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    // Add any other relevant user properties you need
}




