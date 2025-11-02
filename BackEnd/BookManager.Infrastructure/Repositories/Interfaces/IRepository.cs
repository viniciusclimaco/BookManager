namespace BookManager.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Interface genérica para repositório base
/// </summary>
/// <typeparam name="TEntity">Tipo de entidade</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<int> CreateAsync(TEntity entity);
    Task<bool> UpdateAsync(TEntity entity);
    Task<bool> DeleteAsync(int id);
}
