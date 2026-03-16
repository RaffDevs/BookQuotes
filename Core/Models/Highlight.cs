using System;

namespace BookQuotes.Core.Models;

public class Highlight
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookId { get; set; }
    public string RawText { get; set; } = string.Empty;
    public string FinalText { get; set; } = string.Empty;
    public string? ImageBase64 { get; set; }
    public int? PageNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
