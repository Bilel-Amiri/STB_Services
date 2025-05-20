using Microsoft.EntityFrameworkCore;
using Reclamation_Service.Models;
using Microsoft.Extensions.Logging;
using Reclamation_Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net.Http.Headers;

namespace Reclamation_Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // DbContext
            builder.Services.AddDbContext<ReclamationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("ReclamationConnection"))
                       .LogTo(Console.WriteLine, LogLevel.Information)
                       .EnableSensitiveDataLogging(); // for development only
            });




            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    builder => builder.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()  
                        .AllowAnyMethod());  
            });






            builder.Services.AddHttpClient<IUserServiceHttpClient, UserServiceHttpClient>((provider, client) =>
            {
                client.BaseAddress = new Uri("http://localhost:5142"); // UserService base URL
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
 .ConfigurePrimaryHttpMessageHandler(() =>
 {
     return new HttpClientHandler
     {
         ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
     };
 })
 .SetHandlerLifetime(TimeSpan.FromMinutes(5))  // Adjust the lifetime of the handler if needed
 .AddPolicyHandler(GetRetryPolicy())          // Add retry policy
 .AddPolicyHandler(GetTimeoutPolicy())        // Add timeout policy
 .AddPolicyHandler(GetCircuitBreakerPolicy()); // Optional: Add circuit breaker if needed







            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           var jwtKey = builder.Configuration["Jwt:Key"];
           if (string.IsNullOrEmpty(jwtKey))
               throw new ArgumentNullException("Jwt:Key", "JWT secret key is not configured");

           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = builder.Configuration["Jwt:Issuer"],
               ValidAudience = builder.Configuration["Jwt:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
           };
       });





            builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(GetRetryPolicy());
            builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(GetTimeoutPolicy());


            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IReclamationRepository, ReclamationRepository>();
            builder.Services.AddScoped<IUserServiceHttpClient, UserServiceHttpClient>();

            var app = builder.Build();

            // Test DB connection

            app.UseCors("AllowAngular");

            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ReclamationDbContext>();

                Console.WriteLine("Attempting to connect to database...");
                var canConnect = await dbContext.Database.CanConnectAsync();

                if (canConnect)
                {
                    Console.WriteLine("✅ Successfully connected to database!");
                    Console.WriteLine($"Database: {dbContext.Database.GetDbConnection().Database}");
                    Console.WriteLine($"Server: {dbContext.Database.GetDbConnection().DataSource}");
                }
                else
                {
                    Console.WriteLine("❌ Could not connect to database");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Database connection failed!");
                Console.WriteLine($"Error: {ex.Message}");
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }




            // ✅ Correct middleware order
            app.UseAuthentication();
            app.UseAuthorization();

           


            app.UseHttpsRedirection();
            app.MapControllers();
            app.Run();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (outcome, delay, retryAttempt, context) =>
                    {
                        var logger = context["Logger"] as ILogger;
                        logger?.LogWarning($"Retry {retryAttempt} after {delay.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    })
                .WithPolicyKey("RetryPolicy");
        }



        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30),
                onTimeoutAsync: (context, timeSpan, task) =>
                {
                    var logger = context["Logger"] as ILogger;
                    logger?.LogWarning($"Timeout occurred after {timeSpan.TotalSeconds} seconds.");
                    return Task.CompletedTask;
                });
        }





        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, breakDelay) =>
                    {
                        Console.WriteLine($"🔴 Circuit broken! Delay: {breakDelay.TotalSeconds}s, Reason: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("🟢 Circuit reset.");
                    });
        }
    }
}
