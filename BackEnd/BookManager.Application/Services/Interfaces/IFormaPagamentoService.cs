using BookManager.Infrastructure.DTOs;

namespace BookManager.Application.Services.Interfaces
{
    public interface IFormaPagamentoService
    {
        Task<FormaPagamentoDto?> GetByIdAsync(int id);
        Task<IEnumerable<FormaPagamentoDto>> GetAllAsync();
        Task<IEnumerable<FormaPagamentoDto>> GetAtivoAsync();
    }
}
