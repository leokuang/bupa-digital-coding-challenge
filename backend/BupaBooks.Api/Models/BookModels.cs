namespace BupaBooks.Api.Models;

public record ExternalBookDto(string? Name, string? Type);

public record ExternalBookOwnerDto(
    string? Name,
    int Age,
    List<ExternalBookDto>? Books
);

public record CategorizedBooksResponse(
    IReadOnlyList<string> Adults,
    IReadOnlyList<string> Children
);
