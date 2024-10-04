using Guestbooky.Application.Interfaces;
using Guestbooky.Domain.Abstractions.Infrastructure;
using Guestbooky.Domain.Abstractions.Repositories;
using Guestbooky.Infrastructure.Application;
using Guestbooky.Infrastructure.Environment;
using Guestbooky.Infrastructure.Persistence.Configurations;
using Guestbooky.Infrastructure.Persistence.Repositories;
using Guestbooky.Infrastructure.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Guestbooky.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("API", o => 
        {
            o.Timeout = TimeSpan.FromSeconds(10);
        });
        
        services.AddSingleton<MongoDbSettings>(new MongoDbSettings()
        {
            ConnectionString = configuration[Constants.MONGODB_CONNECTIONSTRING]!,
            DatabaseName = configuration[Constants.MONGODB_DATABASENAME]!
        });
        services.AddSingleton<IMongoClient>(o =>
        {
            var settings = o.GetRequiredService<MongoDbSettings>()!;
            return new MongoClient(settings.ConnectionString);
        });
        services.AddScoped<IMongoDatabase>(o =>
        {
            var client = o.GetRequiredService<IMongoClient>();
            var settings = o.GetRequiredService<MongoDbSettings>()!;
            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddScoped<IGuestbookMessageRepository, MongoGuestbookMessageRepository>();

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ICaptchaVerifier, CloudflareCaptchaVerifier>();

        services.AddSingleton<IUserCredentialsProvider, EnvironmentUserCredentialsProvider>();
        return services;
    }
}
