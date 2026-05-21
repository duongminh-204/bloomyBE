using Bloomy.Data.Interfaces;
using Bloomy.Models;
using Bloomy.Models.Enums;
using BloomyBE.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bloomy.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository authRepo, IConfiguration config)
        {
            _authRepo = authRepo;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _authRepo.IsEmailExistAsync(dto.Email))
                throw new Exception("Email đã tồn tại.");

            if (await _authRepo.IsPhoneExistAsync(dto.PhoneNumber))
                throw new Exception("Số điện thoại đã tồn tại.");

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

            var token = GenerateJwtToken(user);

            return CreateAuthResponse(user, token);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _authRepo.GetByEmailAsync(dto.EmailOrPhone);

            if (user == null)
                user = await _authRepo.GetByPhoneAsync(dto.EmailOrPhone);

            if (user == null || !await _authRepo.CheckPasswordAsync(user, dto.Password))
                throw new Exception("Email/Số điện thoại hoặc mật khẩu không chính xác.");

            var token = GenerateJwtToken(user);
            return CreateAuthResponse(user, token);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FullName", user.FullName ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
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

        public Task<AuthResponseDto> ExternalLoginAsync(string provider, string providerKey, string email, string fullName)
        {
            throw new NotImplementedException();
        }
    }
}