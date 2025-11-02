using BookManager.Application.Services;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.DTOs;
using BookManager.Infrastructure.Repositories.Interfaces;
using BookManager.Tests.Fixtures;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace BookManager.Tests.Services;

/// <summary>
/// Testes para LivroService
/// </summary>
public class LivroServiceTests
{
    private readonly Mock<ILivroRepository> _livroRepositoryMock;
    private readonly Mock<ILivroAutorRepository> _livroAutorRepositoryMock;
    private readonly Mock<ILivroPrecoRepository> _livroPrecoRepositoryMock;
    private readonly Mock<IAutorRepository> _autorRepositoryMock;
    private readonly Mock<IAssuntoRepository> _assuntoRepositoryMock;
    private readonly Mock<IFormaPagamentoRepository> _formaPagamentoRepositoryMock;
    private readonly Mock<IValidator<CreateLivroDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateLivroDto>> _updateValidatorMock;
    private readonly Mock<ILogger<LivroService>> _loggerMock;
    private readonly LivroService _livroService;

    public LivroServiceTests()
    {
        _livroRepositoryMock = new Mock<ILivroRepository>();
        _livroAutorRepositoryMock = new Mock<ILivroAutorRepository>();
        _livroPrecoRepositoryMock = new Mock<ILivroPrecoRepository>();
        _autorRepositoryMock = new Mock<IAutorRepository>();
        _assuntoRepositoryMock = new Mock<IAssuntoRepository>();
        _formaPagamentoRepositoryMock = new Mock<IFormaPagamentoRepository>();
        _createValidatorMock = new Mock<IValidator<CreateLivroDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateLivroDto>>();
        _loggerMock = new Mock<ILogger<LivroService>>();

        _livroService = new LivroService(
            _livroRepositoryMock.Object,
            _livroAutorRepositoryMock.Object,
            _livroPrecoRepositoryMock.Object,
            _autorRepositoryMock.Object,
            _assuntoRepositoryMock.Object,
            _formaPagamentoRepositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _loggerMock.Object
        );
    }

    #region GetByIdAsync Tests
    [Fact]
    public async Task GetByIdAsync_ComIdValido_DeveRetornarLivroComDetalhes()
    {
        // Arrange
        var livro = TestDataFixture.GetLivroValido();
        var assunto = TestDataFixture.GetAssuntoValido();
        var autor = TestDataFixture.GetAutorValido();
        var livroAutor = TestDataFixture.GetLivroAutorValido();
        var livroPreco = TestDataFixture.GetLivroPrecoValido();
        var formaPagamento = TestDataFixture.GetFormaPagamentoValida();

        _livroRepositoryMock.Setup(r => r.GetWithDetailsAsync(livro.IdLivro))
            .ReturnsAsync(livro);
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(livro.IdAssunto))
            .ReturnsAsync(assunto);
        _livroAutorRepositoryMock.Setup(r => r.GetByLivroAsync(livro.IdLivro))
            .ReturnsAsync(new List<LivroAutor> { livroAutor });
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(livroAutor.IdAutor))
            .ReturnsAsync(autor);
        _livroPrecoRepositoryMock.Setup(r => r.GetByLivroAsync(livro.IdLivro))
            .ReturnsAsync(new List<LivroPreco> { livroPreco });
        _formaPagamentoRepositoryMock.Setup(r => r.GetByIdAsync(livroPreco.IdFormaPagamento))
            .ReturnsAsync(formaPagamento);

        // Act
        var result = await _livroService.GetByIdAsync(livro.IdLivro);

        // Assert
        result.Should().NotBeNull();
        result!.IdLivro.Should().Be(livro.IdLivro);
        result.Titulo.Should().Be(livro.Titulo);
        result.Autores.Should().HaveCount(1);
        result.Precos.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var idInexistente = 999;
        _livroRepositoryMock.Setup(r => r.GetWithDetailsAsync(idInexistente))
            .ReturnsAsync((Livro?)null);

        // Act
        var result = await _livroService.GetByIdAsync(idInexistente);

        // Assert
        result.Should().BeNull();
    }
    #endregion

    #region GetAllAsync Tests
    [Fact]
    public async Task GetAllAsync_DeveRetornarListaDeLivros()
    {
        // Arrange
        var livros = TestDataFixture.GetListaLivros();
        var assunto = TestDataFixture.GetAssuntoValido();

        _livroRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(livros);
        
        foreach (var livro in livros)
        {
            _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(livro.IdAssunto))
                .ReturnsAsync(assunto);
            _livroAutorRepositoryMock.Setup(r => r.GetByLivroAsync(livro.IdLivro))
                .ReturnsAsync(new List<LivroAutor>());
            _livroPrecoRepositoryMock.Setup(r => r.GetByLivroAsync(livro.IdLivro))
                .ReturnsAsync(new List<LivroPreco>());
        }

        // Act
        var result = await _livroService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(livros.Count);
    }
    #endregion

    #region GetByAssuntoAsync Tests
    [Fact]
    public async Task GetByAssuntoAsync_DeveRetornarLivrosDoAssunto()
    {
        // Arrange
        var idAssunto = 1;
        var livros = TestDataFixture.GetListaLivros();
        var assunto = TestDataFixture.GetAssuntoValido();

        _livroRepositoryMock.Setup(r => r.GetByAssuntoAsync(idAssunto))
            .ReturnsAsync(livros);
        
        foreach (var livro in livros)
        {
            _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(livro.IdAssunto))
                .ReturnsAsync(assunto);
            _livroAutorRepositoryMock.Setup(r => r.GetByLivroAsync(livro.IdLivro))
                .ReturnsAsync(new List<LivroAutor>());
            _livroPrecoRepositoryMock.Setup(r => r.GetByLivroAsync(livro.IdLivro))
                .ReturnsAsync(new List<LivroPreco>());
        }

        // Act
        var result = await _livroService.GetByAssuntoAsync(idAssunto);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(livros.Count);
    }
    #endregion

    #region CreateAsync Tests
    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveCriarLivro()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        var idEsperado = 1;
        var assunto = TestDataFixture.GetAssuntoValido();
        var autor = TestDataFixture.GetAutorValido();
        var formaPagamento = TestDataFixture.GetFormaPagamentoValida();

        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(dto.IdAssunto))
            .ReturnsAsync(assunto);
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(autor);
        _formaPagamentoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(formaPagamento);
        _livroRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Livro>()))
            .ReturnsAsync(idEsperado);
        _livroAutorRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<LivroAutor>()))
            .ReturnsAsync(1);
        _livroPrecoRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<LivroPreco>()))
            .ReturnsAsync(1);

        // Act
        var result = await _livroService.CreateAsync(dto);

        // Assert
        result.Should().Be(idEsperado);
        _livroRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Livro>()), Times.Once);
        _livroAutorRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<LivroAutor>()), Times.Exactly(dto.IdAutores.Count));
        _livroPrecoRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<LivroPreco>()), Times.Exactly(dto.Precos.Count));
    }

    [Fact]
    public async Task CreateAsync_ComAssuntoInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();

        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(dto.IdAssunto))
            .ReturnsAsync((Assunto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _livroService.CreateAsync(dto)
        );
        
        exception.Message.Should().Contain($"Assunto com ID {dto.IdAssunto} não encontrado");
        _livroRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Livro>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ComAutorInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        var assunto = TestDataFixture.GetAssuntoValido();

        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(dto.IdAssunto))
            .ReturnsAsync(assunto);
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Autor?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _livroService.CreateAsync(dto)
        );
        
        exception.Message.Should().Contain("Autor com ID");
        _livroRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Livro>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ComFormaPagamentoInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        var assunto = TestDataFixture.GetAssuntoValido();
        var autor = TestDataFixture.GetAutorValido();

        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(dto.IdAssunto))
            .ReturnsAsync(assunto);
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(autor);
        _formaPagamentoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((FormaPagamento?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _livroService.CreateAsync(dto)
        );
        
        exception.Message.Should().Contain("Forma de Pagamento com ID");
        _livroRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Livro>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ComDadosInvalidos_DeveLancarValidationException()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Titulo", "Título é obrigatório")
        };

        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await _livroService.CreateAsync(dto)
        );
        
        _livroRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Livro>()), Times.Never);
    }
    #endregion

    #region UpdateAsync Tests
    [Fact]
    public async Task UpdateAsync_ComDadosValidos_DeveAtualizarLivro()
    {
        // Arrange
        var id = 1;
        var dto = TestDataFixture.GetUpdateLivroDtoValido();
        var livro = TestDataFixture.GetLivroValido();
        var assunto = TestDataFixture.GetAssuntoValido();
        var autor = TestDataFixture.GetAutorValido();

        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _livroRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(livro);
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(dto.IdAssunto))
            .ReturnsAsync(assunto);
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(autor);
        _livroRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Livro>()))
            .ReturnsAsync(true);
        _livroAutorRepositoryMock.Setup(r => r.DeleteByLivroAsync(id))
            .ReturnsAsync(true);
        _livroAutorRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<LivroAutor>()))
            .ReturnsAsync(1);
        _livroPrecoRepositoryMock.Setup(r => r.DeleteByLivroAsync(id))
            .ReturnsAsync(true);
        _livroPrecoRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<LivroPreco>()))
            .ReturnsAsync(1);

        // Act
        var result = await _livroService.UpdateAsync(id, dto);

        // Assert
        result.Should().BeTrue();
        _livroRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Livro>()), Times.Once);
        _livroAutorRepositoryMock.Verify(r => r.DeleteByLivroAsync(id), Times.Once);
        _livroPrecoRepositoryMock.Verify(r => r.DeleteByLivroAsync(id), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var id = 999;
        var dto = TestDataFixture.GetUpdateLivroDtoValido();

        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _livroRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Livro?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _livroService.UpdateAsync(id, dto)
        );
        
        exception.Message.Should().Contain($"Livro com ID {id} não encontrado");
        _livroRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Livro>()), Times.Never);
    }
    #endregion

    #region DeleteAsync Tests
    [Fact]
    public async Task DeleteAsync_ComIdValido_DeveDeletarLivro()
    {
        // Arrange
        var id = 1;
        var livro = TestDataFixture.GetLivroValido();

        _livroRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(livro);
        _livroRepositoryMock.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        // Act
        var result = await _livroService.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _livroRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var id = 999;

        _livroRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Livro?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _livroService.DeleteAsync(id)
        );
        
        exception.Message.Should().Contain($"Livro com ID {id} não encontrado");
        _livroRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Never);
    }
    #endregion
}
