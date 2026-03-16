using System;
using BookQuotes.Core.Interfaces;
using BookQuotes.Core.Interfaces.Services;
using BookQuotes.Core.Models;

namespace BookQuotes.Infrastructure.Storage;

public class HighlightRepository : IHighlightRepository
{

    private const string StorageKey = "bookquotes.highlights";
    private readonly ILocalStorageService _localStorageService;

    public HighlightRepository(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }
   public async Task<IReadOnlyList<Highlight>> GetByBookIdAsync(Guid bookId)
    {
        var highlights = await _localStorageService.GetItemAsync<List<Highlight>>(StorageKey) ?? [];

        return highlights
            .Where(highlight => highlight.BookId == bookId)
            .OrderByDescending(highlight => highlight.CreatedAtUtc)
            .ToList();
    }

    public async Task AddAsync(Highlight highlight)
    {
        var highlights = await _localStorageService.GetItemAsync<List<Highlight>>(StorageKey) ?? [];
        highlights.Add(highlight);

        await _localStorageService.SetItemAsync(StorageKey, highlights);
    }

    public async Task DeleteAsync(Guid id)
    {
        var highlights = await _localStorageService.GetItemAsync<List<Highlight>>(StorageKey) ?? [];
        highlights.RemoveAll(highlight => highlight.Id == id);

        await _localStorageService.SetItemAsync(StorageKey, highlights);
    } 
}
