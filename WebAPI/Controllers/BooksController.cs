using Application.Common;
using Application.CommandHandlers.Books.Create;
using Application.CommandHandlers.Books.Delete;
using Application.CommandHandlers.Books.Update;
using Application.Dto;
using Application.QueryHandlers.Books.GetById;
using Application.QueryHandlers.Books.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/books")]
public sealed class BooksController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<BookDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<BookDto>> Create(
        CreateBookRequest request,
        CancellationToken cancellationToken)
    {
        var book = await sender.Send(new CreateBookCommand(request.Title, request.PublicationYear, request.TableOfContentsXml, request.AuthorIds), cancellationToken);
        Response.Headers.ETag = ETagHelper.Format(book.RowVersion);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<BookDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<BookDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var book = await sender.Send(new GetBookByIdQuery(id), cancellationToken);
        Response.Headers.ETag = ETagHelper.Format(book.RowVersion);
        return Ok(book);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<BookListItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BookListItemDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await sender.Send(new SearchBooksQuery(q, page, pageSize), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<BookDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<BookDto>> Update(
        Guid id,
        UpdateBookRequest request,
        CancellationToken cancellationToken)
    {
        var book = await sender.Send(new UpdateBookCommand(id, request.Title, request.PublicationYear, request.TableOfContentsXml, request.AuthorIds, request.RowVersion), cancellationToken);
        Response.Headers.ETag = ETagHelper.Format(book.RowVersion);
        return Ok(book);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteBookCommand(id, ETagHelper.Parse(ifMatch)), cancellationToken);
        return NoContent();
    }
}
