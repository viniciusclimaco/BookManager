using BookManager.Application.Services;
using BookManager.Domain.Entities;
using BookManager.Domain.Exceptions;
using BookManager.Infrastructure.DTOs;
using BookManager.Infrastructure.Repositories.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookManager.Tests.Services;

/// <summary>
/// Testes unitários para validação de tratamento de erros - AutorService
/// Estes testes validam os cenários críticos identificados na análise
/// </summary>
public class AutorServiceErrorHandlingTests
{
    private readonly Mock<IAutorRepository> _autorRepositoryMock;
    private readonly Mock<ILivroRepository> _livroRepositoryMock;
    private readonly Mock<IValidator<CreateAutorDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateAutorDto>> _updateValidatorMock;
    private readonly Mock<ILogger<AutorService>> _loggerMock;
    private readonly AutorService _autorService;

    public AutorServiceErrorHandlingTests()
    {
        _autorRepositoryMock = new Mock<IAutorRepository>();
        _livroRepositoryMock = new Mock<ILivroRepository>();
        _createValidatorMock = new Mock<IValidator<CreateAutorDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateAutorDto>>();
        _loggerMock = new Mock<ILogger<AutorService>>();

        _autorService = new AutorService(
            _autorRepositoryMock.Object,
            _livroRepositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _loggerMock.Object
        );
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_QuandoNomeDuplicado_DeveLancarDuplicateResourceException()
    {
        // Arrange
        var dto = new CreateAutorDto { Nome = "J.K. Rowling" };
        var autorExistente = new Autor { IdAutor = 1, Nome = "J.K. Rowling" };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        _autorRepositoryMock
            .Setup(r => r.GetByNomeAsync(dto.Nome))
            .ReturnsAsync(autorExistente);

        // Act
        var act = async () => await _autorService.CreateAsync(dto);

        // Assert
        var exception = await act.Should().ThrowAsync<DuplicateResourceException>();
        exception.Which.ResourceType.Should().Be("Autor");
        exception.Which.FieldName.Should().Be("Nome");
        exception.Which.Value.Should().Be(dto.Nome);
        exception.Which.Message.Should().Contain("já existe");
    }

    [Fact]
    public async Task CreateAsync_QuandoDadosValidos_DeveCriarComSucesso()
    {
        // Arrange
        var dto = new CreateAutorDto { Nome = "Machado de Assis" };
        var novoAutorId = 42;

        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        _autorRepositoryMock
            .Setup(r => r.GetByNomeAsync(dto.Nome))
            .ReturnsAsync((Autor?)null);

        _autorRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Autor>()))
            .ReturnsAsync(novoAutorId);

        // Act
        var result = await _autorService.CreateAsync(dto);

        // Assert
        result.Should().Be(novoAutorId);
        _autorRepositoryMock.Verify(r => r.CreateAsync(It.Is<Autor>(a => a.Nome == dto.Nome)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_QuandoValidacaoFalha_DeveLancarValidationException()
    {
        // Arrange
        var dto = new CreateAutorDto { Nome = "" }; // Nome vazio
        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Nome", "Nome é obrigatório")
        };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(validationErrors));

        // Act
        var act = async () => await _autorService.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_QuandoAutorNaoExiste_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var autorId = 999;

        _autorRepositoryMock
            .Setup(r => r.GetByIdAsync(autorId))
            .ReturnsAsync((Autor?)null);

        // Act
        var act = async () => await _autorService.DeleteAsync(autorId);

        // Assert
        var exception = await act.Should().ThrowAsync<KeyNotFoundException>();
        exception.Which.Message.Should().Contain($"ID {autorId}");
    }

    [Fact]
    public async Task DeleteAsync_QuandoAutorTemLivrosAssociados_DeveLancarResourceInUseException()
    {
        // Arrange
        var autorId = 1;
        var autor = new Autor { IdAutor = autorId, Nome = "J.K. Rowling" };
        var livrosAssociados = new List<Livro>
        {
            new Livro { IdLivro = 1, Titulo = "Harry Potter 1" },
            new Livro { IdLivro = 2, Titulo = "Harry Potter 2" },
            new Livro { IdLivro = 3, Titulo = "Harry Potter 3" }
        };

        _autorRepositoryMock
            .Setup(r => r.GetByIdAsync(autorId))
            .ReturnsAsync(autor);

        _livroRepositoryMock
            .Setup(r => r.GetByAutorAsync(autorId))
            .ReturnsAsync(livrosAssociados);

        // Act
        var act = async () => await _autorService.DeleteAsync(autorId);

        // Assert
        var exception = await act.Should().ThrowAsync<ResourceInUseException>();
        exception.Which.ResourceType.Should().Be("Autor");
        exception.Which.ResourceId.Should().Be(autorId);
        exception.Which.ResourceName.Should().Be(autor.Nome);
        exception.Which.DependentEntityType.Should().Be("Livro");
        exception.Which.DependentCount.Should().Be(3);
        exception.Which.Message.Should().Contain("3 Livro(s) associado(s)");
    }

    [Fact]
    public async Task DeleteAsync_QuandoAutorSemLivros_DeveDeletarComSucesso()
    {
        // Arrange
        var autorId = 1;
        var autor = new Autor { IdAutor = autorId, Nome = "Autor Sem Livros" };
        var livrosAssociados = new List<Livro>(); // Vazio

        _autorRepositoryMock
            .Setup(r => r.GetByIdAsync(autorId))
            .ReturnsAsync(autor);

        _livroRepositoryMock
            .Setup(r => r.GetByAutorAsync(autorId))
            .ReturnsAsync(livrosAssociados);

        _autorRepositoryMock
            .Setup(r => r.DeleteAsync(autorId))
            .ReturnsAsync(true);

        // Act
        var result = await _autorService.DeleteAsync(autorId);

        // Assert
        result.Should().BeTrue();
        _autorRepositoryMock.Verify(r => r.DeleteAsync(autorId), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_QuandoAutorNaoExiste_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var autorId = 999;
        var dto = new UpdateAutorDto { Nome = "Novo Nome", Ativo = true };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        _autorRepositoryMock
            .Setup(r => r.GetByIdAsync(autorId))
            .ReturnsAsync((Autor?)null);

        // Act
        var act = async () => await _autorService.UpdateAsync(autorId, dto);

        // Assert
        var exception = await act.Should().ThrowAsync<KeyNotFoundException>();
        exception.Which.Message.Should().Contain($"ID {autorId}");
    }

    [Fact]
    public async Task UpdateAsync_QuandoNomeDuplicadoEmOutroAutor_DeveLancarDuplicateResourceException()
    {
        // Arrange
        var autorId = 1;
        var autor = new Autor { IdAutor = autorId, Nome = "Nome Antigo" };
        var dto = new UpdateAutorDto { Nome = "J.K. Rowling", Ativo = true };
        var autorComNomeDuplicado = new Autor { IdAutor = 2, Nome = "J.K. Rowling" };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        _autorRepositoryMock
            .Setup(r => r.GetByIdAsync(autorId))
            .ReturnsAsync(autor);

        _autorRepositoryMock
            .Setup(r => r.GetByNomeAsync(dto.Nome))
            .ReturnsAsync(autorComNomeDuplicado);

        // Act
        var act = async () => await _autorService.UpdateAsync(autorId, dto);

        // Assert
        var exception = await act.Should().ThrowAsync<DuplicateResourceException>();
        exception.Which.ResourceType.Should().Be("Autor");
        exception.Which.FieldName.Should().Be("Nome");
        exception.Which.Value.Should().Be(dto.Nome);
    }

    [Fact]
    public async Task UpdateAsync_QuandoNomeNaoMuda_NaoDeveVerificarDuplicacao()
    {
        // Arrange
        var autorId = 1;
        var nomeOriginal = "J.K. Rowling";
        var autor = new Autor { IdAutor = autorId, Nome = nomeOriginal };
        var dto = new UpdateAutorDto { Nome = nomeOriginal, Ativo = false };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        _autorRepositoryMock
            .Setup(r => r.GetByIdAsync(autorId))
            .ReturnsAsync(autor);

        _autorRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Autor>()))
            .ReturnsAsync(true);

        // Act
        var result = await _autorService.UpdateAsync(autorId, dto);

        // Assert
        result.Should().BeTrue();
        _autorRepositoryMock.Verify(r => r.GetByNomeAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion
}

/// <summary>
/// Testes unitários para validação de tratamento de erros - LivroService
/// Foco especial em ISBN duplicado
/// </summary>
public class LivroServiceErrorHandlingTests
{
    private readonly Mock<ILivroRepository> _livroRepositoryMock;
    private readonly Mock<IAutorRepository> _autorRepositoryMock;
    private readonly Mock<IAssuntoRepository> _assuntoRepositoryMock;
    private readonly Mock<IValidator<CreateLivroDto>> _createValidatorMock;
    private readonly Mock<ILogger<LivroService>> _loggerMock;

    [Fact]
    public async Task CreateAsync_QuandoISBNDuplicado_DeveLancarDuplicateResourceException()
    {
        // Arrange
        var livroRepositoryMock = new Mock<ILivroRepository>();
        var autorRepositoryMock = new Mock<IAutorRepository>();
        var assuntoRepositoryMock = new Mock<IAssuntoRepository>();
        var createValidatorMock = new Mock<IValidator<CreateLivroDto>>();
        var updateValidatorMock = new Mock<IValidator<UpdateLivroDto>>();
        var livroAutorRepositoryMock = new Mock<ILivroAutorRepository>();
        var livroPrecoRepositoryMock = new Mock<ILivroPrecoRepository>();
        var formaPagamentoRepositoryMock = new Mock<IFormaPagamentoRepository>();
        var loggerMock = new Mock<ILogger<LivroService>>();

        var livroService = new LivroService(
            livroRepositoryMock.Object,
            livroAutorRepositoryMock.Object,
            livroPrecoRepositoryMock.Object,
            autorRepositoryMock.Object,
            assuntoRepositoryMock.Object,
            formaPagamentoRepositoryMock.Object,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            loggerMock.Object
        );

        var dto = new CreateLivroDto
        {
            Titulo = "Novo Livro",
            ISBN = "978-0439708180", // ISBN duplicado
            IdAssunto = 1,
            IdAutores = new List<int> { 1 },
            Precos = new List<CreateLivroPrecoDto>()
        };

        var livroExistente = new Livro
        {
            IdLivro = 10,
            Titulo = "Harry Potter",
            ISBN = "978-0439708180"
        };

        createValidatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        livroRepositoryMock
            .Setup(r => r.GetByISBNAsync(dto.ISBN))
            .ReturnsAsync(livroExistente);

        // Act
        var act = async () => await livroService.CreateAsync(dto);

        // Assert
        var exception = await act.Should().ThrowAsync<DuplicateResourceException>();
        exception.Which.ResourceType.Should().Be("Livro");
        exception.Which.FieldName.Should().Be("ISBN");
        exception.Which.Value.Should().Be(dto.ISBN);
        exception.Which.Message.Should().Contain("já existe");
    }

    [Fact]
    public async Task UpdateAsync_QuandoISBNDuplicadoEmOutroLivro_DeveLancarDuplicateResourceException()
    {
        // Arrange
        var livroRepositoryMock = new Mock<ILivroRepository>();
        var autorRepositoryMock = new Mock<IAutorRepository>();
        var assuntoRepositoryMock = new Mock<IAssuntoRepository>();
        var createValidatorMock = new Mock<IValidator<CreateLivroDto>>();
        var updateValidatorMock = new Mock<IValidator<UpdateLivroDto>>();
        var livroAutorRepositoryMock = new Mock<ILivroAutorRepository>();
        var livroPrecoRepositoryMock = new Mock<ILivroPrecoRepository>();
        var formaPagamentoRepositoryMock = new Mock<IFormaPagamentoRepository>();
        var loggerMock = new Mock<ILogger<LivroService>>();

        var livroService = new LivroService(
            livroRepositoryMock.Object,
            livroAutorRepositoryMock.Object,
            livroPrecoRepositoryMock.Object,
            autorRepositoryMock.Object,
            assuntoRepositoryMock.Object,
            formaPagamentoRepositoryMock.Object,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            loggerMock.Object
        );

        var livroId = 1;
        var livroAtual = new Livro
        {
            IdLivro = livroId,
            Titulo = "Livro Atual",
            ISBN = "978-0000000001"
        };

        var livroComISBNDuplicado = new Livro
        {
            IdLivro = 2,
            Titulo = "Outro Livro",
            ISBN = "978-0439708180"
        };

        var dto = new UpdateLivroDto
        {
            Titulo = "Livro Atualizado",
            ISBN = "978-0439708180", // ISBN que já existe em outro livro
            IdAssunto = 1,
            IdAutores = new List<int> { 1 },
            Ativo = true,
            Precos = new List<UpdateLivroPrecoDto>()
        };

        updateValidatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        livroRepositoryMock
            .Setup(r => r.GetByIdAsync(livroId))
            .ReturnsAsync(livroAtual);

        livroRepositoryMock
            .Setup(r => r.GetByISBNAsync(dto.ISBN))
            .ReturnsAsync(livroComISBNDuplicado);

        // Act
        var act = async () => await livroService.UpdateAsync(livroId, dto);

        // Assert
        var exception = await act.Should().ThrowAsync<DuplicateResourceException>();
        exception.Which.ResourceType.Should().Be("Livro");
        exception.Which.FieldName.Should().Be("ISBN");
        exception.Which.Value.Should().Be(dto.ISBN);
    }
}

/*
 * CENÁRIOS DE TESTE COBERTOS:
 * 
 * 1. CreateAsync - Nome duplicado (Autor)
 * 2. CreateAsync - Dados válidos (sucesso)
 * 3. CreateAsync - Validação falha
 * 4. DeleteAsync - Autor não existe
 * 5. DeleteAsync - Autor com livros associados (CRÍTICO)
 * 6. DeleteAsync - Autor sem livros (sucesso)
 * 7. UpdateAsync - Autor não existe
 * 8. UpdateAsync - Nome duplicado em outro autor
 * 9. UpdateAsync - Nome não muda (skip validação)
 * 10. CreateAsync - ISBN duplicado (CRÍTICO)
 * 11. UpdateAsync - ISBN duplicado em outro livro (CRÍTICO)
 * 
 * ⚠️ TESTES ADICIONAIS RECOMENDADOS:
 * - AssuntoService.DeleteAsync com livros associados
 * - FormaPagamentoService com nome duplicado
 * - LivroAutor com combinação duplicada (IdLivro + IdAutor)
 * - LivroPreco com combinação duplicada (IdLivro + IdFormaPagamento)
 * - Deadlock e Timeout scenarios (usando mocks)
 */
