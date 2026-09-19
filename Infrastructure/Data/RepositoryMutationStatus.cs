using Application.Common;

namespace Infrastructure.Data;

internal enum RepositoryMutationStatus
{
    Success = 0,
    NotFound = 1,
    Conflict = 2,
    InvalidReference = 3
}

internal static class RepositoryMutationStatusExtensions
{
    public static void EnsureSucceeded(this RepositoryMutationStatus status, string entityName)
    {
        switch (status)
        {
            case RepositoryMutationStatus.Success:
                return;
            case RepositoryMutationStatus.NotFound:
                throw new NotFoundException($"{entityName} was not found.");
            case RepositoryMutationStatus.Conflict:
                throw new ConflictException($"{entityName} has changed or conflicts with an existing record.");
            case RepositoryMutationStatus.InvalidReference:
                throw new ConflictException($"{entityName} contains a missing or deleted reference.");
            default:
                throw new InvalidOperationException($"Unexpected mutation status for {entityName}.");
        }
    }

    public static void EnsureRemoved(this RepositoryMutationStatus status, string entityName)
    {
        switch (status)
        {
            case RepositoryMutationStatus.Success:
                return;
            case RepositoryMutationStatus.NotFound:
                throw new NotFoundException($"{entityName} was not found.");
            case RepositoryMutationStatus.Conflict:
                throw new ConflictException($"{entityName} has changed or is still in use.");
            default:
                throw new InvalidOperationException($"Unexpected removal status for {entityName}.");
        }
    }
}
