using Microsoft.EntityFrameworkCore;
using OpenAPISwaggerDoc.DataLayer.Context;
using OpenAPISwaggerDoc.Entities;

namespace OpenAPISwaggerDoc.Services;

public class BooksService : IBooksService
{
    private readonly DbSet<Book> _books;
    private readonly IUnitOfWork _uow;

    public BooksService(IUnitOfWork uow)
    {
        _uow = uow;
        _books = _uow.Set<Book>();
    }

    public async Task<IEnumerable<Book>> GetBooksAsync(Guid authorId)
    {
        if (authorId == Guid.Empty)
        {
            throw new ArgumentException("authorId is empty", nameof(authorId));
        }

        return await _books
                     .Include(b => b.Author)
                     .Where(b => b.AuthorId == authorId)
                     .ToListAsync();
    }

    public async Task<Book> GetBookAsync(Guid authorId, Guid bookId)
    {
        if (authorId == Guid.Empty)
        {
            throw new ArgumentException("authorId is empty", nameof(authorId));
        }

        if (bookId == Guid.Empty)
        {
            throw new ArgumentException("bookId is empty", nameof(bookId));
        }

        return await _books
                     .Include(b => b.Author)
                     .Where(b => b.AuthorId == authorId && b.Id == bookId)
                     .FirstOrDefaultAsync();
    }

    public void AddBook(Book bookToAdd)
    {
        if (bookToAdd == null)
        {
            throw new ArgumentNullException(nameof(bookToAdd));
        }

        _books.Add(bookToAdd);
    }
}