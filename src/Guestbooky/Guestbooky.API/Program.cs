using Guestbooky.Application.DependencyInjection;
using Guestbooky.Infrastructure.DependencyInjection;
using Guestbooky.Infrastructure.Environment;
using Guestbooky.API.Configurations;
using Guestbooky.API.Enums;
using Guestbooky.API.Validations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

namespace Guestbooky.API
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            await builder.AddLogging();

            if (!ValidateEnvironment(builder.Configuration))
                return;

            await builder.AddServices();

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("local");

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            await app.RunAsync();
        }

        private static ValueTask AddLogging(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, conf) =>
            {
                conf.UseLogLevel(builder.Configuration[Constants.LOG_LEVEL]!.ToUpper());
                conf.WriteTo.Console();
                conf.ReadFrom.Configuration(context.Configuration);
                conf.MinimumLevel.Debug();
                conf.Enrich.AtLevel(Serilog.Events.LogEventLevel.Debug, enricher =>
                {
                    enricher.FromLogContext();
                });
            });

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Upon checking we have all variables we need, perform the necessary injections. <br />
        /// -> <![CDATA[CORS]]> is defined to accept requests from the specified origins <br />
        /// -> ASP.NET Controllers are added <br />
        /// -> JWT Authentication is set up <br />
        /// -> The Infrastructure and Application layers are added <br />
        /// -> If we're in development, we add OpenAPI support.
        /// </summary>
        private static ValueTask AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(cfg =>
            {
                var corsOrigins = builder.Configuration[Constants.CORS_ORIGINS]?.Split(',') ?? Array.Empty<string>();
                cfg.AddPolicy(name: "local", policy =>
                {
                    policy.WithExposedHeaders("Content-Range", "Accept-Ranges", "Set-Cookie")
                    .WithOrigins(corsOrigins)
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithMethods("GET", "POST", "DELETE", "OPTIONS");
                });
            });

            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.DefaultInvalidModelStateResponse;
            });

            builder.Services.AddAuthentication(o => 
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = true;
                    o.TokenValidationParameters = new()
                    {
                        ValidIssuer = builder.Configuration[Constants.ACCESS_ISSUER],
                        ValidAudience = builder.Configuration[Constants.ACCESS_AUDIENCE],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration[Constants.ACCESS_TOKENKEY]!)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                    o.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["token"];
                            return Task.CompletedTask;
                        }
                    };

                });

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();

            builder.Services.AddSingleton(new APISettings(){ RunningEnvironment = GetRunningEnvironment(builder.Configuration["ASPNETCORE_ENVIRONMENT"]!) });


            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(o =>
                {
                    o.EnableAnnotations();
                    var jwtSecurityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
                    {
                        BearerFormat = "JWT",
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Description = "Please add the bearer token",
                        Name = "JWT Authentication",
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                        Reference = new()
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
                        }
                    };
                    o.AddSecurityDefinition("Bearer", jwtSecurityScheme);
                    o.AddSecurityRequirement(new()
                    {
                        { jwtSecurityScheme, Array.Empty<string>()  }
                    });
                });
            }

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// There's little to do if we don't have all the information we're supposed to use, so let's validate and fail early if needed.
        /// </summary>
        private static bool ValidateEnvironment(IConfiguration configuration)
        {
            if (configuration == null) return false;

            bool validConfig = true;
            foreach (var constant in Constants.GetAllConstantKeys().Where(constant => string.IsNullOrWhiteSpace(configuration[constant])))
            {
                Console.WriteLine($"Environment variable not found: {constant}");
                validConfig = false;
            }

            return validConfig;
        }

        /// <summary>
        /// Helper for the log configuration from environment variables.
        /// </summary>
        private static LoggerConfiguration UseLogLevel(this LoggerConfiguration conf, string level) => level.ToUpper() switch
        {
            "TRACE" => conf.MinimumLevel.Verbose(),
            "DEBUG" => conf.MinimumLevel.Debug(),
            "INFO" => conf.MinimumLevel.Information(),
            "WARN" => conf.MinimumLevel.Warning(),
            "ERROR" => conf.MinimumLevel.Error(),
            _ => conf.MinimumLevel.Information()
        };

        public static RunningEnvironment GetRunningEnvironment(string env) => env.ToUpper() switch 
        {
            "DEVELOPMENT" => RunningEnvironment.Development,
            "PRODUCTION" => RunningEnvironment.Production,
            _ => RunningEnvironment.Unknown
        };

    }
}
