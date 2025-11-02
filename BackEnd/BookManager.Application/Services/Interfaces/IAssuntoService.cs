using BookManager.Infrastructure.DTOs;

namespace BookManager.Application.Services.Interfaces
{
    public interface IAssuntoService
    {
        Task<AssuntoDto?> GetByIdAsync(int id);
        Task<IEnumerable<AssuntoDto>> GetAllAsync();
        Task<IEnumerable<AssuntoDto>> GetAtivoAsync();
        Task<int> CreateAsync(CreateAssuntoDto dto);
        Task<bool> UpdateAsync(int id, UpdateAssuntoDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
