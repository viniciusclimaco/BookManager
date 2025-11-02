using BookManager.Domain.Entities;
using BookManager.Infrastructure.Data;
using BookManager.Infrastructure.Repositories.Interfaces;
using Dapper;

namespace BookManager.Infrastructure.Repositories;

/// <summary>
/// Repositório de Livro-Autor
/// </summary>
public class LivroAutorRepository : RepositoryBase<LivroAutor>, ILivroAutorRepository
{
    protected override string TableName => "LivroAutor";

    public LivroAutorRepository(SqlConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<LivroAutor>> GetByLivroAsync(int idLivro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM [dbo].[LivroAutor] 
            WHERE [IdLivro] = @IdLivro 
            ORDER BY [Ordem]";
        return await connection.QueryAsync<LivroAutor>(sql, new { IdLivro = idLivro });
    }

    public async Task<IEnumerable<LivroAutor>> GetByAutorAsync(int idAutor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM [dbo].[LivroAutor] 
            WHERE [IdAutor] = @IdAutor 
            ORDER BY [IdLivro]";
        return await connection.QueryAsync<LivroAutor>(sql, new { IdAutor = idAutor });
    }

    public async Task<bool> DeleteByLivroAsync(int idLivro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM [dbo].[LivroAutor] WHERE [IdLivro] = @IdLivro";
        var result = await connection.ExecuteAsync(sql, new { IdLivro = idLivro });
        return result > 0;
    }

    protected override string BuildInsertQuery()
    {
        return @"
            INSERT INTO [dbo].[LivroAutor] ([IdLivro], [IdAutor], [Ordem], [DataCadastro])
            VALUES (@IdLivro, @IdAutor, @Ordem, @DataCadastro);
            SELECT CAST(SCOPE_IDENTITY() as int);";
    }

    protected override string BuildUpdateQuery()
    {
        return @"
            UPDATE [dbo].[LivroAutor]
            SET [Ordem] = @Ordem
            WHERE [IdLivroAutor] = @IdLivroAutor";
    }

    public override async Task<LivroAutor?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[LivroAutor] WHERE [IdLivroAutor] = @Id";
        return await connection.QueryFirstOrDefaultAsync<LivroAutor>(sql, new { Id = id });
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM [dbo].[LivroAutor] WHERE [IdLivroAutor] = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
