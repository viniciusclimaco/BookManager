using BookManager.Application.Validators;
using BookManager.Infrastructure.DTOs;
using BookManager.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace BookManager.Tests.Validators;

/// <summary>
/// Testes para validadores de Autor
/// </summary>
public class AutorValidatorTests
{
    private readonly CreateAutorDtoValidator _createValidator;
    private readonly UpdateAutorDtoValidator _updateValidator;

    public AutorValidatorTests()
    {
        _createValidator = new CreateAutorDtoValidator();
        _updateValidator = new UpdateAutorDtoValidator();
    }

    #region CreateAutorDto Tests
    [Fact]
    public async Task CreateAutorDto_ComDadosValidos_DevePassar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAutorDtoValido();

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAutorDto_ComNomeVazio_DeveFalhar()
    {
        // Arrange
        var dto = new CreateAutorDto { Nome = string.Empty };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Nome");
    }

    [Fact]
    public async Task CreateAutorDto_ComNomeNull_DeveFalhar()
    {
        // Arrange
        var dto = new CreateAutorDto { Nome = null! };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Nome");
    }

    [Fact]
    public async Task CreateAutorDto_ComNomeMuitoLongo_DeveFalhar()
    {
        // Arrange
        var dto = new CreateAutorDto { Nome = new string('A', 256) };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Nome");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("João Silva")]
    [InlineData("Maria da Silva Santos")]
    public async Task CreateAutorDto_ComNomesValidos_DevePassar(string nome)
    {
        // Arrange
        var dto = new CreateAutorDto { Nome = nome };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
    #endregion

    #region UpdateAutorDto Tests
    [Fact]
    public async Task UpdateAutorDto_ComDadosValidos_DevePassar()
    {
        // Arrange
        var dto = TestDataFixture.GetUpdateAutorDtoValido();

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAutorDto_ComNomeVazio_DeveFalhar()
    {
        // Arrange
        var dto = new UpdateAutorDto { Nome = string.Empty, Ativo = true };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Nome");
    }

    [Fact]
    public async Task UpdateAutorDto_ComNomeMuitoLongo_DeveFalhar()
    {
        // Arrange
        var dto = new UpdateAutorDto { Nome = new string('A', 256), Ativo = true };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Nome");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAutorDto_ComQualquerStatusAtivo_DevePassar(bool ativo)
    {
        // Arrange
        var dto = new UpdateAutorDto { Nome = "Autor Teste", Ativo = ativo };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
    #endregion
}
