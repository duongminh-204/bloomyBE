using Bloomy.Data.Interfaces;
using Bloomy.Models;
using Microsoft.EntityFrameworkCore;

namespace Bloomy.Data.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly BloomyDbContext _context;

        public AuthRepository(BloomyDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByPhoneAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> IsEmailExistAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsPhoneExistAsync(string phoneNumber)
        {
            return await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task CreateAsync(User user, string password)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public Task<bool> CheckPasswordAsync(User user, string password)
        {
            try
            {
                var isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                return Task.FromResult(isValid);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Legacy records or manually inserted rows may not use BCrypt hashes.
                return Task.FromResult(false);
            }
        }
    }
}