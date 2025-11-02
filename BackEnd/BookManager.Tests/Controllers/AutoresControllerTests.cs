using BookManager.Application.Services.Interfaces;
using BookManager.Infrastructure.DTOs;
using BookManager.Tests.Fixtures;
using FluentAssertions;
using FluentValidation;
using LibraryManager.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookManager.Tests.Controllers;

/// <summary>
/// Testes para AutoresController
/// </summary>
public class AutoresControllerTests
{
    private readonly Mock<IAutorService> _autorServiceMock;
    private readonly Mock<ILogger<AutoresController>> _loggerMock;
    private readonly AutoresController _controller;

    public AutoresControllerTests()
    {
        _autorServiceMock = new Mock<IAutorService>();
        _loggerMock = new Mock<ILogger<AutoresController>>();
        _controller = new AutoresController(_autorServiceMock.Object, _loggerMock.Object);
    }

    #region GetTodos Tests
    [Fact]
    public async Task GetTodos_ComSucesso_DeveRetornarOkComLista()
    {
        // Arrange
        var autores = TestDataFixture.GetListaAutores();
        var autoresDto = autores.Select(a => new AutorDto
        {
            IdAutor = a.IdAutor,
            Nome = a.Nome,
            Ativo = a.Ativo
        }).ToList();

        _autorServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(autoresDto);

        // Act
        var result = await _controller.GetTodos();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeAssignableTo<IEnumerable<AutorDto>>().Subject;
        returnValue.Should().HaveCount(autoresDto.Count);
        _autorServiceMock.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTodos_ComErro_DeveRetornar500()
    {
        // Arrange
        _autorServiceMock.Setup(s => s.GetAllAsync())
            .ThrowsAsync(new Exception("Erro de banco de dados"));

        // Act
        var result = await _controller.GetTodos();

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }
    #endregion

    #region GetAtivos Tests
    [Fact]
    public async Task GetAtivos_ComSucesso_DeveRetornarOkComListaDeAtivos()
    {
        // Arrange
        var autores = TestDataFixture.GetListaAutores().Where(a => a.Ativo);
        var autoresDto = autores.Select(a => new AutorDto
        {
            IdAutor = a.IdAutor,
            Nome = a.Nome,
            Ativo = a.Ativo
        }).ToList();

        _autorServiceMock.Setup(s => s.GetAtivoAsync())
            .ReturnsAsync(autoresDto);

        // Act
        var result = await _controller.GetAtivos();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeAssignableTo<IEnumerable<AutorDto>>().Subject;
        returnValue.Should().OnlyContain(a => a.Ativo);
        _autorServiceMock.Verify(s => s.GetAtivoAsync(), Times.Once);
    }
    #endregion

    #region GetPorId Tests
    [Fact]
    public async Task GetPorId_ComIdValido_DeveRetornarOkComAutor()
    {
        // Arrange
        var autor = TestDataFixture.GetAutorValido();
        var autorDto = new AutorDto
        {
            IdAutor = autor.IdAutor,
            Nome = autor.Nome,
            Ativo = autor.Ativo
        };

        _autorServiceMock.Setup(s => s.GetByIdAsync(autor.IdAutor))
            .ReturnsAsync(autorDto);

        // Act
        var result = await _controller.GetPorId(autor.IdAutor);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<AutorDto>().Subject;
        returnValue.IdAutor.Should().Be(autor.IdAutor);
        _autorServiceMock.Verify(s => s.GetByIdAsync(autor.IdAutor), Times.Once);
    }

    [Fact]
    public async Task GetPorId_ComIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var idInexistente = 999;
        _autorServiceMock.Setup(s => s.GetByIdAsync(idInexistente))
            .ReturnsAsync((AutorDto?)null);

        // Act
        var result = await _controller.GetPorId(idInexistente);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        _autorServiceMock.Verify(s => s.GetByIdAsync(idInexistente), Times.Once);
    }
    #endregion

    #region Criar Tests
    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornarCreated()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAutorDtoValido();
        var idEsperado = 1;

        _autorServiceMock.Setup(s => s.CreateAsync(dto))
            .ReturnsAsync(idEsperado);

        // Act
        var result = await _controller.Criar(dto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        _autorServiceMock.Verify(s => s.CreateAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Criar_ComDadosInvalidos_DeveRetornarBadRequest()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAutorDtoValido();
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Nome", "Nome é obrigatório")
        };

        _autorServiceMock.Setup(s => s.CreateAsync(dto))
            .ThrowsAsync(new ValidationException(validationFailures));

        // Act
        var result = await _controller.Criar(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Criar_ComNomeDuplicado_DeveRetornarBadRequest()
    {
        // Arrange
        var dto = TestDataFixture.GetCreateAutorDtoValido();

        _autorServiceMock.Setup(s => s.CreateAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Já existe um autor com este nome"));

        // Act
        var result = await _controller.Criar(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
    #endregion

    #region Atualizar Tests
    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveRetornarOk()
    {
        // Arrange
        var id = 1;
        var dto = TestDataFixture.GetUpdateAutorDtoValido();

        _autorServiceMock.Setup(s => s.UpdateAsync(id, dto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Atualizar(id, dto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        _autorServiceMock.Verify(s => s.UpdateAsync(id, dto), Times.Once);
    }

    [Fact]
    public async Task Atualizar_ComIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var id = 999;
        var dto = TestDataFixture.GetUpdateAutorDtoValido();

        _autorServiceMock.Setup(s => s.UpdateAsync(id, dto))
            .ThrowsAsync(new KeyNotFoundException($"Autor com ID {id} não encontrado"));

        // Act
        var result = await _controller.Atualizar(id, dto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Atualizar_ComDadosInvalidos_DeveRetornarBadRequest()
    {
        // Arrange
        var id = 1;
        var dto = TestDataFixture.GetUpdateAutorDtoValido();
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Nome", "Nome é obrigatório")
        };

        _autorServiceMock.Setup(s => s.UpdateAsync(id, dto))
            .ThrowsAsync(new ValidationException(validationFailures));

        // Act
        var result = await _controller.Atualizar(id, dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
    #endregion

    #region Deletar Tests
    [Fact]
    public async Task Deletar_ComIdValido_DeveRetornarOk()
    {
        // Arrange
        var id = 1;

        _autorServiceMock.Setup(s => s.DeleteAsync(id))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Deletar(id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        _autorServiceMock.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Deletar_ComIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var id = 999;

        _autorServiceMock.Setup(s => s.DeleteAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Autor com ID {id} não encontrado"));

        // Act
        var result = await _controller.Deletar(id);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Deletar_ComErro_DeveRetornar500()
    {
        // Arrange
        var id = 1;

        _autorServiceMock.Setup(s => s.DeleteAsync(id))
            .ThrowsAsync(new Exception("Erro ao deletar"));

        // Act
        var result = await _controller.Deletar(id);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }
    #endregion
}
