using Microsoft.EntityFrameworkCore;
using ForecastFlow.Core.Models;

namespace ForecastFlow.Api.Data.Repository
{
    public class AppUserRepository
    {
        private readonly ApplicationDbContext _context;

        public AppUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddAsync(AppUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AppUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}