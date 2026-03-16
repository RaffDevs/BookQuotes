using BookQuotes.Core.Interfaces;
using BookQuotes.Core.Interfaces.Services;
using BookQuotes.Core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BookQuotes.Features.Highlights.Pages;

public partial class NewHighlightPage
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    protected const string OcrImageElementId = "ocr-source-image";

    [Inject]
    protected IBookRepository BookRepository { get; set; } = default!;

    [Inject]
    protected IHighlightRepository HighlightRepository { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    protected IOcrService OcrService { get; set; } = default!;

    [Parameter]
    public Guid BookId { get; set; }

    protected bool _isLoading = true;
    protected bool _isProcessingImage;
    protected bool _isCroppingImage;
    protected bool _isProcessingOcr;
    protected bool _isSavingHighlight;
    protected bool _isCameraDrawerOpen;
    protected bool _isEditorDrawerOpen;
    protected Breakpoint _currentBreakpoint = Breakpoint.Md;
    protected Book? _book;
    protected string? _imageBase64;
    protected string? _croppedImageBase64;
    protected string? _selectedFileName;
    protected string _rawExtractedText = string.Empty;
    protected string _finalText = string.Empty;
    protected int? _pageNumber;

    protected bool IsMobile =>
        _currentBreakpoint is Breakpoint.Xs or Breakpoint.Sm or Breakpoint.SmAndDown;

    protected Anchor EditorDrawerAnchor => IsMobile ? Anchor.Bottom : Anchor.End;

    protected string? EditorDrawerWidth => IsMobile ? null : "560px";

    protected string? EditorDrawerHeight => IsMobile ? "92dvh" : null;

    protected string? CameraDrawerWidth => IsMobile ? null : "480px";

    protected string? CameraDrawerHeight => IsMobile ? "100dvh" : null;

    protected string EditorDrawerClass =>
        IsMobile
            ? "highlight-editor-drawer mobile-drawer"
            : "highlight-editor-drawer desktop-drawer";

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _book = await BookRepository.GetByIdAsync(BookId);
        _isLoading = false;
    }

    protected async Task HandleImageSelectedAsync(IBrowserFile file)
    {
        try
        {
            _isProcessingImage = true;
            _selectedFileName = file.Name;

            await using var stream = file.OpenReadStream(maxAllowedSize: MaxFileSize);
            using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream);

            var bytes = memoryStream.ToArray();
            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "image/jpeg"
                : file.ContentType;

            _imageBase64 = $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
            _croppedImageBase64 = null;
            ResetExtractedContent();
            _isCameraDrawerOpen = false;
            _isEditorDrawerOpen = true;
        }
        catch
        {
            Snackbar.Add("Nao foi possivel carregar a imagem selecionada.", Severity.Error);
        }
        finally
        {
            _isProcessingImage = false;
        }
    }

    protected Task HandleCapturedImageAsync(string imageBase64)
    {
        _selectedFileName = $"captura-{DateTime.Now:yyyyMMdd-HHmmss}.jpg";
        _imageBase64 = imageBase64;
        _croppedImageBase64 = null;
        ResetExtractedContent();
        _isCameraDrawerOpen = false;
        _isEditorDrawerOpen = true;

        return Task.CompletedTask;
    }

    protected Task HandleCropStartedAsync()
    {
        _isCroppingImage = true;
        return Task.CompletedTask;
    }

    protected Task HandleCropGeneratedAsync(string croppedImageBase64)
    {
        _croppedImageBase64 = croppedImageBase64;
        _isCroppingImage = false;
        _isEditorDrawerOpen = false;
        ResetExtractedContent();
        return Task.CompletedTask;
    }

    protected Task HandleCropFailedAsync()
    {
        _isCroppingImage = false;
        Snackbar.Add("Nao foi possivel gerar o recorte da imagem.", Severity.Error);
        return Task.CompletedTask;
    }

    protected void GoBack()
    {
        NavigationManager.NavigateTo($"/books/{BookId}");
    }

    protected void OpenEditorDrawer()
    {
        _isEditorDrawerOpen = true;
    }

    protected void OpenCameraDrawer()
    {
        _isCameraDrawerOpen = true;
    }

    protected Task CloseCameraDrawerAsync()
    {
        _isCameraDrawerOpen = false;
        return InvokeAsync(StateHasChanged);
    }

    protected Task CloseEditorDrawerAsync()
    {
        _isEditorDrawerOpen = false;
        return InvokeAsync(StateHasChanged);
    }

    protected Task OnBreakpointChanged(Breakpoint breakpoint)
    {
        _currentBreakpoint = breakpoint;
        return Task.CompletedTask;
    }

    protected async Task ExtractTextAsync()
    {
        if (string.IsNullOrWhiteSpace(_croppedImageBase64))
        {
            return;
        }

        try
        {
            _isProcessingOcr = true;

            var extractedText = await OcrService.ExtractTextFromElementAsync(OcrImageElementId);

            _rawExtractedText = extractedText.Trim();
            _finalText = _rawExtractedText;

            if (string.IsNullOrWhiteSpace(_rawExtractedText))
            {
                Snackbar.Add("Nenhum texto foi identificado na imagem recortada.", Severity.Warning);
                return;
            }

            Snackbar.Add("Texto extraido com sucesso.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Nao foi possivel executar o OCR da imagem. Erro: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isProcessingOcr = false;
        }
    }

    protected Task HandleFinalTextChanged(string value)
    {
        _finalText = value;
        return Task.CompletedTask;
    }

    protected Task HandlePageNumberChanged(int? value)
    {
        _pageNumber = value;
        return Task.CompletedTask;
    }

    protected async Task SaveHighlightAsync()
    {
        if (_book is null || string.IsNullOrWhiteSpace(_croppedImageBase64) || string.IsNullOrWhiteSpace(_finalText))
        {
            return;
        }

        try
        {
            _isSavingHighlight = true;

            var highlight = new Highlight
            {
                BookId = _book.Id,
                RawText = _rawExtractedText,
                FinalText = _finalText.Trim(),
                ImageBase64 = _croppedImageBase64,
                PageNumber = _pageNumber
            };

            await HighlightRepository.AddAsync(highlight);

            Snackbar.Add("Destaque salvo com sucesso.", Severity.Success);
            NavigationManager.NavigateTo($"/books/{BookId}");
        }
        catch
        {
            Snackbar.Add("Nao foi possivel salvar o destaque.", Severity.Error);
        }
        finally
        {
            _isSavingHighlight = false;
        }
    }

    private void ResetExtractedContent()
    {
        _rawExtractedText = string.Empty;
        _finalText = string.Empty;
        _pageNumber = null;
    }
}
