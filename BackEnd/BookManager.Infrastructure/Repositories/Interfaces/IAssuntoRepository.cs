using BookManager.Domain.Entities;

namespace BookManager.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Interface para repositório de Assunto
/// </summary>
public interface IAssuntoRepository : IRepository<Assunto>
{
    Task<Assunto?> GetByDescricaoAsync(string descricao);
    Task<IEnumerable<Assunto>> GetByAtivo(bool ativo);
}
