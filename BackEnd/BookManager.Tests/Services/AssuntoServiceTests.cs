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
/// Testes para AssuntoService
/// </summary>
public class AssuntoServiceTests
{
    private readonly Mock<IAssuntoRepository> _assuntoRepositoryMock;
    private readonly Mock<ILivroRepository> _livroRepositoryMock;
    private readonly Mock<IValidator<CreateAssuntoDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateAssuntoDto>> _updateValidatorMock;
    private readonly Mock<ILogger<AssuntoService>> _loggerMock;
    private readonly AssuntoService _assuntoService;

    public AssuntoServiceTests()
    {
        _assuntoRepositoryMock = new Mock<IAssuntoRepository>();
        _livroRepositoryMock = new Mock<ILivroRepository>();
        _createValidatorMock = new Mock<IValidator<CreateAssuntoDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateAssuntoDto>>();
        _loggerMock = new Mock<ILogger<AssuntoService>>();
        
        _assuntoService = new AssuntoService(
            _assuntoRepositoryMock.Object,
            _livroRepositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _loggerMock.Object
        );
    }

    #region GetByIdAsync Tests
    [Fact]
    public async Task GetByIdAsync_ComIdValido_DeveRetornarAssunto()
    {
        // Arrange
        var assunto = TestDataFixture.GetAssuntoValido();
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(assunto.IdAssunto))
            .ReturnsAsync(assunto);

        // Act
        var result = await _assuntoService.GetByIdAsync(assunto.IdAssunto);

        // Assert
        result.Should().NotBeNull();
        result!.IdAssunto.Should().Be(assunto.IdAssunto);
        result.Descricao.Should().Be(assunto.Descricao);
        _assuntoRepositoryMock.Verify(r => r.GetByIdAsync(assunto.IdAssunto), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var idInexistente = 999;
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(idInexistente))
            .ReturnsAsync((Assunto?)null);

        // Act
        var result = await _assuntoService.GetByIdAsync(idInexistente);

        // Assert
        result.Should().BeNull();
        _assuntoRepositoryMock.Verify(r => r.GetByIdAsync(idInexistente), Times.Once);
    }
    #endregion

    #region GetAllAsync Tests
    [Fact]
    public async Task GetAllAsync_DeveRetornarListaDeAssuntos()
    {
        // Arrange
        var assuntos = TestDataFixture.GetListaAssuntos();
        _assuntoRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(assuntos);

        // Act
        var result = await _assuntoService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(assuntos.Count);
        _assuntoRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_SemAssuntos_DeveRetornarListaVazia()
    {
        // Arrange
        _assuntoRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Assunto>());

        // Act
        var result = await _assuntoService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _assuntoRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
    #endregion

    #region GetAtivoAsync Tests
    [Fact]
    public async Task GetAtivoAsync_DeveRetornarApenasAssuntosAtivos()
    {
        // Arrange
        var assuntos = TestDataFixture.GetListaAssuntos();
        var assuntosAtivos = assuntos.Where(a => a.Ativo).ToList();
        _assuntoRepositoryMock.Setup(r => r.GetByAtivo(true))
            .ReturnsAsync(assuntosAtivos);

        // Act
        var result = await _assuntoService.GetAtivoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(assuntosAtivos.Count);
        result.Should().OnlyContain(a => a.Ativo);
        _assuntoRepositoryMock.Verify(r => r.GetByAtivo(true), Times.Once);
    }
    #endregion

    #region CreateAsync Tests
    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveCriarAssunto()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAssuntoDtoValido();
        var idEsperado = 1;
        
        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        _assuntoRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Assunto>()))
            .ReturnsAsync(idEsperado);

        // Act
        var result = await _assuntoService.CreateAsync(dto);

        // Assert
        result.Should().Be(idEsperado);
        _createValidatorMock.Verify(v => v.ValidateAsync(dto, default), Times.Once);
        _assuntoRepositoryMock.Verify(r => r.CreateAsync(It.Is<Assunto>(a => 
            a.Descricao == dto.Descricao && a.Ativo == true
        )), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ComDadosInvalidos_DeveLancarValidationException()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAssuntoDtoValido();
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Descricao", "Descrição é obrigatória")
        };
        
        _createValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await _assuntoService.CreateAsync(dto)
        );
        
        _createValidatorMock.Verify(v => v.ValidateAsync(dto, default), Times.Once);
        _assuntoRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Assunto>()), Times.Never);
    }
    #endregion

    #region UpdateAsync Tests
    [Fact]
    public async Task UpdateAsync_ComDadosValidos_DeveAtualizarAssunto()
    {
        // Arrange
        var id = 1;
        var dto = TestDataFixture.GetUpdateAssuntoDtoValido();
        var assuntoExistente = TestDataFixture.GetAssuntoValido();
        
        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(assuntoExistente);
        
        _assuntoRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Assunto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _assuntoService.UpdateAsync(id, dto);

        // Assert
        result.Should().BeTrue();
        _updateValidatorMock.Verify(v => v.ValidateAsync(dto, default), Times.Once);
        _assuntoRepositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _assuntoRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Assunto>(a =>
            a.IdAssunto == id && a.Descricao == dto.Descricao && a.Ativo == dto.Ativo
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var id = 999;
        var dto = TestDataFixture.GetUpdateAssuntoDtoValido();
        
        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Assunto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _assuntoService.UpdateAsync(id, dto)
        );
        
        _assuntoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Assunto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ComDadosInvalidos_DeveLancarValidationException()
    {
        // Arrange
        var id = 1;
        var dto = TestDataFixture.GetUpdateAssuntoDtoValido();
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Descricao", "Descrição é obrigatória")
        };
        
        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await _assuntoService.UpdateAsync(id, dto)
        );
        
        _assuntoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Assunto>()), Times.Never);
    }
    #endregion

    #region DeleteAsync Tests
    [Fact]
    public async Task DeleteAsync_ComIdValido_DeveDeletarAssunto()
    {
        // Arrange
        var id = 1;
        var assunto = TestDataFixture.GetAssuntoValido();
        
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(assunto);
        
        _assuntoRepositoryMock.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        // Act
        var result = await _assuntoService.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _assuntoRepositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _assuntoRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var id = 999;
        
        _assuntoRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Assunto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _assuntoService.DeleteAsync(id)
        );
        
        _assuntoRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Never);
    }
    #endregion
}
