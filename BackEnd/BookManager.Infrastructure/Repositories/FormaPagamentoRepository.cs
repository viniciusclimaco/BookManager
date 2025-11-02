using Dapper;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.Data;
using BookManager.Infrastructure.Repositories.Interfaces;

namespace BookManager.Infrastructure.Repositories;

/// <summary>
/// Repositório de Forma de Pagamento
/// </summary>
public class FormaPagamentoRepository : RepositoryBase<FormaPagamento>, IFormaPagamentoRepository
{
    protected override string TableName => "FormaPagamento";

    public FormaPagamentoRepository(SqlConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<FormaPagamento?> GetByNomeAsync(string nome)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[FormaPagamento] WHERE [Nome] = @Nome";
        return await connection.QueryFirstOrDefaultAsync<FormaPagamento>(sql, new { Nome = nome });
    }

    public async Task<IEnumerable<FormaPagamento>> GetByAtivo(bool ativo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[FormaPagamento] WHERE [Ativo] = @Ativo ORDER BY [Nome]";
        return await connection.QueryAsync<FormaPagamento>(sql, new { Ativo = ativo });
    }

    protected override string BuildInsertQuery()
    {
        return @"
            INSERT INTO [dbo].[FormaPagamento] ([Nome], [Descricao], [Ativo])
            VALUES (@Nome, @Descricao, @Ativo);
            SELECT CAST(SCOPE_IDENTITY() as int);";
    }

    protected override string BuildUpdateQuery()
    {
        return @"
            UPDATE [dbo].[FormaPagamento]
            SET [Nome] = @Nome, [Descricao] = @Descricao, [Ativo] = @Ativo
            WHERE [IdFormaPagamento] = @IdFormaPagamento";
    }

    public override async Task<FormaPagamento?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[FormaPagamento] WHERE [IdFormaPagamento] = @Id";
        return await connection.QueryFirstOrDefaultAsync<FormaPagamento>(sql, new { Id = id });
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM [dbo].[FormaPagamento] WHERE [IdFormaPagamento] = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
