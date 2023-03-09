using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenAPISwaggerDoc.DataLayer.Context;
using OpenAPISwaggerDoc.Models;
using OpenAPISwaggerDoc.Services;

namespace OpenAPISwaggerDoc.Web.Controllers;

[Produces("application/json", "application/xml")]
[Route("api/authors/{authorId}/books")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IAuthorsService _authorsService;
    private readonly IBooksService _booksService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public BooksController(
        IBooksService booksService,
        IAuthorsService authorsService,
        IMapper mapper,
        IUnitOfWork uow)
    {
        _booksService = booksService;
        _authorsService = authorsService;
        _mapper = mapper;
        _uow = uow;
    }

    /// <summary>
    ///     Get the books for a specific author
    /// </summary>
    /// <param name="authorId">The id of the book author</param>
    /// <returns>An ActionResult of type IEnumerable of Book</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks(Guid authorId)
    {
        if (!await _authorsService.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var booksFromRepo = await _booksService.GetBooksAsync(authorId);
        return Ok(_mapper.Map<IEnumerable<Book>>(booksFromRepo));
    }

    /// <summary>
    ///     Get a book by id for a specific author
    /// </summary>
    /// <param name="authorId">The id of the book author</param>
    /// <param name="bookId">The id of the book</param>
    /// <returns>An ActionResult of type Book</returns>
    /// <response code="200">Returns the requested book</response>
    [HttpGet("{bookId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK /*, Type = typeof(Book)*/)]
    public async Task<ActionResult<Book>> GetBook(Guid authorId, Guid bookId)
    {
        if (!await _authorsService.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var bookFromRepo = await _booksService.GetBookAsync(authorId, bookId);
        if (bookFromRepo == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<Book>(bookFromRepo));
    }


    /// <summary>
    ///     Create a book for a specific author
    /// </summary>
    /// <param name="authorId">The id of the book author</param>
    /// <param name="bookForCreation">The book to create</param>
    /// <returns>An ActionResult of type Book</returns>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Book>> CreateBook(Guid authorId, [FromBody] BookForCreation bookForCreation)
    {
        if (!await _authorsService.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var bookToAdd = _mapper.Map<Entities.Book>(bookForCreation);
        _booksService.AddBook(bookToAdd);
        await _uow.SaveChangesAsync();

        return CreatedAtRoute(
                              "GetBook",
                              new { authorId, bookId = bookToAdd.Id },
                              _mapper.Map<Book>(bookToAdd));
    }
}