using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OpenAPISwaggerDoc.DataLayer.Context;
using OpenAPISwaggerDoc.Models;
using OpenAPISwaggerDoc.Services;

namespace OpenAPISwaggerDoc.Web.Controllers;

[Route("api/authors")]
[Produces("application/json", "application/xml")]
[ApiController]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorsService _authorsService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public AuthorsController(
        IAuthorsService authorsService,
        IMapper mapper,
        IUnitOfWork uow)
    {
        _authorsService = authorsService;
        _mapper = mapper;
        _uow = uow;
    }

    /// <summary>
    ///     Get a list of authors
    /// </summary>
    /// <returns>An ActionResult of type IEnumerable of Author</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
    {
        var authorsFromRepo = await _authorsService.GetAuthorsAsync();
        return Ok(_mapper.Map<IEnumerable<Author>>(authorsFromRepo));
    }

    /// <summary>
    ///     Get an author by his/her id
    /// </summary>
    /// <param name="authorId">The id of the author you want to get</param>
    /// <returns>An ActionResult of type Author</returns>
    [HttpGet("{authorId}")]
    public async Task<ActionResult<Author>> GetAuthor(Guid authorId)
    {
        var authorFromRepo = await _authorsService.GetAuthorAsync(authorId);
        if (authorFromRepo == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<Author>(authorFromRepo));
    }

    /// <summary>
    ///     Update an author
    /// </summary>
    /// <param name="authorId">The id of the author to update</param>
    /// <param name="authorForUpdate">The author with updated values</param>
    /// <returns>An ActionResult of type Author</returns>
    [HttpPut("{authorId}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Author>> UpdateAuthor(Guid authorId, AuthorForUpdate authorForUpdate)
    {
        var authorFromRepo = await _authorsService.GetAuthorAsync(authorId);
        if (authorFromRepo == null)
        {
            return NotFound();
        }

        _mapper.Map(authorForUpdate, authorFromRepo);

        // update & save
        _authorsService.UpdateAuthor(authorFromRepo);
        await _uow.SaveChangesAsync();

        // return the author
        return Ok(_mapper.Map<Author>(authorFromRepo));
    }

    /// <summary>
    ///     Partially update an author
    /// </summary>
    /// <param name="authorId">The id of the author you want to get</param>
    /// <param name="patchDocument">The set of operations to apply to the author</param>
    /// <returns>An ActionResult of type Author</returns>
    /// <remarks>
    ///     Sample request (this request updates the author's **first name**)
    ///     PATCH /authors/id
    ///     [
    ///     {
    ///     "op": "replace",
    ///     "path": "/firstname",
    ///     "value": "new first name"
    ///     }
    ///     ]
    /// </remarks>
    [HttpPatch("{authorId}")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(422)]
    public async Task<ActionResult<Author>> UpdateAuthor(Guid authorId,
                                                         JsonPatchDocument<AuthorForUpdate> patchDocument)
    {
        var authorFromRepo = await _authorsService.GetAuthorAsync(authorId);
        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // map to DTO to apply the patch to
        var author = _mapper.Map<AuthorForUpdate>(authorFromRepo);
        patchDocument.ApplyTo(author, ModelState);

        // if there are errors when applying the patch the patch doc
        // was badly formed  These aren't caught via the ApiController
        // validation, so we must manually check the modelstate and
        // potentially return these errors.
        if (!ModelState.IsValid)
        {
            return new UnprocessableEntityObjectResult(ModelState);
        }

        // map the applied changes on the DTO back into the entity
        _mapper.Map(author, authorFromRepo);

        // update & save
        _authorsService.UpdateAuthor(authorFromRepo);
        await _uow.SaveChangesAsync();

        // return the author
        return Ok(_mapper.Map<Author>(authorFromRepo));
    }
}