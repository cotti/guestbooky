namespace Guestbooky.API.DTOs.Auth;

public record LoginRequestDto(string Username, string Password);

public record LoginResponseDto(string Token, string RefreshToken);