using BookManager.Application.Validators;
using BookManager.Infrastructure.DTOs;
using BookManager.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace BookManager.Tests.Validators;

/// <summary>
/// Testes para validadores de Assunto
/// </summary>
public class AssuntoValidatorTests
{
    private readonly CreateAssuntoDtoValidator _createValidator;
    private readonly UpdateAssuntoDtoValidator _updateValidator;

    public AssuntoValidatorTests()
    {
        _createValidator = new CreateAssuntoDtoValidator();
        _updateValidator = new UpdateAssuntoDtoValidator();
    }

    #region CreateAssuntoDto Tests
    [Fact]
    public async Task CreateAssuntoDto_ComDadosValidos_DevePassar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAssuntoDtoValido();

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAssuntoDto_ComDescricaoVazia_DeveFalhar()
    {
        // Arrange
        var dto = new CreateAssuntoDto { Descricao = string.Empty };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Descricao");
    }

    [Fact]
    public async Task CreateAssuntoDto_ComDescricaoNull_DeveFalhar()
    {
        // Arrange
        var dto = new CreateAssuntoDto { Descricao = null! };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Descricao");
    }

    [Fact]
    public async Task CreateAssuntoDto_ComDescricaoMuitoLonga_DeveFalhar()
    {
        // Arrange
        var dto = new CreateAssuntoDto { Descricao = new string('A', 256) };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Descricao");
    }

    [Theory]
    [InlineData("Romance")]
    [InlineData("Ficção Científica")]
    [InlineData("História do Brasil")]
    public async Task CreateAssuntoDto_ComDescricoesValidas_DevePassar(string descricao)
    {
        // Arrange
        var dto = new CreateAssuntoDto { Descricao = descricao };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
    #endregion

    #region UpdateAssuntoDto Tests
    [Fact]
    public async Task UpdateAssuntoDto_ComDadosValidos_DevePassar()
    {
        // Arrange
        var dto = TestDataFixture.GetUpdateAssuntoDtoValido();

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAssuntoDto_ComDescricaoVazia_DeveFalhar()
    {
        // Arrange
        var dto = new UpdateAssuntoDto { Descricao = string.Empty, Ativo = true };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Descricao");
    }

    [Fact]
    public async Task UpdateAssuntoDto_ComDescricaoMuitoLonga_DeveFalhar()
    {
        // Arrange
        var dto = new UpdateAssuntoDto { Descricao = new string('A', 256), Ativo = true };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Descricao");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAssuntoDto_ComQualquerStatusAtivo_DevePassar(bool ativo)
    {
        // Arrange
        var dto = new UpdateAssuntoDto { Descricao = "Assunto Teste", Ativo = ativo };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
    #endregion
}
