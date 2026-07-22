namespace Quaply.Data.Interfaces;

public interface IUnitOfWork
{
    IProfileRepository Profiles { get; }

    Task<int> SaveChangesAsync();
}
