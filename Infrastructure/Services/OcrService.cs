using BookQuotes.Core.Interfaces.Services;
using Microsoft.JSInterop;

namespace BookQuotes.Infrastructure.Services;

public class OcrService : IOcrService
{
    private readonly IJSRuntime _jsRuntime;

    public OcrService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public Task<string> ExtractTextFromElementAsync(string elementId, string language = "por")
    {
        return _jsRuntime.InvokeAsync<string>(
            "bookQuotesOcr.extractTextFromElement",
            elementId,
            language).AsTask();
    }
}
