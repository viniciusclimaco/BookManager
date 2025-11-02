using BookManager.Domain.Entities;

namespace BookManager.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Interface para repositório de Autor
/// </summary>
public interface IAutorRepository : IRepository<Autor>
{
    Task<Autor?> GetByNomeAsync(string nome);
    Task<IEnumerable<Autor>> GetByAtivo(bool ativo);
}
