using BookManager.Infrastructure.DTOs.Report;

namespace BookManager.Application.Services.Interfaces
{
    public interface IRelatorioService
    {        
        Task<IEnumerable<LivroPorAssuntoDto>> ObterLivrosPorAssuntoAsync(
        int? idAssunto = null,
        int? anoInicio = null,
        int? anoFim = null,
        bool? apenasAtivos = true);
             
        Task<IEnumerable<AutorPorLivroDto>> ObterAutoresPorLivroAsync(
            int? idAutor = null);
                
        Task<IEnumerable<LivroComPrecoDto>> ObterLivrosComPrecoAsync(
            decimal? valorMinimo = null,
            decimal? valorMaximo = null,
            int? idFormaPagamento = null,
            bool? apenasAtivos = true);
    }
}
