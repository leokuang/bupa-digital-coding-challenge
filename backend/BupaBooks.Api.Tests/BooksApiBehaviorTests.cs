using System.Net;
using System.Text;
using System.Text.Json;
using BupaBooks.Api.Controllers;
using BupaBooks.Api.Models;
using BupaBooks.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace BupaBooks.Api.Tests;

public class BooksApiBehaviorTests
{
    [Fact]
    public async Task GetBookOwnersAsync_ReusesSuccessfulResponseFromCache()
    {
        var handler = new CountingResponseHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://books.example.test/")
        };
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var client = new BupaBooksClient(httpClient, cache, NullLogger<BupaBooksClient>.Instance);

        var firstResponse = await client.GetBookOwnersAsync();
        var secondResponse = await client.GetBookOwnersAsync();

        Assert.Single(firstResponse);
        Assert.Same(firstResponse, secondResponse);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GetBooks_WhenUpstreamIsRateLimited_Returns429WithUserMessage()
    {
        var controller = new BooksController(
            new RateLimitedBooksClient(),
            new BookCategorizationService(),
            NullLogger<BooksController>.Instance);

        var result = await controller.GetBooks();

        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status429TooManyRequests, response.StatusCode);

        using var body = JsonDocument.Parse(JsonSerializer.Serialize(response.Value));
        Assert.Equal(
            "The Bupa book service is currently rate-limiting requests. Please wait a moment and try again.",
            body.RootElement.GetProperty("message").GetString());
    }

    private sealed class CountingResponseHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """[{"name":"Alice","age":30,"books":[]}]""",
                    Encoding.UTF8,
                    "application/json")
            });
        }
    }

    private sealed class RateLimitedBooksClient : IBupaBooksClient
    {
        public Task<IReadOnlyList<ExternalBookOwnerDto>> GetBookOwnersAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<IReadOnlyList<ExternalBookOwnerDto>>(
                new HttpRequestException(
                    "Rate limited",
                    inner: null,
                    statusCode: HttpStatusCode.TooManyRequests));
        }
    }
}
