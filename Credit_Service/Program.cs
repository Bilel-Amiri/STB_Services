using System.Net.Http.Headers;
using Credit_Service.DTOs;
using Credit_Service.Interfaces;
using Credit_Service.Models;
using Credit_Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;

namespace Credit_Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Database Configuration
            builder.Services.AddDbContext<CreditDBcontext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("CreditConnection");
                Console.WriteLine("Configuring database connection...");

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

                if (builder.Environment.IsDevelopment())
                {
                    options.LogTo(Console.WriteLine, LogLevel.Information)
                           .EnableSensitiveDataLogging()
                           .EnableDetailedErrors();
                }
            });

            // HTTP Client Configuration
            builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>((provider, client) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var apiConfig = configuration.GetSection("ApiSettings");

                var baseUrl = apiConfig["UserServiceBaseUrl"] ?? throw new InvalidOperationException("UserServiceBaseUrl is not configured");
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Configure timeout
                client.Timeout = TimeSpan.FromSeconds(30);

                // Configure authentication if needed
                var apiKey = apiConfig["UserServiceApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);
                }
            })
            .AddPolicyHandler((provider, _) =>
                Policy<HttpResponseMessage>
                    .Handle<HttpRequestException>()
                    .OrResult(r => (int)r.StatusCode >= 500)
                    .WaitAndRetryAsync(3, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (outcome, delay, retryCount, _) =>
                        {
                            var logger = provider.GetRequiredService<ILogger<UserServiceClient>>();
                            logger.LogWarning(
                                $"Retry {retryCount} after {delay.TotalSeconds}s for {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                        }))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10))
            .AddPolicyHandler(Policy
                .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

            // Service Registrations
            builder.Services.AddScoped<ICreditRepository, CreditRepository>();
            builder.Services.AddScoped<ICreditService, CreditService>();
            builder.Services.AddScoped<ICreditStatusService, CreditService>();
            builder.Services.AddScoped<ICreditSimulationService, CreditService>();
            builder.Services.AddScoped<INotificationSender, NotificationSender>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();


            // API Configuration
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            builder.Services.AddSingleton<RateConfiguration>();
            builder.Services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();

            var app = builder.Build();

            // Database Connection Test
            if (app.Environment.IsDevelopment())
            {
                TestDatabaseConnection(app);
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            Console.WriteLine("Application started successfully");
            app.Run();
        }

        private static void TestDatabaseConnection(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var dbContext = services.GetRequiredService<CreditDBcontext>();
                dbContext.Database.OpenConnection();
                dbContext.Database.CloseConnection();
                Console.WriteLine("Database connection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database connection failed!");
                Console.WriteLine($"Error: {ex.Message}");
                // In production, you might want to exit here
                if (!app.Environment.IsDevelopment())
                {
                    Environment.Exit(1);
                }
            }
        }
    }
}