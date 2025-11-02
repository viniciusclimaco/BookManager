using BookManager.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using System.Xml.Linq;
using System.Text;

namespace BookManager.API.Controllers;

/// <summary>
/// Controller responsável pela geração de relatórios em diferentes formatos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioRepository _relatorioRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<RelatoriosController> _logger;

    public RelatoriosController(
        IRelatorioRepository relatorioRepository,
        IWebHostEnvironment environment,
        ILogger<RelatoriosController> logger)
    {
        _relatorioRepository = relatorioRepository;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Gera relatório de livros agrupados por assunto
    /// </summary>
    /// <param name="formato">Formato do arquivo: PDF, EXCEL ou WORD (padrão: PDF)</param>
    /// <param name="idAssunto">ID do assunto para filtrar (opcional)</param>
    /// <param name="anoInicio">Ano inicial de publicação (opcional)</param>
    /// <param name="anoFim">Ano final de publicação (opcional)</param>
    /// <param name="apenasAtivos">Retornar apenas livros ativos (padrão: true)</param>
    /// <returns>Arquivo do relatório no formato especificado</returns>
    [HttpGet("livros-por-assunto")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GerarRelatorioLivrosPorAssunto(
        [FromQuery] string formato = "PDF",
        [FromQuery] int? idAssunto = null,
        [FromQuery] int? anoInicio = null,
        [FromQuery] int? anoFim = null,
        [FromQuery] bool? apenasAtivos = true)
    {
        try
        {
            _logger.LogInformation("Gerando relatório de livros por assunto - Formato: {Formato}", formato);

            var dados = await _relatorioRepository.ObterLivrosPorAssuntoAsync(
                idAssunto, anoInicio, anoFim, apenasAtivos);

            var resultado = GerarRelatorio(
                "LivrosPorAssunto.rdlc",
                dados,
                "DsLivrosPorAssunto",
                formato);

            return File(resultado.Bytes, resultado.MimeType, resultado.NomeArquivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de livros por assunto");
            return StatusCode(500, new { message = $"Erro ao gerar relatório: {ex.Message}" });
        }
    }

    /// <summary>
    /// Gera relatório de autores com seus livros
    /// </summary>
    /// <param name="formato">Formato do arquivo: PDF, EXCEL ou WORD (padrão: PDF)</param>
    /// <param name="idAutor">ID do autor para filtrar (opcional)</param>
    /// <returns>Arquivo do relatório no formato especificado</returns>
    [HttpGet("autores-por-livro")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GerarRelatorioAutoresPorLivro(
        [FromQuery] string formato = "PDF",
        [FromQuery] int? idAutor = null)
    {
        try
        {
            _logger.LogInformation("Gerando relatório de autores por livro - Formato: {Formato}", formato);

            var dados = await _relatorioRepository.ObterAutoresPorLivroAsync(idAutor);

            var resultado = GerarRelatorio(
                "AutoresPorLivro.rdlc",
                dados,
                "DsAutoresPorLivro",
                formato);

            return File(resultado.Bytes, resultado.MimeType, resultado.NomeArquivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de autores por livro");
            return StatusCode(500, new { message = $"Erro ao gerar relatório: {ex.Message}" });
        }
    }

    /// <summary>
    /// Gera relatório de livros com preços por forma de pagamento
    /// </summary>
    /// <param name="formato">Formato do arquivo: PDF, EXCEL ou WORD (padrão: PDF)</param>
    /// <param name="valorMinimo">Valor mínimo para filtrar (opcional)</param>
    /// <param name="valorMaximo">Valor máximo para filtrar (opcional)</param>
    /// <param name="idFormaPagamento">ID da forma de pagamento para filtrar (opcional)</param>
    /// <param name="apenasAtivos">Retornar apenas livros ativos (padrão: true)</param>
    /// <returns>Arquivo do relatório no formato especificado</returns>
    [HttpGet("livros-com-preco")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GerarRelatorioLivrosComPreco(
        [FromQuery] string formato = "PDF",
        [FromQuery] decimal? valorMinimo = null,
        [FromQuery] decimal? valorMaximo = null,
        [FromQuery] int? idFormaPagamento = null,
        [FromQuery] bool? apenasAtivos = true)
    {
        try
        {
            _logger.LogInformation("Gerando relatório de livros com preço - Formato: {Formato}", formato);

            var dados = await _relatorioRepository.ObterLivrosComPrecoAsync(
                valorMinimo, valorMaximo, idFormaPagamento, apenasAtivos);

            var resultado = GerarRelatorio(
                "LivrosComPreco.rdlc",
                dados,
                "DsLivrosComPreco",
                formato);

            return File(resultado.Bytes, resultado.MimeType, resultado.NomeArquivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de livros com preço");
            return StatusCode(500, new { message = $"Erro ao gerar relatório: {ex.Message}" });
        }
    }

    /// <summary>
    /// Método privado para geração do relatório usando ReportViewer    
    /// </summary>
    private RelatorioResultado GerarRelatorio(
        string nomeArquivo,
        object dados,
        string dataSourceName,
        string formato)
    {
        var reportViewer = new LocalReport();
        var caminhoRelatorio = Path.Combine(_environment.ContentRootPath, "Reports", nomeArquivo);

        if (!System.IO.File.Exists(caminhoRelatorio))
        {
            throw new FileNotFoundException($"Arquivo de relatório não encontrado: {caminhoRelatorio}");
        }

        // Carrega o conteúdo RDLC e força orientação paisagem (landscape) no XML antes de carregar no LocalReport
        var xdoc = XDocument.Load(caminhoRelatorio);
        XNamespace ns = xdoc.Root?.Name.Namespace ?? "http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition";
        var page = xdoc.Descendants(ns + "Page").FirstOrDefault();
        if (page != null)
        {
            var pageWidth = page.Element(ns + "PageWidth");
            var pageHeight = page.Element(ns + "PageHeight");
            // Definir A4 em paisagem se valores existentes indicarem retrato
            if (pageWidth != null && pageHeight != null)
            {
                // Troca para29.7cm x21cm (A4 landscape)
                pageWidth.SetValue("29.7cm");
                pageHeight.SetValue("21cm");
            }
            else
            {
                page.Add(new XElement(ns + "PageWidth", "29.7cm"));
                page.Add(new XElement(ns + "PageHeight", "21cm"));
            }
        }

        // Carrega definição modificada no ReportViewer a partir de stream
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xdoc.Declaration + xdoc.ToString())))
        {
            reportViewer.LoadReportDefinition(ms);
        }

        reportViewer.DataSources.Add(new ReportDataSource(dataSourceName, dados));

        string reportType = formato.ToUpper();
        string mimeType;
        string encoding;
        string fileNameExtension;
        string[] streams;
        Warning[] warnings;

        // Forçar paisagem via deviceInfo também (fallback para renderers que respeitam)
        string deviceInfo =
            "<DeviceInfo>" +
            "  <OutputFormat>" + reportType + "</OutputFormat>" +
            "  <PageWidth>29.7cm</PageWidth>" +
            "  <PageHeight>21cm</PageHeight>" +
            "  <MarginTop>1cm</MarginTop>" +
            "  <MarginLeft>1cm</MarginLeft>" +
            "  <MarginRight>1cm</MarginRight>" +
            "  <MarginBottom>1cm</MarginBottom>" +
            "</DeviceInfo>";

        byte[] bytes = reportViewer.Render(
            reportType,
            deviceInfo,
            out mimeType,
            out encoding,
            out fileNameExtension,
            out streams,
            out warnings);

        return new RelatorioResultado
        {
            Bytes = bytes,
            MimeType = mimeType,
            NomeArquivo = $"Relatorio_{DateTime.Now:yyyyMMddHHmmss}.{fileNameExtension}"
        };
    }
}

/// <summary>
/// Classe auxiliar para resultado da geração de relatório
/// </summary>
public class RelatorioResultado
{
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
    public string MimeType { get; set; } = string.Empty;
    public string NomeArquivo { get; set; } = string.Empty;
}