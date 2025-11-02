using BookManager.Domain.Entities;

namespace BookManager.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Interface para repositório de Preço do Livro
/// </summary>
public interface ILivroPrecoRepository : IRepository<LivroPreco>
{
    Task<LivroPreco?> GetByLivroAndFormaPagamentoAsync(int idLivro, int idFormaPagamento);
    Task<IEnumerable<LivroPreco>> GetByLivroAsync(int idLivro);
    Task<IEnumerable<LivroPreco>> GetByFormaPagamentoAsync(int idFormaPagamento);
    Task<bool> DeleteByLivroAsync(int idLivro);
}
