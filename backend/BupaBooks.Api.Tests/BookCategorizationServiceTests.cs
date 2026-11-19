using BupaBooks.Api.Models;
using BupaBooks.Api.Services;

namespace BupaBooks.Api.Tests;

public class BookCategorizationServiceTests
{
    private readonly BookCategorizationService _sut = new();

    [Fact]
    public void Categorize_GroupsAtAge18Boundary_AndSortsAlphabetically()
    {
        var input = new List<ExternalBookOwnerDto>
        {
            new("AdultBoundary", 18, [new("Zebra", "Paperback"), new("Alpha", "Hardcover")]),
            new("ChildBoundary", 17, [new("Charlie", "Hardcover"), new("Bravo", "Paperback")]),
            new("NullBooksOwner", 30, null)
        };

        var result = _sut.Categorize(input, hardcoverOnly: false);

        Assert.Equal(["Alpha", "Zebra"], result.Adults);
        Assert.Equal(["Bravo", "Charlie"], result.Children);
    }

    [Fact]
    public void Categorize_WhenHardcoverOnlyIsTrue_FiltersCaseInsensitively()
    {
        var input = new List<ExternalBookOwnerDto>
        {
            new("Alice", 25, [new("Book A", "hardcover"), new("Book B", "Paperback")]),
            new("Bob", 10, [new("Book C", "Hardcover"), new("Book D", "Ebook")])
        };

        var result = _sut.Categorize(input, hardcoverOnly: true);

        Assert.Equal(["Book A"], result.Adults);
        Assert.Equal(["Book C"], result.Children);
    }

    [Fact]
    public void Categorize_WhenOwnersIsNull_ReturnsEmptyCollections()
    {
        var result = _sut.Categorize(null, hardcoverOnly: false);

        Assert.Empty(result.Adults);
        Assert.Empty(result.Children);
    }
}
