using BookManager.Infrastructure.DTOs;

namespace BookManager.Application.Services.Interfaces
{
    public interface ILivroService
    {
        Task<LivroDto?> GetByIdAsync(int id);
        Task<IEnumerable<LivroDto>> GetAllAsync();
        Task<IEnumerable<LivroDto>> GetByAssuntoAsync(int idAssunto);
        Task<IEnumerable<LivroDto>> GetByAutorAsync(int idAutor);
        Task<IEnumerable<LivroDto>> GetAtivoAsync();
        Task<int> CreateAsync(CreateLivroDto dto);
        Task<bool> UpdateAsync(int id, UpdateLivroDto dto);
        Task<bool> DeleteAsync(int id);        
    }
}
