using System.Net.Http.Headers;
using System.Text.Json;
using BupaBooks.Api.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BupaBooks.Api.Services;

public interface IBupaBooksClient
{
    Task<IReadOnlyList<ExternalBookOwnerDto>> GetBookOwnersAsync(CancellationToken cancellationToken = default);
}

public class BupaBooksClient(
    HttpClient httpClient,
    IMemoryCache cache,
    ILogger<BupaBooksClient> logger) : IBupaBooksClient
{
    private const string CacheKey = "BupaBookOwners";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<ExternalBookOwnerDto>> GetBookOwnersAsync(
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyList<ExternalBookOwnerDto>? cachedOwners) &&
            cachedOwners is not null)
        {
            return cachedOwners;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, string.Empty);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("BupaBooksApp/1.0");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            logger.LogWarning("Upstream Bupa API returned an empty response body (Status: {StatusCode}).", response.StatusCode);
            return [];
        }

        var owners = JsonSerializer.Deserialize<List<ExternalBookOwnerDto>>(content, JsonOptions) ?? [];

        if (owners.Count > 0)
        {
            cache.Set(CacheKey, (IReadOnlyList<ExternalBookOwnerDto>)owners, TimeSpan.FromSeconds(30));
        }

        return owners;
    }
}
