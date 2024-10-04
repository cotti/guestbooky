using Guestbooky.Application.Interfaces;
using Guestbooky.Infrastructure.DTOs.CloudflareCaptchaVerifier;
using Guestbooky.Infrastructure.Environment;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Guestbooky.Infrastructure.Application
{
    /// <summary>
    /// The captcha verification was the first thing I tried to make work, and work it did.
    /// I remember not having <see cref="IHttpClientFactory"/>! Those were the days. I saw socket exhaustion. It's not pretty.
    /// </summary>
    public class CloudflareCaptchaVerifier : ICaptchaVerifier
    {
        private const string VERIFY_ADDRESS = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
        private readonly string _secret;
        private readonly IHttpClientFactory _httpClientFactory;

        public CloudflareCaptchaVerifier(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(configuration[Constants.CLOUDFLARE_SECRET]);

            _secret = configuration[Constants.CLOUDFLARE_SECRET]!;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<bool> VerifyAsync(string challengeResponse, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient("API");

            var request = new VerifyRequestDto() { Secret = _secret, Response = challengeResponse };
            var content = JsonSerializer.Serialize(request);
            using var result = await httpClient.PostAsync(VERIFY_ADDRESS, new StringContent(content, Encoding.UTF8, "application/json"), cancellationToken);
            var serialized = await JsonSerializer.DeserializeAsync<VerifyResultDto>(await result.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);

            return await Task.FromResult(serialized?.Success ?? false);
        }
    }
}
