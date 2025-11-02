using BookManager.Application.Services.Interfaces;
using BookManager.Infrastructure.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BookManager.API.Controllers;

/// <summary>
/// Controller para gerenciamento de Formas de Pagamento
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FormasPagamentoController : ControllerBase
{
    private readonly IFormaPagamentoService _formaPagamentoService;
    private readonly ILogger<FormasPagamentoController> _logger;

    public FormasPagamentoController(IFormaPagamentoService formaPagamentoService, ILogger<FormasPagamentoController> logger)
    {
        _formaPagamentoService = formaPagamentoService ?? throw new ArgumentNullException(nameof(formaPagamentoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FormaPagamentoDto>>> GetTodos()
    {
        try
        {
            var formasPagamento = await _formaPagamentoService.GetAllAsync();
            return Ok(formasPagamento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter formas de pagamento");
            return StatusCode(500, new { mensagem = "Erro ao obter formas de pagamento", erro = ex.Message });
        }
    }

    [HttpGet("ativos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FormaPagamentoDto>>> GetAtivos()
    {
        try
        {
            var formasPagamento = await _formaPagamentoService.GetAtivoAsync();
            return Ok(formasPagamento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter formas de pagamento ativas");
            return StatusCode(500, new { mensagem = "Erro ao obter formas de pagamento ativas", erro = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FormaPagamentoDto>> GetPorId(int id)
    {
        try
        {
            var formaPagamento = await _formaPagamentoService.GetByIdAsync(id);
            if (formaPagamento == null)
                return NotFound(new { mensagem = $"Forma de Pagamento com ID {id} não encontrada" });

            return Ok(formaPagamento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao obter forma de pagamento com ID {id}");
            return StatusCode(500, new { mensagem = "Erro ao obter forma de pagamento", erro = ex.Message });
        }
    }
}
