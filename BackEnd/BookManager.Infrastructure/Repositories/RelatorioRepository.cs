using BookManager.Infrastructure.Data;
using BookManager.Infrastructure.DTOs.Report;
using BookManager.Infrastructure.Repositories.Interfaces;
using Dapper;

namespace BookManager.Infrastructure.Repositories;

/// <summary>
/// Implementação do repository de relatórios
/// Utiliza Dapper para consultas otimizadas em views do SQL Server
/// </summary>
public class RelatorioRepository : IRelatorioRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public RelatorioRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<IEnumerable<LivroPorAssuntoDto>> ObterLivrosPorAssuntoAsync(int? idAssunto, int? anoInicio, int? anoFim, bool? apenasAtivos)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                IdAssunto,
                Assunto,
                IdLivro,
                Titulo,
                Editora,
                ISBN,
                AnoPublicacao,
                Autores,
                Ativo
            FROM vw_RelatorioLivrosPorAssunto
            WHERE (@IdAssunto IS NULL OR IdAssunto = @IdAssunto)
              AND (@AnoInicio IS NULL OR AnoPublicacao >= @AnoInicio)
              AND (@AnoFim IS NULL OR AnoPublicacao <= @AnoFim)
              AND (@ApenasAtivos IS NULL OR Ativo = @ApenasAtivos)
            ORDER BY Assunto, Titulo";

        return await connection.QueryAsync<LivroPorAssuntoDto>(sql, new
        {
            IdAssunto = idAssunto,
            AnoInicio = anoInicio,
            AnoFim = anoFim,
            ApenasAtivos = apenasAtivos
        });
    }

    public async Task<IEnumerable<AutorPorLivroDto>> ObterAutoresPorLivroAsync(int? idAutor)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                IdAutor,
                Autor,
                IdLivro,
                Titulo,
                Editora,
                Assunto,
                AnoPublicacao,
                ISBN,
                OrdemAutor
            FROM vw_RelatorioAutoresPorLivro
            WHERE (@IdAutor IS NULL OR IdAutor = @IdAutor)
            ORDER BY Autor, OrdemAutor";

        return await connection.QueryAsync<AutorPorLivroDto>(sql, new
        {
            IdAutor = idAutor
        });
    }

    public async Task<IEnumerable<LivroComPrecoDto>> ObterLivrosComPrecoAsync(decimal? valorMinimo, decimal? valorMaximo, int? idFormaPagamento, bool? apenasAtivos)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                IdLivro,
                Titulo,
                Editora,
                ISBN,
                AnoPublicacao,
                Assunto,
                Autores,
                IdFormaPagamento,
                FormaPagamento,
                Preco
            FROM vw_RelatorioLivrosComPreco
            WHERE (@ValorMinimo IS NULL OR Preco >= @ValorMinimo)
              AND (@ValorMaximo IS NULL OR Preco <= @ValorMaximo)
              AND (@IdFormaPagamento IS NULL OR IdFormaPagamento = @IdFormaPagamento)
            ORDER BY Titulo, FormaPagamento";

        return await connection.QueryAsync<LivroComPrecoDto>(sql, new
        {
            ValorMinimo = valorMinimo,
            ValorMaximo = valorMaximo,
            IdFormaPagamento = idFormaPagamento
        });
    }
}
