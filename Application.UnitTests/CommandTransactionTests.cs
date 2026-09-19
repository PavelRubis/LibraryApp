using Application.Common;
using Application.CommandHandlers.Authors.Create;
using Application.Dependencies.DataAccess;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Application.UnitTests;

public sealed class CommandTransactionTests
{
    [Fact]
    public async Task SuccessfulCommandCommitsTransaction()
    {
        var repository = Substitute.For<IAuthorRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateAuthorCommandHandler>>();
        unitOfWork.Authors.Returns(repository);
        repository.AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>()).Returns(call => InitializeAuthor(call.Arg<Author>()));
        var handler = new CreateAuthorCommandHandler(unitOfWork, logger);

        var result = await handler.Handle(new CreateAuthorCommand("Martin Fowler"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        await unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FailedCommandRollsBackTransaction()
    {
        var repository = Substitute.For<IAuthorRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateAuthorCommandHandler>>();
        unitOfWork.Authors.Returns(repository);
        repository.AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>()).Returns<Task<Author>>(_ => throw new InvalidOperationException("Database failed"));
        var handler = new CreateAuthorCommandHandler(unitOfWork, logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreateAuthorCommand("Martin Fowler"), CancellationToken.None));

        await unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RepositoryConflictIsRolledBack()
    {
        var repository = Substitute.For<IAuthorRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateAuthorCommandHandler>>();
        unitOfWork.Authors.Returns(repository);
        repository.AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>()).Returns<Task<Author>>(_ => throw new ConflictException("Duplicate"));
        var handler = new CreateAuthorCommandHandler(unitOfWork, logger);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new CreateAuthorCommand("Duplicate"), CancellationToken.None));

        await unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    private static Author InitializeAuthor(Author author)
    {
        author.CreatedAt = DateTime.UtcNow;
        author.RowVersion = new byte[8];
        return author;
    }
}
