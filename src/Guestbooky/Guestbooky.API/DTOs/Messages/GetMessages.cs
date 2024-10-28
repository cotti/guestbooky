using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Http.Headers;

namespace Guestbooky.API.DTOs.Messages;

[SwaggerSchema("Range", Format = "string", Description = "Optional Range header (e.g., \"messages=1-50\")")]
[SwaggerSubType(typeof(string))]
public record GetMessagesRequestDto
{
    [FromHeader(Name = "Range")]
    public required RangeHeaderValue Range { get; init; }

    public string Unit => Range?.Unit ?? string.Empty;

    public long Offset => Range?.Ranges.FirstOrDefault()?.From ?? 0;
}

public record GetMessagesResponseDto(string Id, string Author, string Message, DateTimeOffset Timestamp);