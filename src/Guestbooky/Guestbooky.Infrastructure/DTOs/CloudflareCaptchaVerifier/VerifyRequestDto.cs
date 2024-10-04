using System.Text.Json.Serialization;

namespace Guestbooky.Infrastructure.DTOs.CloudflareCaptchaVerifier;
public record VerifyRequestDto
{
    [JsonPropertyName("secret")]
    public string Secret { get; init; } = default!;

    [JsonPropertyName("response")]
    public string Response { get; init; } = default!;

    [JsonPropertyName("remoteip")]
    public string? RemoteIp { get; init; }

    [JsonPropertyName("idempotency_key")]
    public string? IdempotencyKey { get; init; }
}
