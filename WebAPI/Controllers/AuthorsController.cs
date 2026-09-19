using Application.Common;
using Application.CommandHandlers.Authors.Create;
using Application.CommandHandlers.Authors.Delete;
using Application.CommandHandlers.Authors.Update;
using Application.Dto;
using Application.QueryHandlers.Authors.GetById;
using Application.QueryHandlers.Authors.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/authors")]
public sealed class AuthorsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AuthorDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthorDto>> Create(
        CreateAuthorRequest request,
        CancellationToken cancellationToken)
    {
        var author = await sender.Send(new CreateAuthorCommand(request.Name), cancellationToken);
        Response.Headers.ETag = ETagHelper.Format(author.RowVersion);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AuthorDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthorDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var author = await sender.Send(new GetAuthorByIdQuery(id), cancellationToken);
        Response.Headers.ETag = ETagHelper.Format(author.RowVersion);
        return Ok(author);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<AuthorDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuthorDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await sender.Send(new SearchAuthorsQuery(q, page, pageSize), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<AuthorDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthorDto>> Update(
        Guid id,
        UpdateAuthorRequest request,
        CancellationToken cancellationToken)
    {
        var author = await sender.Send(new UpdateAuthorCommand(id, request.Name, request.RowVersion), cancellationToken);
        Response.Headers.ETag = ETagHelper.Format(author.RowVersion);
        return Ok(author);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAuthorCommand(id, ETagHelper.Parse(ifMatch)), cancellationToken);
        return NoContent();
    }
}
