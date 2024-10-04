using MongoDB.Driver;
using System.Reflection;

namespace Guestbooky.Infrastructure.Environment;

/// <summary>
/// One of the main drawbacks of making this project easily settable from the environment was this being a bit convoluted.
/// Organized enough, but could be better.
/// </summary>
public static class Constants
{
    public const string ACCESS_USERNAME = "ACCESS_USERNAME";
    public const string ACCESS_PASSWORD = "ACCESS_PASSWORD";
    public const string ACCESS_TOKENKEY = "ACCESS_TOKENKEY";
    public const string ACCESS_ISSUER = "ACCESS_ISSUER";
    public const string ACCESS_AUDIENCE = "ACCESS_AUDIENCE";
    public const string CLOUDFLARE_SECRET = "CLOUDFLARE_SECRET";
    public const string MONGODB_CONNECTIONSTRING = "MONGODB_CONNECTIONSTRING";
    public const string MONGODB_DATABASENAME = "MONGODB_DATABASENAME";
    public const string CORS_ORIGINS = "CORS_ORIGINS";
    public const string LOG_LEVEL = "LOG_LEVEL";

    public static List<string> GetAllConstantKeys()
    {
        var fields = typeof(Constants).GetFields(BindingFlags.Public | BindingFlags.Static).Where(field => field.FieldType == typeof(string));
        return fields.Select(field => field.GetValue(null)?.ToString() ?? string.Empty).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
    }
}
