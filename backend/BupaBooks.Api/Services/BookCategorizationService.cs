using BupaBooks.Api.Models;

namespace BupaBooks.Api.Services;

public interface IBookCategorizationService
{
    CategorizedBooksResponse Categorize(
        IEnumerable<ExternalBookOwnerDto>? owners,
        bool hardcoverOnly);
}

public class BookCategorizationService : IBookCategorizationService
{
    private const int AdultAgeThreshold = 18;

    public CategorizedBooksResponse Categorize(
        IEnumerable<ExternalBookOwnerDto>? owners,
        bool hardcoverOnly)
    {
        if (owners is null)
        {
            return new CategorizedBooksResponse([], []);
        }

        var adults = new List<string>();
        var children = new List<string>();

        foreach (var owner in owners)
        {
            if (owner.Books is null) continue;

            var targetBucket = owner.Age >= AdultAgeThreshold ? adults : children;

            foreach (var book in owner.Books)
            {
                if (string.IsNullOrWhiteSpace(book.Name)) continue;

                if (hardcoverOnly &&
                    !string.Equals(book.Type?.Trim(), "Hardcover", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                targetBucket.Add(book.Name.Trim());
            }
        }

        adults.Sort(StringComparer.OrdinalIgnoreCase);
        children.Sort(StringComparer.OrdinalIgnoreCase);

        return new CategorizedBooksResponse(adults, children);
    }
}
