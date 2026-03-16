using System;
using BookQuotes.Core.Interfaces.Services;
using Microsoft.JSInterop;

namespace BookQuotes.Infrastructure.Services;

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask<T?> GetItemAsync<T>(string key)
    {
        return _jsRuntime.InvokeAsync<T?>("localStorageAdapter.getItem", key);
    }

    public ValueTask RemoveItemAsync(string key)
    {
        return _jsRuntime.InvokeVoidAsync("localStorageAdapter.removeItem", key);    
    }

    public ValueTask SetItemAsync<T>(string key, T value)
    {
        return _jsRuntime.InvokeVoidAsync("localStorageAdapter.setItem", key, value);
    }
}
