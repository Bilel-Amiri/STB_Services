using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Polly.CircuitBreaker;
using Credit_Service.Interfaces;
using Credit_Service.Models;

namespace Credit_Service.Services
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _resiliencePolicy;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserServiceClient(
            HttpClient httpClient,
            ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            // Build resilience pipeline
            _resiliencePolicy = BuildResiliencePipeline();
        }

        private IAsyncPolicy<HttpResponseMessage> BuildResiliencePipeline()
        {
            // Retry Policy with exponential backoff
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, delay, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retry attempt {RetryCount} after {Delay}s for {Operation}. Reason: {Reason}",
                            retryCount,
                            delay.TotalSeconds,
                            context.OperationKey,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    });

            // Timeout Policy (optimistic)
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(15),
                TimeoutStrategy.Optimistic);

            // Circuit Breaker Policy
            var circuitBreakerPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => (int)r.StatusCode >= 500)
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.5,
                    samplingDuration: TimeSpan.FromSeconds(60),
                    minimumThroughput: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (result, breakDelay, context) =>
                    {
                        _logger.LogWarning(
                            "Circuit broken! Blocking calls for {BreakDuration}s. Reason: {Reason}",
                            breakDelay.TotalSeconds,
                            result.Exception?.Message ?? result.Result?.StatusCode.ToString());
                    },
                    onReset: context =>
                    {
                        _logger.LogInformation("Circuit reset");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Circuit half-open");
                    });

            // Combine policies: Retry -> Timeout -> Circuit Breaker
            return Policy.WrapAsync(retryPolicy, timeoutPolicy, circuitBreakerPolicy);
        }

        public async Task<AccountDto> GetAccountAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync<AccountDto>(
                $"api/auth/account/{accountId}",
                cancellationToken);
        }

        public async Task<AccountDto> GetAccountByRibAsync(long rib, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync<AccountDto>(
                $"api/auth/account/by-rib/{rib}",
                cancellationToken);
        }

        public async Task<UserInfoDto> GetUserInfoAsync(int clientId, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync<UserInfoDto>(
                $"api/auth/user-info/{clientId}",
                cancellationToken);
        }

        public async Task<UserInfoDto> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var encodedEmail = Uri.EscapeDataString(email);
            return await ExecuteRequestAsync<UserInfoDto>(
                $"api/auth/user-by-email/{encodedEmail}",
                cancellationToken);
        }

        public async Task<bool> UpdateBalanceAsync(UpdateBalanceRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _resiliencePolicy.ExecuteAsync(async (ct) =>
                {
                    var content = new StringContent(
                        JsonSerializer.Serialize(request, _jsonOptions),
                        Encoding.UTF8,
                        "application/json");

                    return await _httpClient.PostAsync("api/auth/update-balance", content, ct);
                }, cancellationToken);

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<bool>(
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update balance for account {AccountId}", request.AccountId);
                throw;
            }
        }

        private async Task<T> ExecuteRequestAsync<T>(string endpoint, CancellationToken cancellationToken)
            where T : class
        {
            try
            {
                var response = await _resiliencePolicy.ExecuteAsync(async (ct) =>
                {
                    return await _httpClient.GetAsync(endpoint, ct);
                }, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>(
                    _jsonOptions,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute request to {Endpoint}", endpoint);
                throw;
            }
        }
    }
}
public class AccountDto
    {
        public int AccountId { get; set; }
        public int ClientId { get; set; }
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
    }
