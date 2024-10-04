using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Guestbooky.API.DTOs.Messages;

public record InsertMessageRequestDto
{
    [JsonPropertyName("author")]
    [Required(ErrorMessage = "The author field is required")]
    [StringLength(200, ErrorMessage = "Author cannot be longer than 200 characters")]
    public string Author { get; init; }

    [JsonPropertyName("message")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "The message field is required")]
    [StringLength(4096, ErrorMessage = "Author cannot be longer than 4096 characters")]
    public string Message { get; init; }

    [JsonPropertyName("captchaResponse")]
    [Required(ErrorMessage = "The captcha challenge response is required")]
    [StringLength(1024)]
    public string CaptchaResponse { get; init; }
}