using System.Linq.Expressions;
using ProductService.Contracts;
using ProductService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class ProductService
{
    private const double ExactMatchCoverageThreshold = 0.70d;
    private const double RelatedMatchCoverageThreshold = 0.35d;
    private static readonly HashSet<string> Stopwords = new(StringComparer.Ordinal)
    {
        "a", "o", "as", "os",
        "um", "uma", "uns", "umas",
        "de", "da", "do", "das", "dos",
        "e", "em", "no", "na", "nos", "nas",
        "para", "por", "com", "sem",
        "ao", "aos", "à", "às"
    };

    public async Task<QrCodeLookupResponse> LookupByQrCodeAsync(
        QrCodeLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var barcode = (request.Barcode ?? string.Empty).Trim();
        var keywords = (request.Keywords ?? Array.Empty<string>())
            .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
            .Select(keyword => keyword.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!string.IsNullOrEmpty(barcode))
        {
            var byBarcode = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.Barcode == barcode, cancellationToken);

            if (byBarcode is not null)
            {
                return new QrCodeLookupResponse(
                    MatchType: "exact",
                    Product: MapToResponse(byBarcode),
                    RelatedProducts: []);
            }
        }

        if (keywords.Count == 0)
        {
            return new QrCodeLookupResponse(
                MatchType: "none",
                Product: null,
                RelatedProducts: []);
        }

        var normalizedKeywords = keywords
            .Select(NormalizeForComparison)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalizedKeywords.Count == 0)
        {
            return new QrCodeLookupResponse(
                MatchType: "none",
                Product: null,
                RelatedProducts: []);
        }

        var usefulTokens = normalizedKeywords
            .SelectMany(Tokenize)
            .Where(token => token.Length >= 2)
            .Where(token => !Stopwords.Contains(token))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (usefulTokens.Count == 0)
        {
            return new QrCodeLookupResponse(
                MatchType: "none",
                Product: null,
                RelatedProducts: []);
        }

        var queryText = NormalizeForComparison(string.Join(' ', usefulTokens));

        // First narrow down candidates via SQL (fast), then rank in-memory.
        var candidateQuery = _dbContext.Products.AsNoTracking();
        candidateQuery = candidateQuery.Where(BuildKeywordPredicate(usefulTokens));

        var candidates = await candidateQuery
            .Take(100)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return new QrCodeLookupResponse(
                MatchType: "none",
                Product: null,
                RelatedProducts: []);
        }

        var ranked = candidates
            .Select(product =>
            {
                var compareText = NormalizeForComparison($"{product.Name} {product.Brand}".Trim());
                var (coverage, matchedCount) = GetKeywordCoverage(compareText, usefulTokens);

                var distance = CalculateLevenshteinDistance(compareText, queryText);
                var longest = Math.Max(compareText.Length, queryText.Length);
                var similarity = longest == 0 ? 1d : 1d - (double)distance / longest;

                // Brand is often not in the name; boost when the brand token is present in keywords.
                var brandMatched = IsBrandInKeywords(product.Brand, usefulTokens);
                var brandBoost = brandMatched ? 0.15d : 0d;

                return new RankedLookupProduct(product, coverage, matchedCount, brandMatched, similarity + brandBoost, distance);
            })
            .OrderByDescending(item => item.Coverage)
            .ThenByDescending(item => item.MatchedCount)
            .ThenByDescending(item => item.BrandMatched)
            .ThenByDescending(item => item.Similarity)
            .ThenBy(item => item.Distance)
            .ThenBy(item => item.Product.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var best = ranked[0];
        var isExact =
            best.Coverage >= ExactMatchCoverageThreshold ||
            (best.BrandMatched && best.MatchedCount >= 2) ||
            (best.MatchedCount >= 3 && best.Coverage >= 0.50d);

        if (isExact && best.Distance == 0)
        {
            return new QrCodeLookupResponse(
                MatchType: "exact",
                Product: MapToResponse(best.Product),
                RelatedProducts: []);
        }

        if (isExact)
        {
            return new QrCodeLookupResponse(
                MatchType: "exact",
                Product: MapToResponse(best.Product),
                RelatedProducts: []);
        }

        var related = ranked
            .Where(item => item.Coverage >= RelatedMatchCoverageThreshold || item.Similarity > 0.20d)
            .Take(10)
            .Select(item => MapToResponse(item.Product))
            .ToList();

        return new QrCodeLookupResponse(
            MatchType: related.Count > 0 ? "related" : "none",
            Product: null,
            RelatedProducts: related);
    }

    private static IEnumerable<string> Tokenize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (var token in value
                     .Split(new[] { ' ', '-', '_', '/', '.', ',', ';', ':', '(', ')', '[', ']', '{', '}', '+', '&' },
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var normalized = NormalizeForComparison(token);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                yield return normalized;
            }
        }
    }

    private static (double Coverage, int MatchedCount) GetKeywordCoverage(string candidate, List<string> usefulTokens)
    {
        if (string.IsNullOrWhiteSpace(candidate) || usefulTokens.Count == 0)
        {
            return (0d, 0);
        }

        var matched = 0;
        foreach (var token in usefulTokens)
        {
            if (candidate.Contains(token, StringComparison.Ordinal))
            {
                matched++;
            }
        }

        return ((double)matched / usefulTokens.Count, matched);
    }

    private static bool IsBrandInKeywords(string? brand, List<string> usefulTokens)
    {
        if (string.IsNullOrWhiteSpace(brand) || usefulTokens.Count == 0)
        {
            return false;
        }

        var normalizedBrand = NormalizeForComparison(brand);
        if (string.IsNullOrWhiteSpace(normalizedBrand))
        {
            return false;
        }

        return usefulTokens.Contains(normalizedBrand, StringComparer.Ordinal);
    }

    private static Expression<Func<ProductEntity, bool>> BuildKeywordPredicate(List<string> usefulTokens)
    {
        // Works for both SQLite tests and PostgreSQL prod.
        var parameter = Expression.Parameter(typeof(ProductEntity), "product");

        Expression? body = null;

        var nameProperty = Expression.Property(parameter, nameof(ProductEntity.Name));
        var brandProperty = Expression.Property(parameter, nameof(ProductEntity.Brand));

        foreach (var keyword in usefulTokens)
        {
            var loweredKeyword = keyword.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(loweredKeyword))
            {
                continue;
            }

            var keywordConstant = Expression.Constant(loweredKeyword);

            var nameLower = Expression.Call(nameProperty, nameof(string.ToLower), Type.EmptyTypes);
            var nameContains = Expression.Call(
                nameLower,
                nameof(string.Contains),
                Type.EmptyTypes,
                keywordConstant);

            var brandNotNull = Expression.NotEqual(brandProperty, Expression.Constant(null, typeof(string)));
            var brandLower = Expression.Call(brandProperty, nameof(string.ToLower), Type.EmptyTypes);
            var brandContains = Expression.Call(
                brandLower,
                nameof(string.Contains),
                Type.EmptyTypes,
                keywordConstant);
            var brandPredicate = Expression.AndAlso(brandNotNull, brandContains);

            var keywordPredicate = Expression.OrElse(nameContains, brandPredicate);

            body = body is null ? keywordPredicate : Expression.OrElse(body, keywordPredicate);
        }

        body ??= Expression.Constant(false);
        return Expression.Lambda<Func<ProductEntity, bool>>(body, parameter);
    }

    private sealed record RankedLookupProduct(
        ProductEntity Product,
        double Coverage,
        int MatchedCount,
        bool BrandMatched,
        double Similarity,
        int Distance);
}
