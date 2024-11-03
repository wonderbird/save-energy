namespace SaveEnergy.Domain;

public interface IRepositoriesQuery
{
    Task<IEnumerable<Repository>> Execute();
}
