using BookManager.Domain.Entities;

namespace BookManager.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Interface para repositório de Livro
/// </summary>
public interface ILivroRepository : IRepository<Livro>
{
    Task<Livro?> GetByTituloAsync(string titulo);
    Task<Livro?> GetByISBNAsync(string isbn);
    Task<Livro?> GetWithDetailsAsync(int id);
    Task<IEnumerable<Livro>> GetByAssuntoAsync(int idAssunto);
    Task<IEnumerable<Livro>> GetByAutorAsync(int idAutor);
    Task<IEnumerable<Livro>> GetByAtivo(bool ativo);    
}
