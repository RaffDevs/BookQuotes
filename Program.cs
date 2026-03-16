using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookQuotes;
using MudBlazor.Services;
using Cropper.Blazor.Extensions;
using BookQuotes.Core.Interfaces.Services;
using BookQuotes.Infrastructure.Services;
using BookQuotes.Core.Interfaces;
using BookQuotes.Infrastructure.Storage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddCropper();

builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IHighlightRepository, HighlightRepository>();

await builder.Build().RunAsync();
