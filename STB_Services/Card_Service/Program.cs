using Microsoft.EntityFrameworkCore;
using Card_Service.Models;
using Card_Service.Services;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Card_Service.Controllers;

namespace Card_Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            
            builder.Services.AddDbContext<CardDBcontext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("CardConnection")));

           
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<ICardService, CardService>();





            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    builder => builder.WithOrigins("http://localhost:4200") // Allow requests from this specific origin
                        .AllowAnyHeader()  // Allow any headers
                        .AllowAnyMethod());  // Allow any HTTP methods (GET, POST, etc.)
            });







            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
          var jwtKey = builder.Configuration["Jwt:Key"];
          if (string.IsNullOrEmpty(jwtKey))
          {
              throw new ArgumentNullException("Jwt:Key", "JWT secret key is not configured");
          }

          options.TokenValidationParameters = new TokenValidationParameters
          {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              ValidIssuer = builder.Configuration["Jwt:Issuer"],
              ValidAudience = builder.Configuration["Jwt:Audience"],
              IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(jwtKey))
          };
      });










            builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>((provider, client) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();

                // Configure base address
                client.BaseAddress = new Uri(configuration["UserService:BaseUrl"]);

                // Configure default headers
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Add authorization if needed
                var authToken = configuration["ServiceAuth:Token"];
                if (!string.IsNullOrEmpty(authToken))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", authToken);
                }




                client.Timeout = TimeSpan.FromSeconds(
      configuration.GetValue<int>("UserService:TimeoutSeconds", 10));
            });




           




            var app = builder.Build();




            app.UseCors("AllowAngular");




            app.UseAuthentication();
            app.UseAuthorization();









            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            // Test database connection
            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CardDBcontext>();
                var canConnect = await dbContext.Database.CanConnectAsync();

                if (canConnect)
                {
                    Console.WriteLine("✅ Database connection successful!");
                    Console.WriteLine($"🔗 Connected to: {builder.Configuration.GetConnectionString("CardConnection")}");
                }
                else
                {
                    Console.WriteLine("❌ Database connection failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error connecting to database: {ex.Message}");
            }

            app.Run();
        }
    }
}