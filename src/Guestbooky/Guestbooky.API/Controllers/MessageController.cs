using Guestbooky.API.DTOs.Messages;
using Guestbooky.Application.UseCases.CountGuestbookMessages;
using Guestbooky.Application.UseCases.DeleteGuestbookMessage;
using Guestbooky.Application.UseCases.InsertGuestMessage;
using Guestbooky.Application.UseCases.ListGuestbookMessages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guestbooky.API.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MessageController> _logger;

    public MessageController(IMediator mediator, ILogger<MessageController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Insert([FromBody] InsertMessageRequestDto message, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Add guestbook message endpoint reached. Sending command.");
            var result = await _mediator.Send(new InsertGuestbookMessageCommand(message.Author, message.Message, message.CaptchaResponse), token);

            if (result.Success) return Created();
            else
            {
                _logger.LogError($"It wasn't possible to add the message. Reason: {result.ErrorMessage}");
                return Problem(result.ErrorMessage, statusCode: StatusCodes.Status403Forbidden);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred upon trying to insert new guestbook entry. Returning server error.");
            return Problem($"An error occurred on the server: {e.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<GetMessagesResponseDto>), StatusCodes.Status206PartialContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status416RequestedRangeNotSatisfiable)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Get([FromHeader(Name = "Range")] GetMessagesRequestDto request, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("List guestbook messages endpoint reached. Sending command.");
            var query = new ListGuestbookMessagesQuery(request.Offset);
            var queryResult = await _mediator.Send(query, token);

            var responseResult = queryResult.Messages.Select(message => new GetMessagesResponseDto(message.Id, message.Author, message.Message, message.Timestamp));

            var totalMessages = await GetMessagesTotalAmount(token);

            Response.Headers.AcceptRanges = "messages";
            Response.Headers.ContentRange = $"messages {request.Offset}-{request.Offset + responseResult.Count() - 1}/{totalMessages}";
            Response.StatusCode = StatusCodes.Status206PartialContent;

            return new ObjectResult(responseResult);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred upon trying to acquire the guestbook entries. Returning server error.");
            return Problem($"An error occurred on the server: {e.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([FromBody] DeleteMessageRequestDto message, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Delete guestbook message endpoint reached. Sending command.");
            var result = await _mediator.Send(new DeleteGuestbookMessageCommand(message.Id), token);

            if (result.Success)
            {
                _logger.LogInformation("Deletion successful. Returning Ok.");
                return Ok();
            }
            else
            {
                _logger.LogError($"An issue occurred upon trying to delete the message. Returning server error.");
                return Problem($"Could not delete the guestbook entry.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred upon trying to delete a guestbook entry. Returning server error.");
            return Problem($"An error occurred on the server: {e.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private async Task<long> GetMessagesTotalAmount(CancellationToken cancellationToken)
    {
        var query = new CountGuestbookMessagesQuery();
        var queryResult = await _mediator.Send(query, cancellationToken);
        return queryResult.Amount;
    }
}
