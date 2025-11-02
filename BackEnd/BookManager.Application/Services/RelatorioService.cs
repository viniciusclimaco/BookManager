using BookManager.Application.Services.Interfaces;
using BookManager.Infrastructure.DTOs.Report;
using BookManager.Infrastructure.Repositories.Interfaces;

namespace BookManager.Application.Services;

/// <summary>
/// Serviço de aplicação para geração de relatórios
/// </summary>
public class RelatorioService : IRelatorioService
{
    private readonly IRelatorioRepository _relatorioRepository;

    public RelatorioService(IRelatorioRepository relatorioRepository)
    {
        _relatorioRepository = relatorioRepository ?? throw new ArgumentNullException(nameof(relatorioRepository)); 
    }

    public async Task<IEnumerable<AutorPorLivroDto>> ObterAutoresPorLivroAsync(int? idAutor = null)
    {
        return await _relatorioRepository.ObterAutoresPorLivroAsync(idAutor);
    }

    public async Task<IEnumerable<LivroComPrecoDto>> ObterLivrosComPrecoAsync(decimal? valorMinimo = null, decimal? valorMaximo = null, int? idFormaPagamento = null, bool? apenasAtivos = true)
    {
        return await _relatorioRepository.ObterLivrosComPrecoAsync(valorMinimo, valorMaximo, idFormaPagamento, apenasAtivos);
    }

    public async Task<IEnumerable<LivroPorAssuntoDto>> ObterLivrosPorAssuntoAsync(int? idAssunto = null, int? anoInicio = null, int? anoFim = null, bool? apenasAtivos = true)
    {
        return await _relatorioRepository.ObterLivrosPorAssuntoAsync(idAssunto, anoInicio, anoFim, apenasAtivos);
    }
}
