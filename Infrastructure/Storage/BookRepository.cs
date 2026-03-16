using System;
using BookQuotes.Core.Interfaces;
using BookQuotes.Core.Interfaces.Services;
using BookQuotes.Core.Models;

namespace BookQuotes.Infrastructure.Storage;

public class BookRepository : IBookRepository
{
    private const string StorageKey = "bookquotes.books";
    private readonly ILocalStorageService _localStorageService;

    public BookRepository(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async Task AddAsync(Book book)
    {
        var books = await _localStorageService.GetItemAsync<List<Book>>(StorageKey) ?? [];
        books.Add(book);
        await _localStorageService.SetItemAsync(StorageKey, books);
    }

    public async Task DeleteAsync(Guid id)
    {
        var books = await _localStorageService.GetItemAsync<List<Book>>(StorageKey) ?? [];
        books.RemoveAll(book => book.Id == id);

        await _localStorageService.SetItemAsync(StorageKey, books);
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync()
    {
        return await _localStorageService.GetItemAsync<List<Book>>(StorageKey) ?? [];
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        var books = await _localStorageService.GetItemAsync<List<Book>>(StorageKey) ?? [];
        return books.FirstOrDefault(book => book.Id == id);
    }

    public async Task UpdateAsync(Book book)
    {
        var books = await _localStorageService.GetItemAsync<List<Book>>(StorageKey) ?? [];
        var existingBook = books.FirstOrDefault(item => item.Id == book.Id);

        if (existingBook is null)
        {
            return;
        }

        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.CoverImageUrl = book.CoverImageUrl;

        await _localStorageService.SetItemAsync(StorageKey, books);
    }
}
