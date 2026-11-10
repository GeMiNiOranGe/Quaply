namespace Quaply.Data.Models;

/// <summary>
/// Entity that supports soft deletion (filtered using a global query filter).
/// </summary>
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
}
