using BookManager.Infrastructure.Data;
using BookManager.Infrastructure.Repositories.Interfaces;
using Dapper;

namespace BookManager.Infrastructure.Repositories;

/// <summary>
/// Repositório base genérico usando Dapper
/// </summary>
/// <typeparam name="TEntity">Tipo de entidade</typeparam>
public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly SqlConnectionFactory _connectionFactory;
    protected abstract string TableName { get; }

    protected RepositoryBase(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"SELECT * FROM [dbo].[{TableName}] WHERE Id{TableName} = @Id";
        return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"SELECT * FROM [dbo].[{TableName}]";
        return await connection.QueryAsync<TEntity>(sql);
    }

    public virtual async Task<int> CreateAsync(TEntity entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        var insertQuery = BuildInsertQuery();
        var identity = await connection.ExecuteScalarAsync<int>(insertQuery, entity);
        return identity;
    }

    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        var updateQuery = BuildUpdateQuery();
        var result = await connection.ExecuteAsync(updateQuery, entity);
        return result > 0;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"DELETE FROM [dbo].[{TableName}] WHERE Id{TableName} = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    protected virtual string BuildInsertQuery()
    {
        throw new NotImplementedException("BuildInsertQuery deve ser implementado na classe derivada");
    }

    protected virtual string BuildUpdateQuery()
    {
        throw new NotImplementedException("BuildUpdateQuery deve ser implementado na classe derivada");
    }
}
