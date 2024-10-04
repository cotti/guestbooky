namespace Guestbooky.API.DTOs.Auth;

public record RefreshTokenRequestDto(string RefreshToken);

public record RefreshTokenResponseDto(string Token, string RefreshToken);