using Guestbooky.Application.Interfaces;
using Guestbooky.Domain.Abstractions.Infrastructure;
using Guestbooky.Domain.Entities.User;
using Guestbooky.Infrastructure.Environment;
using Microsoft.Extensions.Configuration;

namespace Guestbooky.Infrastructure.User;

public class EnvironmentUserCredentialsProvider : IUserCredentialsProvider
{
    private ApplicationUser _user;

    public EnvironmentUserCredentialsProvider(IConfiguration configuration, IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(configuration[Constants.ACCESS_USERNAME]);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(configuration[Constants.ACCESS_PASSWORD]);

        _user = new ApplicationUser(configuration[Constants.ACCESS_USERNAME]!, passwordHasher.HashPassword(configuration[Constants.ACCESS_PASSWORD]!));
    }

    public ApplicationUser GetCredentials() => _user;

    public ValueTask UpdateApplicationUser(ApplicationUser updated)
    {
        _user = updated;
        return ValueTask.CompletedTask;
    }
}