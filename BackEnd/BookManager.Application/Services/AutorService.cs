using BookManager.Application.Services.Interfaces;
using BookManager.Domain.Entities;
using BookManager.Domain.Exceptions;
using BookManager.Infrastructure.DTOs;
using BookManager.Infrastructure.Extensions;
using BookManager.Infrastructure.Repositories.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BookManager.Application.Services;

/// <summary>
/// Serviço de aplicação para Autor
/// </summary>
public class AutorService : IAutorService
{
    private readonly IAutorRepository _autorRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly IValidator<CreateAutorDto> _createValidator;
    private readonly IValidator<UpdateAutorDto> _updateValidator;
    private readonly ILogger<AutorService> _logger;

    public AutorService(
        IAutorRepository autorRepository,
        ILivroRepository livroRepository,
        IValidator<CreateAutorDto> createValidator,
        IValidator<UpdateAutorDto> updateValidator,
        ILogger<AutorService> logger)
    {
        _autorRepository = autorRepository ?? throw new ArgumentNullException(nameof(autorRepository));
        _livroRepository = livroRepository ?? throw new ArgumentNullException(nameof(livroRepository));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AutorDto?> GetByIdAsync(int id)
    {
        var autor = await _autorRepository.GetByIdAsync(id);
        return autor == null ? null : MapToDto(autor);
    }

    public async Task<IEnumerable<AutorDto>> GetAllAsync()
    {
        var autores = await _autorRepository.GetAllAsync();
        return autores.Select(MapToDto);
    }

    public async Task<IEnumerable<AutorDto>> GetAtivoAsync()
    {
        var autores = await _autorRepository.GetByAtivo(true);
        return autores.Select(MapToDto);
    }

    public async Task<int> CreateAsync(CreateAutorDto dto)
    {
        // Validação de schema
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
                
        var existente = await _autorRepository.GetByNomeAsync(dto.Nome);
        if (existente != null)
        {
            _logger.LogWarning(
                "Tentativa de criar autor duplicado. Nome: {Nome}",
                dto.Nome);
            throw new DuplicateResourceException("Autor", "Nome", dto.Nome);
        }

        var autor = new Autor
        {
            Nome = dto.Nome,
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };

        try
        {            
            return await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                async () => await _autorRepository.CreateAsync(autor),
                $"CreateAutor - Nome: {dto.Nome}"
            );
        }
        catch (UniqueKeyViolationException)
        {
            // Se mesmo com a validação prévia houver violação (race condition),
            // lançar exceção mais amigável
            throw new DuplicateResourceException("Autor", "Nome", dto.Nome);
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateAutorDto dto)
    {
        // Validação de schema
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Verifica se o autor existe
        var autor = await _autorRepository.GetByIdAsync(id);
        if (autor == null)
        {
            _logger.LogWarning("Tentativa de atualizar autor inexistente. ID: {AutorId}", id);
            throw new KeyNotFoundException($"Autor com ID {id} não encontrado.");
        }
                
        if (autor.Nome != dto.Nome)
        {
            var existente = await _autorRepository.GetByNomeAsync(dto.Nome);
            if (existente != null && existente.IdAutor != id)
            {
                _logger.LogWarning(
                    "Tentativa de atualizar autor para nome duplicado. ID: {AutorId}, Nome: {Nome}",
                    id, dto.Nome);
                throw new DuplicateResourceException("Autor", "Nome", dto.Nome);
            }
        }

        autor.Nome = dto.Nome;
        autor.Ativo = dto.Ativo;

        try
        {
            return await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                async () => await _autorRepository.UpdateAsync(autor),
                $"UpdateAutor - ID: {id}"
            );
        }
        catch (UniqueKeyViolationException)
        {
            throw new DuplicateResourceException("Autor", "Nome", dto.Nome);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Verifica se o autor existe
        var autor = await _autorRepository.GetByIdAsync(id);
        if (autor == null)
        {
            _logger.LogWarning("Tentativa de excluir autor inexistente. ID: {AutorId}", id);
            throw new KeyNotFoundException($"Autor com ID {id} não encontrado.");
        }
                
        var livrosAssociados = await _livroRepository.GetByAutorAsync(id);
        var livrosList = livrosAssociados.ToList();

        if (livrosList.Any())
        {
            _logger.LogWarning(
                "Tentativa de excluir autor com livros associados. AutorId: {AutorId}, Nome: {Nome}, LivrosCount: {Count}",
                id, autor.Nome, livrosList.Count);

            throw new ResourceInUseException(
                resourceType: "Autor",
                resourceId: id,
                resourceName: autor.Nome,
                dependentEntityType: "Livro",
                dependentCount: livrosList.Count
            );
        }

        try
        {            
            return await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                async () => await _autorRepository.DeleteAsync(id),
                $"DeleteAutor - ID: {id}, Nome: {autor.Nome}"
            );
        }
        catch (ForeignKeyViolationException)
        {
            // Se mesmo com a validação prévia houver violação FK (race condition)            
            var livrosAtualizados = await _livroRepository.GetByAutorAsync(id);
            var count = livrosAtualizados.Count();

            throw new ResourceInUseException(
                resourceType: "Autor",
                resourceId: id,
                resourceName: autor.Nome,
                dependentEntityType: "Livro",
                dependentCount: count
            );
        }
    }

    private static AutorDto MapToDto(Autor autor) => new()
    {
        IdAutor = autor.IdAutor,
        Nome = autor.Nome,
        Ativo = autor.Ativo
    };
}