using System.Globalization;
using System.Text;
using ProductService.Contracts;
using ProductService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class ProductService
{
    public async Task<IEnumerable<ProductResponse>> GetRelatedByFavoriteIdsAsync(
        IEnumerable<string> favoriteIds,
        int limit = 10,
        int? maxDistance = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedFavoriteIds = favoriteIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalizedFavoriteIds.Count == 0 || limit <= 0)
        {
            return [];
        }

        var favoriteProducts = await _dbContext.Products
            .AsNoTracking()
            .Where(product => normalizedFavoriteIds.Contains(product.Id))
            .ToListAsync(cancellationToken);

        if (favoriteProducts.Count == 0)
        {
            return [];
        }

        var favoriteIdSet = new HashSet<string>(
            favoriteProducts.Select(product => product.Id),
            StringComparer.Ordinal);

        var favoriteCategoryIds = favoriteProducts
            .Select(product => product.CategoryId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var favoriteNames = favoriteProducts
            .Select(product => NormalizeForComparison(product.Name))
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        if (favoriteNames.Count == 0)
        {
            return [];
        }

        var candidateProducts = await _dbContext.Products
            .AsNoTracking()
            .Where(product =>
                favoriteCategoryIds.Contains(product.CategoryId) &&
                !favoriteIdSet.Contains(product.Id))
            .ToListAsync(cancellationToken);

        var rankedCandidates = candidateProducts
            .Select(product =>
            {
                var candidateName = NormalizeForComparison(product.Name);
                var bestMatch = GetBestMatchScore(candidateName, favoriteNames);

                return new RankedProduct(product, bestMatch.Distance, bestMatch.Similarity);
            })
            .Where(ranked => maxDistance is null || ranked.Distance <= maxDistance.Value)
            .OrderByDescending(ranked => ranked.Similarity)
            .ThenBy(ranked => ranked.Distance)
            .ThenBy(ranked => ranked.Product.Name, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .Select(ranked => MapToResponse(ranked.Product));

        return rankedCandidates;
    }

    private static (int Distance, double Similarity) GetBestMatchScore(
        string candidateName,
        List<string> favoriteNames)
    {
        var bestDistance = int.MaxValue;
        var bestSimilarity = double.MinValue;

        foreach (var favoriteName in favoriteNames)
        {
            var distance = CalculateLevenshteinDistance(candidateName, favoriteName);
            var longestNameLength = Math.Max(candidateName.Length, favoriteName.Length);
            var similarity = longestNameLength == 0
                ? 1d
                : 1d - (double)distance / longestNameLength;

            if (similarity > bestSimilarity || (Math.Abs(similarity - bestSimilarity) < 0.000001 && distance < bestDistance))
            {
                bestSimilarity = similarity;
                bestDistance = distance;
            }
        }

        return (bestDistance, bestSimilarity);
    }

    private static string NormalizeForComparison(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var previousWasSpace = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                if (previousWasSpace)
                {
                    continue;
                }

                builder.Append(' ');
                previousWasSpace = true;
                continue;
            }

            builder.Append(character);
            previousWasSpace = false;
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).Trim();
    }

    private static int CalculateLevenshteinDistance(string source, string target)
    {
        var sourceLength = source.Length;
        var targetLength = target.Length;

        if (sourceLength == 0)
        {
            return targetLength;
        }

        if (targetLength == 0)
        {
            return sourceLength;
        }

        var matrix = new int[sourceLength + 1, targetLength + 1];

        for (var i = 0; i <= sourceLength; i++)
        {
            matrix[i, 0] = i;
        }

        for (var j = 0; j <= targetLength; j++)
        {
            matrix[0, j] = j;
        }

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[sourceLength, targetLength];
    }

    private sealed record RankedProduct(ProductEntity Product, int Distance, double Similarity);
}
