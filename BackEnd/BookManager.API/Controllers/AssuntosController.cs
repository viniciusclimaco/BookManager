using BookManager.Application.Services.Interfaces;
using BookManager.Infrastructure.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BookManager.API.Controllers;

/// <summary>
/// Controller para gerenciamento de Assuntos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AssuntosController : ControllerBase
{
    private readonly IAssuntoService _assuntoService;
    private readonly ILogger<AssuntosController> _logger;

    public AssuntosController(IAssuntoService assuntoService, ILogger<AssuntosController> logger)
    {
        _assuntoService = assuntoService ?? throw new ArgumentNullException(nameof(assuntoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AssuntoDto>>> GetTodos()
    {
        try
        {
            var assuntos = await _assuntoService.GetAllAsync();
            return Ok(assuntos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter assuntos");
            return StatusCode(500, new { mensagem = "Erro ao obter assuntos", erro = ex.Message });
        }
    }

    [HttpGet("ativos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AssuntoDto>>> GetAtivos()
    {
        try
        {
            var assuntos = await _assuntoService.GetAtivoAsync();
            return Ok(assuntos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter assuntos ativos");
            return StatusCode(500, new { mensagem = "Erro ao obter assuntos ativos", erro = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AssuntoDto>> GetPorId(int id)
    {
        try
        {
            var assunto = await _assuntoService.GetByIdAsync(id);
            if (assunto == null)
                return NotFound(new { mensagem = $"Assunto com ID {id} não encontrado" });

            return Ok(assunto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao obter assunto com ID {id}");
            return StatusCode(500, new { mensagem = "Erro ao obter assunto", erro = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> Criar([FromBody] CreateAssuntoDto dto)
    {
        try
        {
            var idAssunto = await _assuntoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetPorId), new { id = idAssunto }, new { id = idAssunto });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { mensagem = "Dados inválidos", erros = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar assunto");
            return StatusCode(500, new { mensagem = "Erro ao criar assunto", erro = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> Atualizar(int id, [FromBody] UpdateAssuntoDto dto)
    {
        try
        {
            var result = await _assuntoService.UpdateAsync(id, dto);
            return Ok(new { sucesso = result, mensagem = result ? "Assunto atualizado com sucesso" : "Erro ao atualizar assunto" });
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { mensagem = "Dados inválidos", erros = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao atualizar assunto com ID {id}");
            return StatusCode(500, new { mensagem = "Erro ao atualizar assunto", erro = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> Deletar(int id)
    {
        try
        {
            var result = await _assuntoService.DeleteAsync(id);
            return Ok(new { sucesso = result, mensagem = result ? "Assunto deletado com sucesso" : "Erro ao deletar assunto" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao deletar assunto com ID {id}");
            return StatusCode(500, new { mensagem = "Erro ao deletar assunto", erro = ex.Message });
        }
    }
}
