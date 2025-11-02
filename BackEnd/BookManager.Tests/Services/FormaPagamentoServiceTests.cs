using BookManager.Application.Services;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.Repositories.Interfaces;
using BookManager.Tests.Fixtures;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookManager.Tests.Services;

/// <summary>
/// Testes para FormaPagamentoService
/// </summary>
public class FormaPagamentoServiceTests
{
    private readonly Mock<IFormaPagamentoRepository> _formaPagamentoRepositoryMock;
    private readonly FormaPagamentoService _formaPagamentoService;

    public FormaPagamentoServiceTests()
    {
        _formaPagamentoRepositoryMock = new Mock<IFormaPagamentoRepository>();
        _formaPagamentoService = new FormaPagamentoService(_formaPagamentoRepositoryMock.Object);
    }

    #region Constructor Tests
    [Fact]
    public void Constructor_ComRepositoryNull_DeveLancarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FormaPagamentoService(null!));
    }
    #endregion

    #region GetByIdAsync Tests
    [Fact]
    public async Task GetByIdAsync_ComIdValido_DeveRetornarFormaPagamento()
    {
        // Arrange
        var formaPagamento = TestDataFixture.GetFormaPagamentoValida();
        _formaPagamentoRepositoryMock.Setup(r => r.GetByIdAsync(formaPagamento.IdFormaPagamento))
            .ReturnsAsync(formaPagamento);

        // Act
        var result = await _formaPagamentoService.GetByIdAsync(formaPagamento.IdFormaPagamento);

        // Assert
        result.Should().NotBeNull();
        result!.IdFormaPagamento.Should().Be(formaPagamento.IdFormaPagamento);
        result.Nome.Should().Be(formaPagamento.Nome);
        result.Descricao.Should().Be(formaPagamento.Descricao);
        result.Ativo.Should().Be(formaPagamento.Ativo);
        _formaPagamentoRepositoryMock.Verify(r => r.GetByIdAsync(formaPagamento.IdFormaPagamento), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var idInexistente = 999;
        _formaPagamentoRepositoryMock.Setup(r => r.GetByIdAsync(idInexistente))
            .ReturnsAsync((FormaPagamento?)null);

        // Act
        var result = await _formaPagamentoService.GetByIdAsync(idInexistente);

        // Assert
        result.Should().BeNull();
        _formaPagamentoRepositoryMock.Verify(r => r.GetByIdAsync(idInexistente), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public async Task GetByIdAsync_ComDiferentesIds_DeveChamarRepositoryComIdCorreto(int id)
    {
        // Arrange
        _formaPagamentoRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((FormaPagamento?)null);

        // Act
        await _formaPagamentoService.GetByIdAsync(id);

        // Assert
        _formaPagamentoRepositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
    }
    #endregion

    #region GetAllAsync Tests
    [Fact]
    public async Task GetAllAsync_DeveRetornarListaDeFormasPagamento()
    {
        // Arrange
        var formasPagamento = TestDataFixture.GetListaFormasPagamento();
        _formaPagamentoRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(formasPagamento);

        // Act
        var result = await _formaPagamentoService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(formasPagamento.Count);
        
        var resultList = result.ToList();
        for (int i = 0; i < formasPagamento.Count; i++)
        {
            resultList[i].IdFormaPagamento.Should().Be(formasPagamento[i].IdFormaPagamento);
            resultList[i].Nome.Should().Be(formasPagamento[i].Nome);
            resultList[i].Descricao.Should().Be(formasPagamento[i].Descricao);
            resultList[i].Ativo.Should().Be(formasPagamento[i].Ativo);
        }
        
        _formaPagamentoRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_SemFormasPagamento_DeveRetornarListaVazia()
    {
        // Arrange
        _formaPagamentoRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<FormaPagamento>());

        // Act
        var result = await _formaPagamentoService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _formaPagamentoRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
    #endregion

    #region GetAtivoAsync Tests
    [Fact]
    public async Task GetAtivoAsync_DeveRetornarApenasFormasPagamentoAtivas()
    {
        // Arrange
        var formasPagamento = TestDataFixture.GetListaFormasPagamento();
        var formasPagamentoAtivas = formasPagamento.Where(f => f.Ativo).ToList();
        _formaPagamentoRepositoryMock.Setup(r => r.GetByAtivo(true))
            .ReturnsAsync(formasPagamentoAtivas);

        // Act
        var result = await _formaPagamentoService.GetAtivoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(formasPagamentoAtivas.Count);
        result.Should().OnlyContain(f => f.Ativo);
        _formaPagamentoRepositoryMock.Verify(r => r.GetByAtivo(true), Times.Once);
    }

    [Fact]
    public async Task GetAtivoAsync_SemFormasPagamentoAtivas_DeveRetornarListaVazia()
    {
        // Arrange
        _formaPagamentoRepositoryMock.Setup(r => r.GetByAtivo(true))
            .ReturnsAsync(new List<FormaPagamento>());

        // Act
        var result = await _formaPagamentoService.GetAtivoAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _formaPagamentoRepositoryMock.Verify(r => r.GetByAtivo(true), Times.Once);
    }

    [Fact]
    public async Task GetAtivoAsync_NuncaDeveChamarGetByAtivoComFalse()
    {
        // Arrange
        var formasPagamentoAtivas = TestDataFixture.GetListaFormasPagamento().Where(f => f.Ativo).ToList();
        _formaPagamentoRepositoryMock.Setup(r => r.GetByAtivo(true))
            .ReturnsAsync(formasPagamentoAtivas);

        // Act
        await _formaPagamentoService.GetAtivoAsync();

        // Assert
        _formaPagamentoRepositoryMock.Verify(r => r.GetByAtivo(false), Times.Never);
        _formaPagamentoRepositoryMock.Verify(r => r.GetByAtivo(true), Times.Once);
    }
    #endregion

    #region MapToDto Tests (Testes indiretos através dos métodos públicos)
    [Fact]
    public async Task MapToDto_DeveManterId()
    {
        // Arrange
        var formaPagamento = new FormaPagamento
        {
            IdFormaPagamento = 123,
            Nome = "Teste",
            Descricao = "Descrição Teste",
            Ativo = true            
        };
        
        _formaPagamentoRepositoryMock.Setup(r => r.GetByIdAsync(123))
            .ReturnsAsync(formaPagamento);

        // Act
        var result = await _formaPagamentoService.GetByIdAsync(123);

        // Assert
        result!.IdFormaPagamento.Should().Be(123);
    }

    [Fact]
    public async Task MapToDto_DeveManterTodosOsCampos()
    {
        // Arrange
        var formaPagamento = new FormaPagamento
        {
            IdFormaPagamento = 456,
            Nome = "Cartão de Crédito",
            Descricao = "Pagamento via cartão de crédito",
            Ativo = false            
        };
        
        _formaPagamentoRepositoryMock.Setup(r => r.GetByIdAsync(456))
            .ReturnsAsync(formaPagamento);

        // Act
        var result = await _formaPagamentoService.GetByIdAsync(456);

        // Assert
        result.Should().NotBeNull();
        result!.IdFormaPagamento.Should().Be(formaPagamento.IdFormaPagamento);
        result.Nome.Should().Be(formaPagamento.Nome);
        result.Descricao.Should().Be(formaPagamento.Descricao);
        result.Ativo.Should().Be(formaPagamento.Ativo);
    }
    #endregion
}
