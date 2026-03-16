using System;
using BookQuotes.Core.Models;

namespace BookQuotes.Core.Interfaces;

public interface IHighlightRepository
{
    Task<IReadOnlyList<Highlight>> GetByBookIdAsync(Guid bookId);
    Task AddAsync(Highlight highlight);
    Task DeleteAsync(Guid id);
}
