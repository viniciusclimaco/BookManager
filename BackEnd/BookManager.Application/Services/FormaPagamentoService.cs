using BookManager.Application.Services.Interfaces;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.DTOs;
using BookManager.Infrastructure.Repositories.Interfaces;

namespace BookManager.Application.Services;

/// <summary>
/// Serviço de aplicação para Forma de Pagamento
/// </summary>
public class FormaPagamentoService : IFormaPagamentoService
{
    private readonly IFormaPagamentoRepository _formaPagamentoRepository;

    public FormaPagamentoService(IFormaPagamentoRepository formaPagamentoRepository)
    {
        _formaPagamentoRepository = formaPagamentoRepository ?? throw new ArgumentNullException(nameof(formaPagamentoRepository));
    }

    public async Task<FormaPagamentoDto?> GetByIdAsync(int id)
    {
        var formaPagamento = await _formaPagamentoRepository.GetByIdAsync(id);
        return formaPagamento == null ? null : MapToDto(formaPagamento);
    }

    public async Task<IEnumerable<FormaPagamentoDto>> GetAllAsync()
    {
        var formasPagamento = await _formaPagamentoRepository.GetAllAsync();
        return formasPagamento.Select(MapToDto);
    }

    public async Task<IEnumerable<FormaPagamentoDto>> GetAtivoAsync()
    {
        var formasPagamento = await _formaPagamentoRepository.GetByAtivo(true);
        return formasPagamento.Select(MapToDto);
    }

    private static FormaPagamentoDto MapToDto(FormaPagamento formaPagamento) => new()
    {
        IdFormaPagamento = formaPagamento.IdFormaPagamento,
        Nome = formaPagamento.Nome,
        Descricao = formaPagamento.Descricao,
        Ativo = formaPagamento.Ativo
    };
}
