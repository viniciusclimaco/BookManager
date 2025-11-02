using Dapper;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.Data;
using BookManager.Infrastructure.Repositories.Interfaces;

namespace BookManager.Infrastructure.Repositories;

/// <summary>
/// Repositório de Preço do Livro
/// </summary>
public class LivroPrecoRepository : RepositoryBase<LivroPreco>, ILivroPrecoRepository
{
    protected override string TableName => "LivroPreco";

    public LivroPrecoRepository(SqlConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<LivroPreco?> GetByLivroAndFormaPagamentoAsync(int idLivro, int idFormaPagamento)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM [dbo].[LivroPreco] 
            WHERE [IdLivro] = @IdLivro AND [IdFormaPagamento] = @IdFormaPagamento";
        return await connection.QueryFirstOrDefaultAsync<LivroPreco>(sql, 
            new { IdLivro = idLivro, IdFormaPagamento = idFormaPagamento });
    }

    public async Task<IEnumerable<LivroPreco>> GetByLivroAsync(int idLivro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[LivroPreco] WHERE [IdLivro] = @IdLivro ORDER BY [IdFormaPagamento]";
        return await connection.QueryAsync<LivroPreco>(sql, new { IdLivro = idLivro });
    }

    public async Task<IEnumerable<LivroPreco>> GetByFormaPagamentoAsync(int idFormaPagamento)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[LivroPreco] WHERE [IdFormaPagamento] = @IdFormaPagamento";
        return await connection.QueryAsync<LivroPreco>(sql, new { IdFormaPagamento = idFormaPagamento });
    }

    public async Task<bool> DeleteByLivroAsync(int idLivro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM [dbo].[LivroPreco] WHERE [IdLivro] = @IdLivro";
        var result = await connection.ExecuteAsync(sql, new { IdLivro = idLivro });
        return result > 0;
    }

    protected override string BuildInsertQuery()
    {
        return @"
            INSERT INTO [dbo].[LivroPreco] ([IdLivro], [IdFormaPagamento], [Valor], [DataCadastro], [DataAtualizacao])
            VALUES (@IdLivro, @IdFormaPagamento, @Valor, @DataCadastro, @DataAtualizacao);
            SELECT CAST(SCOPE_IDENTITY() as int);";
    }

    protected override string BuildUpdateQuery()
    {
        return @"
            UPDATE [dbo].[LivroPreco]
            SET [Valor] = @Valor, [DataAtualizacao] = @DataAtualizacao
            WHERE [IdLivroPreco] = @IdLivroPreco";
    }

    public override async Task<LivroPreco?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[LivroPreco] WHERE [IdLivroPreco] = @Id";
        return await connection.QueryFirstOrDefaultAsync<LivroPreco>(sql, new { Id = id });
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM [dbo].[LivroPreco] WHERE [IdLivroPreco] = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
