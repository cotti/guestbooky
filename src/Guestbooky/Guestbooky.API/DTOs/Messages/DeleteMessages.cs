using System.Text.Json.Serialization;

namespace Guestbooky.API.DTOs.Messages;

public record DeleteMessageRequestDto
{
    [JsonPropertyName("Id")]
    public required string Id { get; init; }
}