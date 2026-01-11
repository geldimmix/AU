using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AlgoritmaUzmani.Helpers;

public static class SlugHelper
{
    private static readonly Dictionary<char, string> TurkishCharMap = new()
    {
        { 'ç', "c" }, { 'Ç', "c" },
        { 'ğ', "g" }, { 'Ğ', "g" },
        { 'ı', "i" }, { 'I', "i" },
        { 'İ', "i" }, { 'i', "i" },
        { 'ö', "o" }, { 'Ö', "o" },
        { 'ş', "s" }, { 'Ş', "s" },
        { 'ü', "u" }, { 'Ü', "u" }
    };

    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert Turkish characters
        var sb = new StringBuilder();
        foreach (var c in text)
        {
            if (TurkishCharMap.TryGetValue(c, out var replacement))
                sb.Append(replacement);
            else
                sb.Append(c);
        }

        var result = sb.ToString().ToLowerInvariant();

        // Remove diacritics
        result = RemoveDiacritics(result);

        // Replace spaces and special characters with hyphens
        result = Regex.Replace(result, @"[^a-z0-9\s-]", "");
        result = Regex.Replace(result, @"\s+", "-");
        result = Regex.Replace(result, @"-+", "-");

        return result.Trim('-');
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}

