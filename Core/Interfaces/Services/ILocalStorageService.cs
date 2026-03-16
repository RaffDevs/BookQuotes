using System;

namespace BookQuotes.Core.Interfaces.Services;

public interface ILocalStorageService
{
    ValueTask SetItemAsync<T>(string key, T value);
    ValueTask<T?> GetItemAsync<T>(string key);
    ValueTask RemoveItemAsync(string key);
}
