using BookManager.Infrastructure.DTOs.Report;

namespace BookManager.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Interface para operações de geração de relatórios
/// Utiliza views otimizadas do SQL Server
/// </summary>
public interface IRelatorioRepository
{    
    Task<IEnumerable<LivroPorAssuntoDto>> ObterLivrosPorAssuntoAsync(
        int? idAssunto = null, 
        int? anoInicio = null, 
        int? anoFim = null, 
        bool? apenasAtivos = true);

    Task<IEnumerable<AutorPorLivroDto>> ObterAutoresPorLivroAsync(int? idAutor = null);
        
    Task<IEnumerable<LivroComPrecoDto>> ObterLivrosComPrecoAsync(
        decimal? valorMinimo = null,
        decimal? valorMaximo = null,
        int? idFormaPagamento = null,
        bool? apenasAtivos = true);
}
