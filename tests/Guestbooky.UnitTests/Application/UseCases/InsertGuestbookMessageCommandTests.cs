using FluentValidation;
using FluentValidation.Results;
using Guestbooky.Application.Interfaces;
using Guestbooky.Application.UseCases.InsertGuestMessage;
using Guestbooky.Domain.Abstractions.Repositories;
using Guestbooky.Domain.Entities.Message;
using MediatR.Pipeline;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.UnitTests.Application.UseCases;

public static class InsertGuestbookMessageTests
{
    public class CommandHandlerTests
    {
        private readonly Mock<IGuestbookMessageRepository> _repositoryMock;
        private readonly InsertGuestbookMessageCommandHandler _handler;

        public CommandHandlerTests()
        {
            _repositoryMock = new Mock<IGuestbookMessageRepository>();
            _handler = new InsertGuestbookMessageCommandHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_InsertsMessage()
        {
            // Arrange
            var command = new InsertGuestbookMessageCommand("Asdrubal", "Perdi!", "captcha-token");
            GuestbookMessage? capturedMessage = null;

            _repositoryMock.Setup(x => x.AddAsync(It.IsAny<GuestbookMessage>(), It.IsAny<CancellationToken>()))
                .Callback<GuestbookMessage, CancellationToken>((message, _) => capturedMessage = message)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.ErrorMessage!);

            Assert.NotNull(capturedMessage);
            Assert.Equal("Asdrubal", capturedMessage.Author);
            Assert.Equal("Perdi!", capturedMessage.Message);
            Assert.True(capturedMessage.Id != Guid.Empty);
            Assert.True(capturedMessage.Timestamp <= DateTimeOffset.UtcNow);

            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<GuestbookMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class InsertGuestbookMessageCommandValidatorTests
    {
        private readonly Mock<ICaptchaVerifier> _captchaVerifierMock;
        private readonly InsertGuestbookMessageCommandValidator _validator;

        public InsertGuestbookMessageCommandValidatorTests()
        {
            _captchaVerifierMock = new Mock<ICaptchaVerifier>();
            _validator = new InsertGuestbookMessageCommandValidator(_captchaVerifierMock.Object);
        }

        [Fact]
        public async Task Validate_WithValidCommand_PassesValidation()
        {
            // Arrange
            var command = new InsertGuestbookMessageCommand("Author", "Message", "valid-captcha");

            _captchaVerifierMock.Setup(x => x.VerifyAsync("valid-captcha", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("", "Message", "captcha", "An author is required.")]
        [InlineData("Author", "", "captcha", "A message is required.")]
        [InlineData("Author", "Message", "", "A captcha response must be sent with the payload.")]
        public async Task Validate_WithMissingRequiredFields_ReturnsExpectedErrors(string author, string message, string captcha, string expectedError)
        {
            // Arrange
            var command = new InsertGuestbookMessageCommand(author, message, captcha);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == expectedError);
        }

        [Fact]
        public async Task Validate_LongAuthor_ReturnsError()
        {
            // Arrange
            var command = new InsertGuestbookMessageCommand(new string('A', 201),  "Message", "captcha");

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "The author field should not exceed 200 characters.");
        }

        [Fact]
        public async Task Validate_LongMessage_ReturnsError()
        {
            // Arrange
            var command = new InsertGuestbookMessageCommand("Author", new string('A', 4097), "captcha");

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "The message field should not exceed 4096 characters.");
        }

        [Fact]
        public async Task Validate_InvalidCaptcha_ReturnsError()
        {
            // Arrange
            var command = new InsertGuestbookMessageCommand("Author", "Message", "invalid-captcha");

            _captchaVerifierMock.Setup(x => x.VerifyAsync("invalid-captcha", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "The captcha challenge response was not accepted.");
        }
    }

    public class InsertGuestbookMessageCommandExceptionHandlerTests
    {
        private readonly InsertGuestbookMessageCommandExceptionHandler _handler;

        public InsertGuestbookMessageCommandExceptionHandlerTests()
        {
            _handler = new InsertGuestbookMessageCommandExceptionHandler();
        }

        [Fact]
        public async Task Handle_WithValidationException_SetsErrorResult()
        {
            // Arrange
            var command = new InsertGuestbookMessageCommand("Author", "Message", "captcha");
            var validationFailures = new[]
            {
                new ValidationFailure("Author", "First error"),
                new ValidationFailure("Message", "Second error")
            };
            var exception = new ValidationException(validationFailures);
            var state = new RequestExceptionHandlerState<InsertGuestbookMessageResult>();

            // Act
            await _handler.Handle(command, exception, state, CancellationToken.None);

            // Assert
            Assert.True(state.Response != null);
            Assert.False(state.Response.Success);
            Assert.Equal("First error Second error", state.Response.ErrorMessage);
        }

        [Fact]
        public async Task Handle_WithEmptyValidationErrors_SetsEmptyErrorMessage()
        {
            // Arrange
            var command = new InsertGuestbookMessageCommand("Author", "Message", "captcha");
            var exception = new ValidationException(Array.Empty<ValidationFailure>());
            var state = new RequestExceptionHandlerState<InsertGuestbookMessageResult>();

            // Act
            await _handler.Handle(command, exception, state, CancellationToken.None);

            // Assert
            Assert.True(state.Response != null);
            Assert.False(state.Response.Success);
            Assert.Empty(state.Response.ErrorMessage);
        }
    }
}