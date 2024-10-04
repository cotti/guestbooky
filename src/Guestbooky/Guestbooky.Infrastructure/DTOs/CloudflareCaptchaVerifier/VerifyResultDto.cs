using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Guestbooky.Infrastructure.DTOs.CloudflareCaptchaVerifier;

public record VerifyResultDto
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("challenge_ts")]
    public DateTimeOffset? ChallengeTimestamp { get; init; }

    [JsonPropertyName("hostname")]
    public string? Hostname { get; init; }

    [JsonPropertyName("error-codes")]
    public List<string>? ErrorCodes { get; init; }

    [JsonPropertyName("action")]
    public string? Action { get; init; }

    [JsonPropertyName("cdata")]
    public string? CData { get; init; }
}