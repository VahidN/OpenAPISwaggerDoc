using Microsoft.EntityFrameworkCore;
using OpenAPISwaggerDoc.DataLayer.Context;
using OpenAPISwaggerDoc.Entities;

namespace OpenAPISwaggerDoc.Services;

public class AuthorsService : IAuthorsService
{
    private readonly DbSet<Author> _authors;
    private readonly IUnitOfWork _uow;

    public AuthorsService(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _authors = uow.Set<Author>();
    }

    public async Task<bool> AuthorExistsAsync(Guid authorId)
    {
        return await _authors.AnyAsync(a => a.Id == authorId);
    }

    public async Task<IEnumerable<Author>> GetAuthorsAsync() => await _authors.ToListAsync();

    public async Task<Author> GetAuthorAsync(Guid authorId)
    {
        if (authorId == Guid.Empty)
        {
            throw new ArgumentException("authorId is empty", nameof(authorId));
        }

        return await _authors
                   .FirstOrDefaultAsync(a => a.Id == authorId);
    }

    public void UpdateAuthor(Author author)
    {
        // no code in this implementation
    }
}