using Dapper;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.Data;
using BookManager.Infrastructure.Repositories.Interfaces;

namespace BookManager.Infrastructure.Repositories;

/// <summary>
/// Repositório de Autor
/// </summary>
public class AutorRepository : RepositoryBase<Autor>, IAutorRepository
{
    protected override string TableName => "Autor";

    public AutorRepository(SqlConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<Autor?> GetByNomeAsync(string nome)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Autor] WHERE [Nome] = @Nome";
        return await connection.QueryFirstOrDefaultAsync<Autor>(sql, new { Nome = nome });
    }

    public async Task<IEnumerable<Autor>> GetByAtivo(bool ativo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Autor] WHERE [Ativo] = @Ativo ORDER BY [Nome]";
        return await connection.QueryAsync<Autor>(sql, new { Ativo = ativo });
    }

    protected override string BuildInsertQuery()
    {
        return @"
            INSERT INTO [dbo].[Autor] ([Nome], [DataCadastro], [Ativo])
            VALUES (@Nome, @DataCadastro, @Ativo);
            SELECT CAST(SCOPE_IDENTITY() as int);";
    }

    protected override string BuildUpdateQuery()
    {
        return @"
            UPDATE [dbo].[Autor]
            SET [Nome] = @Nome, [Ativo] = @Ativo
            WHERE [IdAutor] = @IdAutor";
    }

    public override async Task<Autor?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Autor] WHERE [IdAutor] = @Id";
        return await connection.QueryFirstOrDefaultAsync<Autor>(sql, new { Id = id });
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM [dbo].[Autor] WHERE [IdAutor] = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
