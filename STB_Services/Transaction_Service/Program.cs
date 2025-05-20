using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Transaction_Service.Services;
using Transaction_Service.Model;
using System;

namespace Transaction_Service
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

            // Add configuration
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Register HttpClient with proper configuration
            builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>((httpClient, provider) =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<UserServiceClient>>();

                httpClient.BaseAddress = new Uri(config["UserService:BaseUrl"] ?? "http://localhost:5142");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                return new UserServiceClient(
                    httpClient,
                    logger,
                    config);
            });



            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    builder => builder.WithOrigins("http://localhost:4200")
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });

            // And later
           





            // Database configuration
            builder.Services.AddDbContext<TransactionDBContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("TransactionConnection");
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
            });

            // Register services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<TransactionService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.UseCors("AllowAngular");

            // Log application startup
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application starting at: {time}", DateTimeOffset.Now);

           

            app.Run();
        }

       
    }
}
