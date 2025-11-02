using BookManager.Application.Validators;
using BookManager.Infrastructure.DTOs;
using BookManager.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace BookManager.Tests.Validators;

/// <summary>
/// Testes para validadores de Livro
/// </summary>
public class LivroValidatorTests
{
    private readonly CreateLivroDtoValidator _createValidator;
    private readonly UpdateLivroDtoValidator _updateValidator;
    private readonly CreateLivroPrecoDtoValidator _precoValidator;

    public LivroValidatorTests()
    {
        _createValidator = new CreateLivroDtoValidator();
        _updateValidator = new UpdateLivroDtoValidator();
        _precoValidator = new CreateLivroPrecoDtoValidator();
    }

    #region CreateLivroDto Tests
    [Fact]
    public async Task CreateLivroDto_ComDadosValidos_DevePassar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateLivroDto_ComTituloVazio_DeveFalhar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.Titulo = string.Empty;

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Titulo");
    }

    [Fact]
    public async Task CreateLivroDto_ComTituloMuitoLongo_DeveFalhar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.Titulo = new string('A', 256);

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Titulo");
    }

    [Fact]
    public async Task CreateLivroDto_ComIdAssuntoInvalido_DeveFalhar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.IdAssunto = 0;

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IdAssunto");
    }

    [Fact]
    public async Task CreateLivroDto_SemAutores_DeveFalhar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.IdAutores = new List<int>();

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IdAutores");
    }

    [Fact]
    public async Task CreateLivroDto_ComAutorInvalido_DeveFalhar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.IdAutores = new List<int> { 0, -1 };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IdAutores");
    }

    [Fact]
    public async Task CreateLivroDto_SemPrecos_DeveFalhar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.Precos = new List<CreateLivroPrecoDto>();

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Precos");
    }

    [Theory]
    [InlineData(999)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateLivroDto_ComAnoPublicacaoInvalido_DeveFalhar(int ano)
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.AnoPublicacao = ano;

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AnoPublicacao");
    }

    [Fact]
    public async Task CreateLivroDto_ComAnoPublicacaoFuturo_DeveFalhar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.AnoPublicacao = DateTime.Now.Year + 1;

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AnoPublicacao");
    }

    [Theory]
    [InlineData("9788535908671")]  // ISBN-13 válido (13 dígitos)
    [InlineData("9780134685991")]  // ISBN-13 válido (13 dígitos)
    public async Task CreateLivroDto_ComISBNValido_DevePassar(string isbn)
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.ISBN = isbn;

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("123")]  // Muito curto
    [InlineData("ABC123DEF456")]  // Letras inválidas
    public async Task CreateLivroDto_ComISBNInvalido_DeveFalhar(string isbn)
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroDtoValido();
        dto.ISBN = isbn;

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ISBN");
    }
    #endregion

    #region UpdateLivroDto Tests
    [Fact]
    public async Task UpdateLivroDto_ComDadosValidos_DevePassar()
    {
        // Arrange
        var dto = TestDataFixture.GetUpdateLivroDtoValido();

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateLivroDto_ComTituloVazio_DeveFalhar()
    {
        // Arrange
        var dto = TestDataFixture.GetUpdateLivroDtoValido();
        dto.Titulo = string.Empty;

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Titulo");
    }

    [Fact]
    public async Task UpdateLivroDto_SemAutores_DeveFalhar()
    {
        // Arrange
        var dto = TestDataFixture.GetUpdateLivroDtoValido();
        dto.IdAutores = new List<int>();

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IdAutores");
    }
    #endregion

    #region CreateLivroPrecoDto Tests
    [Fact]
    public async Task CreateLivroPrecoDto_ComDadosValidos_DevePassar()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateLivroPrecoDtoValido();

        // Act
        var result = await _precoValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateLivroPrecoDto_ComIdFormaPagamentoInvalido_DeveFalhar()
    {
        // Arrange
        var dto = new CreateLivroPrecoDto { IdFormaPagamento = 0, Valor = 45.90m };

        // Act
        var result = await _precoValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IdFormaPagamento");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.50)]
    public async Task CreateLivroPrecoDto_ComValorInvalido_DeveFalhar(decimal valor)
    {
        // Arrange
        var dto = new CreateLivroPrecoDto { IdFormaPagamento = 1, Valor = valor };

        // Act
        var result = await _precoValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Valor");
    }

    [Fact]
    public async Task CreateLivroPrecoDto_ComValorMaisDeDuasCasasDecimais_DeveFalhar()
    {
        // Arrange
        var dto = new CreateLivroPrecoDto { IdFormaPagamento = 1, Valor = 45.999m };

        // Act
        var result = await _precoValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Valor");
    }

    [Theory]
    [InlineData(10.50)]
    [InlineData(99.99)]
    [InlineData(100.00)]
    public async Task CreateLivroPrecoDto_ComValoresValidos_DevePassar(decimal valor)
    {
        // Arrange
        var dto = new CreateLivroPrecoDto { IdFormaPagamento = 1, Valor = valor };

        // Act
        var result = await _precoValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
    #endregion
}