using BookManager.Infrastructure.DTOs;

namespace BookManager.Application.Services.Interfaces
{
    public interface IAutorService
    {
        Task<AutorDto?> GetByIdAsync(int id);
        Task<IEnumerable<AutorDto>> GetAllAsync();
        Task<IEnumerable<AutorDto>> GetAtivoAsync();
        Task<int> CreateAsync(CreateAutorDto dto);
        Task<bool> UpdateAsync(int id, UpdateAutorDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
