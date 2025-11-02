using Dapper;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.Data;
using BookManager.Infrastructure.Repositories.Interfaces;

namespace BookManager.Infrastructure.Repositories;

/// <summary>
/// Repositório de Assunto
/// </summary>
public class AssuntoRepository : RepositoryBase<Assunto>, IAssuntoRepository
{
    protected override string TableName => "Assunto";

    public AssuntoRepository(SqlConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<Assunto?> GetByDescricaoAsync(string descricao)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Assunto] WHERE [Descricao] = @Descricao";
        return await connection.QueryFirstOrDefaultAsync<Assunto>(sql, new { Descricao = descricao });
    }

    public async Task<IEnumerable<Assunto>> GetByAtivo(bool ativo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Assunto] WHERE [Ativo] = @Ativo ORDER BY [Descricao]";
        return await connection.QueryAsync<Assunto>(sql, new { Ativo = ativo });
    }

    protected override string BuildInsertQuery()
    {
        return @"
            INSERT INTO [dbo].[Assunto] ([Descricao], [DataCadastro], [Ativo])
            VALUES (@Descricao, @DataCadastro, @Ativo);
            SELECT CAST(SCOPE_IDENTITY() as int);";
    }

    protected override string BuildUpdateQuery()
    {
        return @"
            UPDATE [dbo].[Assunto]
            SET [Descricao] = @Descricao, [Ativo] = @Ativo
            WHERE [IdAssunto] = @IdAssunto";
    }

    public override async Task<Assunto?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Assunto] WHERE [IdAssunto] = @Id";
        return await connection.QueryFirstOrDefaultAsync<Assunto>(sql, new { Id = id });
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM [dbo].[Assunto] WHERE [IdAssunto] = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
