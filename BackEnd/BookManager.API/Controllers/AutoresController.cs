using BookManager.Application.Services.Interfaces;
using BookManager.Infrastructure.DTOs;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using System.Linq;

namespace LibraryManager.API.Controllers;

/// <summary>
/// Controller para gerenciamento de Autores
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AutoresController : ControllerBase
{
    private readonly IAutorService _autorService;
    private readonly ILogger<AutoresController> _logger;

    public AutoresController(IAutorService autorService, ILogger<AutoresController> logger)
    {
        _autorService = autorService ?? throw new ArgumentNullException(nameof(autorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obter todos os autores
    /// </summary>
    /// <response code="200">Lista de autores retornada com sucesso</response>
    /// <response code="500">Erro interno no servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AutorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AutorDto>>> GetTodos()
    {
        try
        {
            var autores = await _autorService.GetAllAsync();
            return Ok(autores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter autores");
            return StatusCode(500, new { mensagem = "Erro ao obter autores", erro = ex.Message });
        }
    }

    /// <summary>
    /// Obter autores ativos
    /// </summary>
    /// <response code="200">Lista de autores ativos retornada com sucesso</response>
    /// <response code="500">Erro interno no servidor</response>
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(IEnumerable<AutorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AutorDto>>> GetAtivos()
    {
        try
        {
            var autores = await _autorService.GetAtivoAsync();
            return Ok(autores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter autores ativos");
            return StatusCode(500, new { mensagem = "Erro ao obter autores ativos", erro = ex.Message });
        }
    }

    /// <summary>
    /// Obter autor por ID
    /// </summary>
    /// <param name="id">ID do autor</param>
    /// <response code="200">Autor encontrado</response>
    /// <response code="404">Autor não encontrado</response>
    /// <response code="500">Erro interno no servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AutorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AutorDto>> GetPorId(int id)
    {
        try
        {
            var autor = await _autorService.GetByIdAsync(id);
            if (autor == null)
                return NotFound(new { mensagem = $"Autor com ID {id} não encontrado" });

            return Ok(autor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao obter autor com ID {id}");
            return StatusCode(500, new { mensagem = "Erro ao obter autor", erro = ex.Message });
        }
    }

    /// <summary>
    /// Criar novo autor
    /// </summary>
    /// <param name="dto">Dados do autor a ser criado</param>
    /// <response code="201">Autor criado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="409">Autor com mesmo nome já existe</response>
    /// <response code="500">Erro interno no servidor</response>    
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> Criar([FromBody] CreateAutorDto dto)
    {
        try
        {
            var idAutor = await _autorService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetPorId), new { id = idAutor }, new { id = idAutor });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { mensagem = "Dados inválidos", erros = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar autor");
            return StatusCode(500, new { mensagem = "Erro ao criar autor", erro = ex.Message });
        }
    }

    /// <summary>
    /// Atualizar autor
    /// </summary>
    /// <param name="id">ID do autor a ser atualizado</param>
    /// <param name="dto">Novos dados do autor</param>
    /// <response code="200">Autor atualizado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Autor não encontrado</response>
    /// <response code="409">Autor com mesmo nome já existe</response>
    /// <response code="500">Erro interno no servidor</response>    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> Atualizar(int id, [FromBody] UpdateAutorDto dto)
    {
        try
        {
            var result = await _autorService.UpdateAsync(id, dto);
            return Ok(new
            {
                sucesso = result,
                mensagem = result ? "Autor atualizado com sucesso" : "Erro ao atualizar autor"
            });
        }
        catch (ValidationException ex)
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
            _logger.LogError(ex, $"Erro ao atualizar autor com ID {id}");
            return StatusCode(500, new { mensagem = "Erro ao atualizar autor", erro = ex.Message });
        }
    }

    /// <summary>
    /// Deletar autor
    /// </summary>
    /// <param name="id">ID do autor a ser deletado</param>
    /// <response code="200">Autor deletado com sucesso</response>
    /// <response code="404">Autor não encontrado</response>
    /// <response code="409">Autor possui livros associados e não pode ser excluído</response>
    /// <response code="500">Erro interno no servidor</response>    
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> Deletar(int id)
    {
        try
        {
            // O Middleware converte em 409 Conflict com mensagem amigável
            var result = await _autorService.DeleteAsync(id);
            return Ok(new
            {
                sucesso = result,
                mensagem = result ? "Autor deletado com sucesso" : "Erro ao deletar autor"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao deletar autor com ID {id}");
            return StatusCode(500, new { mensagem = "Erro ao deletar autor", erro = ex.Message });
        }
    }
}