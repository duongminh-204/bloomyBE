using Bloomy.Data.Interfaces;
using Bloomy.DTOs.Auth;
using Bloomy.Models;
using Bloomy.Models.Enums;
using BloomyBE.Services.Interfaces;

namespace Bloomy.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;

        public AuthService(IAuthRepository authRepo)
        {
            _authRepo = authRepo;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _authRepo.IsEmailExistAsync(dto.Email))
                throw new InvalidOperationException("Email đã tồn tại.");

            if (await _authRepo.IsPhoneExistAsync(dto.PhoneNumber))
                throw new InvalidOperationException("Số điện thoại đã tồn tại.");

            var user = new User
            {
              
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _authRepo.CreateAsync(user, dto.Password);

            return CreateAuthResponse(user, string.Empty);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _authRepo.GetByEmailAsync(dto.EmailOrPhone);

            if (user == null)
                user = await _authRepo.GetByPhoneAsync(dto.EmailOrPhone);

            if (user == null || !await _authRepo.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Email/Số điện thoại hoặc mật khẩu không chính xác.");

            return CreateAuthResponse(user, string.Empty);
        }
        private AuthResponseDto CreateAuthResponse(User user, string token)
        {
            return new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Role = user.Role,
                Token = token
            };
        }
    }
}