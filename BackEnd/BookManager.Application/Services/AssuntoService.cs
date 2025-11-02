using FluentValidation;
using BookManager.Domain.Entities;
using BookManager.Domain.Exceptions;
using BookManager.Infrastructure.Repositories.Interfaces;
using BookManager.Infrastructure.DTOs;
using BookManager.Infrastructure.Extensions;
using BookManager.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookManager.Application.Services;

/// <summary>
/// Serviço de aplicação para Assunto
/// </summary>
public class AssuntoService : IAssuntoService
{
    private readonly IAssuntoRepository _assuntoRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly IValidator<CreateAssuntoDto> _createValidator;
    private readonly IValidator<UpdateAssuntoDto> _updateValidator;
    private readonly ILogger<AssuntoService> _logger;

    public AssuntoService(
        IAssuntoRepository assuntoRepository,
        ILivroRepository livroRepository,
        IValidator<CreateAssuntoDto> createValidator,
        IValidator<UpdateAssuntoDto> updateValidator,
        ILogger<AssuntoService> logger)
    {
        _assuntoRepository = assuntoRepository ?? throw new ArgumentNullException(nameof(assuntoRepository));
        _livroRepository = livroRepository ?? throw new ArgumentNullException(nameof(livroRepository));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AssuntoDto?> GetByIdAsync(int id)
    {
        var assunto = await _assuntoRepository.GetByIdAsync(id);
        return assunto == null ? null : MapToDto(assunto);
    }

    public async Task<IEnumerable<AssuntoDto>> GetAllAsync()
    {
        var assuntos = await _assuntoRepository.GetAllAsync();
        return assuntos.Select(MapToDto);
    }

    public async Task<IEnumerable<AssuntoDto>> GetAtivoAsync()
    {
        var assuntos = await _assuntoRepository.GetByAtivo(true);
        return assuntos.Select(MapToDto);
    }

    public async Task<int> CreateAsync(CreateAssuntoDto dto)
    {
        // Validação de schema
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
                
        var existente = await _assuntoRepository.GetByDescricaoAsync(dto.Descricao);
        if (existente != null)
        {
            _logger.LogWarning(
                "Tentativa de criar assunto duplicado. Descrição: {Descricao}",
                dto.Descricao);
            throw new DuplicateResourceException("Assunto", "Descrição", dto.Descricao);
        }

        var assunto = new Assunto
        {
            Descricao = dto.Descricao,
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };

        try
        {
            return await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                async () => await _assuntoRepository.CreateAsync(assunto),
                $"CreateAssunto - Descrição: {dto.Descricao}"
            );
        }
        catch (UniqueKeyViolationException)
        {
            throw new DuplicateResourceException("Assunto", "Descrição", dto.Descricao);
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateAssuntoDto dto)
    {
        // Validação de schema
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var assunto = await _assuntoRepository.GetByIdAsync(id);
        if (assunto == null)
        {
            _logger.LogWarning("Tentativa de atualizar assunto inexistente. ID: {AssuntoId}", id);
            throw new KeyNotFoundException($"Assunto com ID {id} não encontrado.");
        }
                
        if (assunto.Descricao != dto.Descricao)
        {
            var existente = await _assuntoRepository.GetByDescricaoAsync(dto.Descricao);
            if (existente != null && existente.IdAssunto != id)
            {
                _logger.LogWarning(
                    "Tentativa de atualizar assunto para descrição duplicada. ID: {AssuntoId}, Descrição: {Descricao}",
                    id, dto.Descricao);
                throw new DuplicateResourceException("Assunto", "Descrição", dto.Descricao);
            }
        }

        assunto.Descricao = dto.Descricao;
        assunto.Ativo = dto.Ativo;

        try
        {
            return await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                async () => await _assuntoRepository.UpdateAsync(assunto),
                $"UpdateAssunto - ID: {id}"
            );
        }
        catch (UniqueKeyViolationException)
        {
            throw new DuplicateResourceException("Assunto", "Descrição", dto.Descricao);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var assunto = await _assuntoRepository.GetByIdAsync(id);
        if (assunto == null)
        {
            _logger.LogWarning("Tentativa de excluir assunto inexistente. ID: {AssuntoId}", id);
            throw new KeyNotFoundException($"Assunto com ID {id} não encontrado.");
        }
                
        var livrosAssociados = await _livroRepository.GetByAssuntoAsync(id);
        var livrosList = livrosAssociados.ToList();

        if (livrosList.Any())
        {
            _logger.LogWarning(
                "Tentativa de excluir assunto com livros associados. AssuntoId: {AssuntoId}, Descrição: {Descricao}, LivrosCount: {Count}",
                id, assunto.Descricao, livrosList.Count);

            throw new ResourceInUseException(
                resourceType: "Assunto",
                resourceId: id,
                resourceName: assunto.Descricao,
                dependentEntityType: "Livro",
                dependentCount: livrosList.Count
            );
        }

        try
        {
            return await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                async () => await _assuntoRepository.DeleteAsync(id),
                $"DeleteAssunto - ID: {id}, Descrição: {assunto.Descricao}"
            );
        }
        catch (ForeignKeyViolationException)
        {
            // Race condition: busca novamente para ter a contagem atualizada
            var livrosAtualizados = await _livroRepository.GetByAssuntoAsync(id);
            var count = livrosAtualizados.Count();

            throw new ResourceInUseException(
                resourceType: "Assunto",
                resourceId: id,
                resourceName: assunto.Descricao,
                dependentEntityType: "Livro",
                dependentCount: count
            );
        }
    }

    private static AssuntoDto MapToDto(Assunto assunto) => new()
    {
        IdAssunto = assunto.IdAssunto,
        Descricao = assunto.Descricao,
        Ativo = assunto.Ativo
    };
}