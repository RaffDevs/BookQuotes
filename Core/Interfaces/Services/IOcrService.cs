namespace BookQuotes.Core.Interfaces.Services;

public interface IOcrService
{
    Task<string> ExtractTextFromElementAsync(string elementId, string language = "por");
}
