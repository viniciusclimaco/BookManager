using BookManager.Domain.Entities;

namespace BookManager.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interface para repositório de Livro-Autor
    /// </summary>
    public interface ILivroAutorRepository : IRepository<LivroAutor>
    {
        Task<IEnumerable<LivroAutor>> GetByLivroAsync(int idLivro);
        Task<IEnumerable<LivroAutor>> GetByAutorAsync(int idAutor);
        Task<bool> DeleteByLivroAsync(int idLivro);
    }
}
