using Microsoft.EntityFrameworkCore;
using ForecastFlow.Core.Models;
using ForecastFlow.Core.Interfaces;

namespace ForecastFlow.Api.Data.Repository
{
    public class AppTaskRepository : IAppTaskRepository
    {
        private readonly ApplicationDbContext _context;

        public AppTaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppTask>> GetAllAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<AppTask?> GetByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<IEnumerable<AppTask>> GetByUserIdAsync(int userId)
        {
            return await _context.Tasks.Where(t => t.UserId == userId).ToListAsync();
        }

        public async Task AddAsync(AppTask task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AppTask task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}