using OpenAPISwaggerDoc.Entities;

namespace OpenAPISwaggerDoc.Services;

public interface IBooksService
{
    Task<IEnumerable<Book>> GetBooksAsync(Guid authorId);
    Task<Book> GetBookAsync(Guid authorId, Guid bookId);
    void AddBook(Book bookToAdd);
}