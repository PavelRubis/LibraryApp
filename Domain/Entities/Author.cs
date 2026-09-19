using Domain.Common;

namespace Domain.Entities;

public class Author : EntityBase
{
    public string Name { get; set; } = string.Empty;
}
