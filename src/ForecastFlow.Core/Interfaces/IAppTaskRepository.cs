using ForecastFlow.Core.Models;

namespace ForecastFlow.Core.Interfaces;

public interface IAppTaskRepository
{
    Task<IEnumerable<AppTask>> GetAllAsync();
    Task<AppTask?> GetByIdAsync(int id);
    Task<IEnumerable<AppTask>> GetByUserIdAsync(int userId);
    Task AddAsync(AppTask task);
    Task UpdateAsync(AppTask task);
    Task DeleteAsync(int id);
}