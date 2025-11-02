using BookManager.Domain.Entities;

namespace BookManager.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Interface para repositório de Forma de Pagamento
/// </summary>
public interface IFormaPagamentoRepository : IRepository<FormaPagamento>
{
    Task<FormaPagamento?> GetByNomeAsync(string nome);
    Task<IEnumerable<FormaPagamento>> GetByAtivo(bool ativo);
}
