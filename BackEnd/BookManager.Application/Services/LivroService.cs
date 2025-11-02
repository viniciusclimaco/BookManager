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
/// Serviço de aplicação para Livro
/// </summary>
public class LivroService : ILivroService
{
    private readonly ILivroRepository _livroRepository;
    private readonly ILivroAutorRepository _livroAutorRepository;
    private readonly ILivroPrecoRepository _livroPrecoRepository;
    private readonly IAutorRepository _autorRepository;
    private readonly IAssuntoRepository _assuntoRepository;
    private readonly IFormaPagamentoRepository _formaPagamentoRepository;
    private readonly IValidator<CreateLivroDto> _createValidator;
    private readonly IValidator<UpdateLivroDto> _updateValidator;
    private readonly ILogger<LivroService> _logger;

    public LivroService(
        ILivroRepository livroRepository,
        ILivroAutorRepository livroAutorRepository,
        ILivroPrecoRepository livroPrecoRepository,
        IAutorRepository autorRepository,
        IAssuntoRepository assuntoRepository,
        IFormaPagamentoRepository formaPagamentoRepository,
        IValidator<CreateLivroDto> createValidator,
        IValidator<UpdateLivroDto> updateValidator,
        ILogger<LivroService> logger)
    {
        _livroRepository = livroRepository ?? throw new ArgumentNullException(nameof(livroRepository));
        _livroAutorRepository = livroAutorRepository ?? throw new ArgumentNullException(nameof(livroAutorRepository));
        _livroPrecoRepository = livroPrecoRepository ?? throw new ArgumentNullException(nameof(livroPrecoRepository));
        _autorRepository = autorRepository ?? throw new ArgumentNullException(nameof(autorRepository));
        _assuntoRepository = assuntoRepository ?? throw new ArgumentNullException(nameof(assuntoRepository));
        _formaPagamentoRepository = formaPagamentoRepository ?? throw new ArgumentNullException(nameof(formaPagamentoRepository));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LivroDto?> GetByIdAsync(int id)
    {
        var livro = await _livroRepository.GetWithDetailsAsync(id);
        if (livro == null) return null;

        return await MapToDtoAsync(livro);
    }

    public async Task<IEnumerable<LivroDto>> GetAllAsync()
    {
        var livros = await _livroRepository.GetAllAsync();
        var dtos = new List<LivroDto>();

        foreach (var livro in livros)
        {
            dtos.Add(await MapToDtoAsync(livro));
        }

        return dtos;
    }

    public async Task<IEnumerable<LivroDto>> GetByAssuntoAsync(int idAssunto)
    {
        var livros = await _livroRepository.GetByAssuntoAsync(idAssunto);
        var dtos = new List<LivroDto>();

        foreach (var livro in livros)
        {
            dtos.Add(await MapToDtoAsync(livro));
        }

        return dtos;
    }

    public async Task<IEnumerable<LivroDto>> GetByAutorAsync(int idAutor)
    {
        var livros = await _livroRepository.GetByAutorAsync(idAutor);
        var dtos = new List<LivroDto>();

        foreach (var livro in livros)
        {
            dtos.Add(await MapToDtoAsync(livro));
        }

        return dtos;
    }

    public async Task<IEnumerable<LivroDto>> GetAtivoAsync()
    {
        var livros = await _livroRepository.GetByAtivo(true);
        var dtos = new List<LivroDto>();

        foreach (var livro in livros)
        {
            dtos.Add(await MapToDtoAsync(livro));
        }

        return dtos;
    }

    public async Task<int> CreateAsync(CreateLivroDto dto)
    {
        // Validação de schema
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
                
        if (!string.IsNullOrWhiteSpace(dto.ISBN))
        {
            var livroExistente = await _livroRepository.GetByISBNAsync(dto.ISBN);
            if (livroExistente != null)
            {
                _logger.LogWarning(
                    "Tentativa de criar livro com ISBN duplicado. ISBN: {ISBN}, LivroExistente: {LivroId}",
                    dto.ISBN, livroExistente.IdLivro);

                throw new DuplicateResourceException("Livro", "ISBN", dto.ISBN);
            }
        }

        // Validações de relacionamentos
        var assunto = await _assuntoRepository.GetByIdAsync(dto.IdAssunto);
        if (assunto == null)
        {
            _logger.LogWarning("Tentativa de criar livro com assunto inexistente. IdAssunto: {IdAssunto}", dto.IdAssunto);
            throw new KeyNotFoundException($"Assunto com ID {dto.IdAssunto} não encontrado.");
        }

        foreach (var idAutor in dto.IdAutores)
        {
            var autor = await _autorRepository.GetByIdAsync(idAutor);
            if (autor == null)
            {
                _logger.LogWarning("Tentativa de criar livro com autor inexistente. IdAutor: {IdAutor}", idAutor);
                throw new KeyNotFoundException($"Autor com ID {idAutor} não encontrado.");
            }
        }

        foreach (var precoDto in dto.Precos)
        {
            var formaPagamento = await _formaPagamentoRepository.GetByIdAsync(precoDto.IdFormaPagamento);
            if (formaPagamento == null)
            {
                _logger.LogWarning(
                    "Tentativa de criar livro com forma de pagamento inexistente. IdFormaPagamento: {IdFormaPagamento}",
                    precoDto.IdFormaPagamento);
                throw new KeyNotFoundException($"Forma de Pagamento com ID {precoDto.IdFormaPagamento} não encontrada.");
            }
        }

        // Criar livro
        var livro = new Livro
        {
            Titulo = dto.Titulo,
            Editora = dto.Editora,
            AnoPublicacao = dto.AnoPublicacao,
            ISBN = dto.ISBN,
            IdAssunto = dto.IdAssunto,
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };

        int idLivro;
        try
        {            
            idLivro = await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                async () => await _livroRepository.CreateAsync(livro),
                $"CreateLivro - Título: {dto.Titulo}, ISBN: {dto.ISBN}"
            );
        }
        catch (UniqueKeyViolationException)
        {            
            throw new DuplicateResourceException("Livro", "ISBN", dto.ISBN);
        }

        // Adicionar autores
        for (int ordem = 0; ordem < dto.IdAutores.Count; ordem++)
        {
            var livroAutor = new LivroAutor
            {
                IdLivro = idLivro,
                IdAutor = dto.IdAutores[ordem],
                Ordem = ordem + 1,
                DataCadastro = DateTime.UtcNow
            };

            try
            {
                await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                    async () => await _livroAutorRepository.CreateAsync(livroAutor),
                    $"CreateLivroAutor - IdLivro: {idLivro}, IdAutor: {dto.IdAutores[ordem]}"
                );
            }
            catch (UniqueKeyViolationException)
            {
                // Autor duplicado para o mesmo livro
                var autor = await _autorRepository.GetByIdAsync(dto.IdAutores[ordem]);
                throw new InvalidOperationException(
                    $"O autor '{autor?.Nome}' já está associado a este livro.");
            }
        }

        // Adicionar preços
        foreach (var precoDto in dto.Precos)
        {
            var livroPreco = new LivroPreco
            {
                IdLivro = idLivro,
                IdFormaPagamento = precoDto.IdFormaPagamento,
                Valor = precoDto.Valor,
                DataCadastro = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            try
            {
                await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                    async () => await _livroPrecoRepository.CreateAsync(livroPreco),
                    $"CreateLivroPreco - IdLivro: {idLivro}, IdFormaPagamento: {precoDto.IdFormaPagamento}"
                );
            }
            catch (UniqueKeyViolationException)
            {
                // Preço duplicado para mesma forma de pagamento
                var forma = await _formaPagamentoRepository.GetByIdAsync(precoDto.IdFormaPagamento);
                throw new InvalidOperationException(
                    $"Já existe um preço cadastrado para a forma de pagamento '{forma?.Nome}'.");
            }
        }

        _logger.LogInformation(
            "Livro criado com sucesso. ID: {LivroId}, Título: {Titulo}, ISBN: {ISBN}",
            idLivro, dto.Titulo, dto.ISBN);

        return idLivro;
    }

    public async Task<bool> UpdateAsync(int id, UpdateLivroDto dto)
    {
        // Validação de schema
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var livro = await _livroRepository.GetByIdAsync(id);
        if (livro == null)
        {
            _logger.LogWarning("Tentativa de atualizar livro inexistente. ID: {LivroId}", id);
            throw new KeyNotFoundException($"Livro com ID {id} não encontrado.");
        }
                
        if (!string.IsNullOrWhiteSpace(dto.ISBN) && livro.ISBN != dto.ISBN)
        {
            var livroExistente = await _livroRepository.GetByISBNAsync(dto.ISBN);
            if (livroExistente != null && livroExistente.IdLivro != id)
            {
                _logger.LogWarning(
                    "Tentativa de atualizar livro para ISBN duplicado. ID: {LivroId}, ISBN: {ISBN}, LivroExistente: {LivroExistenteId}",
                    id, dto.ISBN, livroExistente.IdLivro);

                throw new DuplicateResourceException("Livro", "ISBN", dto.ISBN);
            }
        }

        // Validações de relacionamentos
        var assunto = await _assuntoRepository.GetByIdAsync(dto.IdAssunto);
        if (assunto == null)
        {
            _logger.LogWarning("Tentativa de atualizar livro com assunto inexistente. IdAssunto: {IdAssunto}", dto.IdAssunto);
            throw new KeyNotFoundException($"Assunto com ID {dto.IdAssunto} não encontrado.");
        }

        foreach (var idAutor in dto.IdAutores)
        {
            var autor = await _autorRepository.GetByIdAsync(idAutor);
            if (autor == null)
            {
                _logger.LogWarning("Tentativa de atualizar livro com autor inexistente. IdAutor: {IdAutor}", idAutor);
                throw new KeyNotFoundException($"Autor com ID {idAutor} não encontrado.");
            }
        }

        // Atualizar livro
        livro.Titulo = dto.Titulo;
        livro.Editora = dto.Editora;
        livro.AnoPublicacao = dto.AnoPublicacao;
        livro.ISBN = dto.ISBN;
        livro.IdAssunto = dto.IdAssunto;
        livro.Ativo = dto.Ativo;

        bool result;
        try
        {
            result = await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                async () => await _livroRepository.UpdateAsync(livro),
                $"UpdateLivro - ID: {id}"
            );
        }
        catch (UniqueKeyViolationException)
        {
            throw new DuplicateResourceException("Livro", "ISBN", dto.ISBN);
        }

        if (result)
        {
            // Atualizar autores (remove todos e adiciona novamente)
            await _livroAutorRepository.DeleteByLivroAsync(id);
            for (int ordem = 0; ordem < dto.IdAutores.Count; ordem++)
            {
                var livroAutor = new LivroAutor
                {
                    IdLivro = id,
                    IdAutor = dto.IdAutores[ordem],
                    Ordem = ordem + 1,
                    DataCadastro = DateTime.UtcNow
                };
                await _livroAutorRepository.CreateAsync(livroAutor);
            }

            // Atualizar preços (remove todos e adiciona novamente)
            await _livroPrecoRepository.DeleteByLivroAsync(id);
            foreach (var precoDto in dto.Precos)
            {
                var livroPreco = new LivroPreco
                {
                    IdLivro = id,
                    IdFormaPagamento = precoDto.IdFormaPagamento,
                    Valor = precoDto.Valor,
                    DataCadastro = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow
                };
                await _livroPrecoRepository.CreateAsync(livroPreco);
            }

            _logger.LogInformation(
                "Livro atualizado com sucesso. ID: {LivroId}, Título: {Titulo}",
                id, dto.Titulo);
        }

        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var livro = await _livroRepository.GetByIdAsync(id);
        if (livro == null)
        {
            _logger.LogWarning("Tentativa de excluir livro inexistente. ID: {LivroId}", id);
            throw new KeyNotFoundException($"Livro com ID {id} não encontrado.");
        }

        try
        {
            var result = await SqlExceptionHandler.ExecuteWithSqlExceptionHandlingAsync(
                async () => await _livroRepository.DeleteAsync(id),
                $"DeleteLivro - ID: {id}, Título: {livro.Titulo}"
            );

            if (result)
            {
                _logger.LogInformation(
                    "Livro excluído com sucesso. ID: {LivroId}, Título: {Titulo}",
                    id, livro.Titulo);
            }

            return result;
        }
        catch (ForeignKeyViolationException)
        {
            // Não deveria acontecer pois as FKs têm ON DELETE CASCADE,
            // mas mantemos o tratamento por segurança
            throw new InvalidOperationException(
                $"Não é possível excluir o livro '{livro.Titulo}' devido a registros relacionados.");
        }
    }

    private async Task<LivroDto> MapToDtoAsync(Livro livro)
    {
        var assunto = await _assuntoRepository.GetByIdAsync(livro.IdAssunto);
        var autores = await _livroAutorRepository.GetByLivroAsync(livro.IdLivro);
        var precos = await _livroPrecoRepository.GetByLivroAsync(livro.IdLivro);

        var dtoAutores = new List<LivroAutorDto>();
        foreach (var livroAutor in autores.OrderBy(x => x.Ordem))
        {
            var autor = await _autorRepository.GetByIdAsync(livroAutor.IdAutor);
            if (autor != null)
            {
                dtoAutores.Add(new LivroAutorDto
                {
                    IdAutor = autor.IdAutor,
                    Nome = autor.Nome,
                    Ordem = livroAutor.Ordem
                });
            }
        }

        var dtoPrecos = new List<LivroPrecoDto>();
        foreach (var livroPreco in precos)
        {
            var formaPagamento = await _formaPagamentoRepository.GetByIdAsync(livroPreco.IdFormaPagamento);
            dtoPrecos.Add(new LivroPrecoDto
            {
                IdLivroPreco = livroPreco.IdLivroPreco,
                IdFormaPagamento = livroPreco.IdFormaPagamento,
                FormaPagamentoNome = formaPagamento?.Nome,
                Valor = livroPreco.Valor
            });
        }

        return new LivroDto
        {
            IdLivro = livro.IdLivro,
            Titulo = livro.Titulo,
            Editora = livro.Editora,
            AnoPublicacao = livro.AnoPublicacao,
            ISBN = livro.ISBN,
            IdAssunto = livro.IdAssunto,
            AssuntoDescricao = assunto?.Descricao,
            Ativo = livro.Ativo,
            Autores = dtoAutores,
            Precos = dtoPrecos
        };
    }
}