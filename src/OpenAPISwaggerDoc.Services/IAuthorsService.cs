using OpenAPISwaggerDoc.Entities;

namespace OpenAPISwaggerDoc.Services;

public interface IAuthorsService
{
    Task<bool> AuthorExistsAsync(Guid authorId);
    Task<IEnumerable<Author>> GetAuthorsAsync();
    Task<Author> GetAuthorAsync(Guid authorId);
    void UpdateAuthor(Author author);
}