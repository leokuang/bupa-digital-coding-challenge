using System.Net;
using System.Text.Json;
using BupaBooks.Api.Models;
using BupaBooks.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BupaBooks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(
    IBupaBooksClient booksClient,
    IBookCategorizationService categorizationService,
    ILogger<BooksController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CategorizedBooksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<CategorizedBooksResponse>> GetBooks(
        [FromQuery] bool hardcoverOnly = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var owners = await booksClient.GetBookOwnersAsync(cancellationToken);
            var response = categorizationService.Categorize(owners, hardcoverOnly);
            return Ok(response);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            logger.LogWarning(ex, "Upstream Bupa bookowners API rate limit exceeded (429).");
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "The Bupa book service is currently rate-limiting requests. Please wait a moment and try again."
            });
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogError(ex, "Upstream Bupa bookowners API call failed.");
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Unable to retrieve books from the Bupa service."
            });
        }
    }
}
