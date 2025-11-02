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
/// Testes para AutorService
/// </summary>
public class AutorServiceTests
{
    private readonly Mock<IAutorRepository> _autorRepositoryMock;
    private readonly Mock<ILivroRepository> _livroRepositoryMock;
    private readonly Mock<IValidator<CreateAutorDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateAutorDto>> _updateValidatorMock;
    private readonly Mock<ILogger<AutorService>> _loggerMock;
    private readonly AutorService _autorService;

    public AutorServiceTests()
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

    #region GetByIdAsync Tests
    [Fact]
    public async Task GetByIdAsync_ComIdValido_DeveRetornarAutor()
    {
        // Arrange
        var autor = TestDataFixture.GetAutorValido();
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(autor.IdAutor))
            .ReturnsAsync(autor);

        // Act
        var result = await _autorService.GetByIdAsync(autor.IdAutor);

        // Assert
        result.Should().NotBeNull();
        result!.IdAutor.Should().Be(autor.IdAutor);
        result.Nome.Should().Be(autor.Nome);
        _autorRepositoryMock.Verify(r => r.GetByIdAsync(autor.IdAutor), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var idInexistente = 999;
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(idInexistente))
            .ReturnsAsync((Autor?)null);

        // Act
        var result = await _autorService.GetByIdAsync(idInexistente);

        // Assert
        result.Should().BeNull();
        _autorRepositoryMock.Verify(r => r.GetByIdAsync(idInexistente), Times.Once);
    }
    #endregion

    #region GetAllAsync Tests
    [Fact]
    public async Task GetAllAsync_DeveRetornarListaDeAutores()
    {
        // Arrange
        var autores = TestDataFixture.GetListaAutores();
        _autorRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(autores);

        // Act
        var result = await _autorService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(autores.Count);
        _autorRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_SemAutores_DeveRetornarListaVazia()
    {
        // Arrange
        _autorRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Autor>());

        // Act
        var result = await _autorService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _autorRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
    #endregion

    #region GetAtivoAsync Tests
    [Fact]
    public async Task GetAtivoAsync_DeveRetornarApenasAutoresAtivos()
    {
        // Arrange
        var autores = TestDataFixture.GetListaAutores();
        var autoresAtivos = autores.Where(a => a.Ativo).ToList();
        _autorRepositoryMock.Setup(r => r.GetByAtivo(true))
            .ReturnsAsync(autoresAtivos);

        // Act
        var result = await _autorService.GetAtivoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(autoresAtivos.Count);
        result.Should().OnlyContain(a => a.Ativo);
        _autorRepositoryMock.Verify(r => r.GetByAtivo(true), Times.Once);
    }
    #endregion

    #region CreateAsync Tests
    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveCriarAutor()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAutorDtoValido();
        var idEsperado = 1;
        
        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        _autorRepositoryMock.Setup(r => r.GetByNomeAsync(dto.Nome))
            .ReturnsAsync((Autor?)null);
        
        _autorRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Autor>()))
            .ReturnsAsync(idEsperado);

        // Act
        var result = await _autorService.CreateAsync(dto);

        // Assert
        result.Should().Be(idEsperado);
        _createValidatorMock.Verify(v => v.ValidateAsync(dto, default), Times.Once);
        _autorRepositoryMock.Verify(r => r.GetByNomeAsync(dto.Nome), Times.Once);
        _autorRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Autor>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ComDadosInvalidos_DeveLancarValidationException()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAutorDtoValido();
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Nome", "Nome é obrigatório")
        };
        
        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await _autorService.CreateAsync(dto)
        );
        
        _createValidatorMock.Verify(v => v.ValidateAsync(dto, default), Times.Once);
        _autorRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Autor>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ComNomeDuplicado_DeveLancarInvalidOperationException()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAutorDtoValido();
        var autorExistente = TestDataFixture.GetAutorValido();
        
        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        _autorRepositoryMock.Setup(r => r.GetByNomeAsync(dto.Nome))
            .ReturnsAsync(autorExistente);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _autorService.CreateAsync(dto)
        );
        
        _autorRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Autor>()), Times.Never);
    }
    #endregion

    #region UpdateAsync Tests
    [Fact]
    public async Task UpdateAsync_ComDadosValidos_DeveAtualizarAutor()
    {
        // Arrange
        var id = 1;
        var dto = TestDataFixture.GetUpdateAutorDtoValido();
        var autorExistente = TestDataFixture.GetAutorValido();
        
        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(autorExistente);
        
        _autorRepositoryMock.Setup(r => r.GetByNomeAsync(dto.Nome))
            .ReturnsAsync((Autor?)null);
        
        _autorRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Autor>()))
            .ReturnsAsync(true);

        // Act
        var result = await _autorService.UpdateAsync(id, dto);

        // Assert
        result.Should().BeTrue();
        _updateValidatorMock.Verify(v => v.ValidateAsync(dto, default), Times.Once);
        _autorRepositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _autorRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Autor>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var id = 999;
        var dto = TestDataFixture.GetUpdateAutorDtoValido();
        
        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Autor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _autorService.UpdateAsync(id, dto)
        );
        
        _autorRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Autor>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ComNomeDuplicado_DeveLancarInvalidOperationException()
    {
        // Arrange
        var id = 1;
        var dto = TestDataFixture.GetUpdateAutorDtoValido();
        var autorExistente = TestDataFixture.GetAutorValido();
        var autorComMesmoNome = TestDataFixture.GetAutorValido();
        autorComMesmoNome.IdAutor = 2;
        
        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(autorExistente);
        
        _autorRepositoryMock.Setup(r => r.GetByNomeAsync(dto.Nome))
            .ReturnsAsync(autorComMesmoNome);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _autorService.UpdateAsync(id, dto)
        );
        
        _autorRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Autor>()), Times.Never);
    }
    #endregion

    #region DeleteAsync Tests
    [Fact]
    public async Task DeleteAsync_ComIdValido_DeveDeletarAutor()
    {
        // Arrange
        var id = 1;
        var autor = TestDataFixture.GetAutorValido();
        
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(autor);
        
        _autorRepositoryMock.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        // Act
        var result = await _autorService.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _autorRepositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _autorRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var id = 999;
        
        _autorRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Autor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _autorService.DeleteAsync(id)
        );
        
        _autorRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Never);
    }
    #endregion
}
