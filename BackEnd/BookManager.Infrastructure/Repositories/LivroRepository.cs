using Dapper;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.Data;
using BookManager.Infrastructure.Repositories.Interfaces;

namespace BookManager.Infrastructure.Repositories;

/// <summary>
/// Repositório de Livro
/// </summary>
public class LivroRepository : RepositoryBase<Livro>, ILivroRepository
{
    protected override string TableName => "Livro";

    public LivroRepository(SqlConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<Livro?> GetByTituloAsync(string titulo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Livro] WHERE [Titulo] = @Titulo";
        return await connection.QueryFirstOrDefaultAsync<Livro>(sql, new { Titulo = titulo });
    }

    public async Task<Livro?> GetByISBNAsync(string isbn)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Livro] WHERE [ISBN] = @ISBN";
        return await connection.QueryFirstOrDefaultAsync<Livro>(sql, new { ISBN = isbn });
    }

    public async Task<Livro?> GetWithDetailsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT * FROM [dbo].[Livro] WHERE [IdLivro] = @Id";

        return await connection.QueryFirstOrDefaultAsync<Livro>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Livro>> GetByAssuntoAsync(int idAssunto)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Livro] WHERE [IdAssunto] = @IdAssunto ORDER BY [Titulo]";
        return await connection.QueryAsync<Livro>(sql, new { IdAssunto = idAssunto });
    }

    public async Task<IEnumerable<Livro>> GetByAutorAsync(int idAutor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT DISTINCT L.* FROM [dbo].[Livro] L
            INNER JOIN [dbo].[LivroAutor] LA ON L.[IdLivro] = LA.[IdLivro]
            WHERE LA.[IdAutor] = @IdAutor
            ORDER BY L.[Titulo]";
        return await connection.QueryAsync<Livro>(sql, new { IdAutor = idAutor });
    }

    public async Task<IEnumerable<Livro>> GetByAtivo(bool ativo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Livro] WHERE [Ativo] = @Ativo ORDER BY [Titulo]";
        return await connection.QueryAsync<Livro>(sql, new { Ativo = ativo });
    }

    /// <summary>
    /// Retorna todos os livros com relacionamentos
    /// Application faz a transformação em DTO (RelatorioAutorAgrupado)
    /// </summary>
    public async Task<IEnumerable<Livro>> GetLivrosComTodosOsRelacionamentosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Retorna apenas Livros com Assunto
        const string sql = @"
            SELECT L.*, A.[IdAssunto], A.[Descricao] AS [Descricao]
            FROM [dbo].[Livro] L
            INNER JOIN [dbo].[Assunto] A ON L.[IdAssunto] = A.[IdAssunto]
            WHERE L.[Ativo] = 1
            ORDER BY L.[Titulo]";

        // Retorna Entity, não DTO
        return await connection.QueryAsync<Livro>(sql);
    }

    protected override string BuildInsertQuery()
    {
        return @"
            INSERT INTO [dbo].[Livro] ([Titulo], [Editora], [AnoPublicacao], [ISBN], [IdAssunto], [DataCadastro], [Ativo])
            VALUES (@Titulo, @Editora, @AnoPublicacao, @ISBN, @IdAssunto, @DataCadastro, @Ativo);
            SELECT CAST(SCOPE_IDENTITY() as int);";
    }

    protected override string BuildUpdateQuery()
    {
        return @"
            UPDATE [dbo].[Livro]
            SET [Titulo] = @Titulo, [Editora] = @Editora, [AnoPublicacao] = @AnoPublicacao, 
                [ISBN] = @ISBN, [IdAssunto] = @IdAssunto, [Ativo] = @Ativo
            WHERE [IdLivro] = @IdLivro";
    }

    public override async Task<Livro?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM [dbo].[Livro] WHERE [IdLivro] = @Id";
        return await connection.QueryFirstOrDefaultAsync<Livro>(sql, new { Id = id });
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM [dbo].[Livro] WHERE [IdLivro] = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
