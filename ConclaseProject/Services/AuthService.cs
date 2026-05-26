using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ConclaseProject.DTOs;
using ConclaseProject.Models;
using ConclaseProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ConclaseProject.Services
{
    public class AuthService
    {
        private readonly EventDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(EventDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Check if user already exists
            var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (existingUser)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Email address is already registered." };
            }

            // Securely hash the password using BCrypt
            string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

            var newUser = new User
            {
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = hashedPassword,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Role = "Guest" // Default registration role
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "User registered successfully! You can now log in."
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." };
            }

            // Verify the entered password against the secure stored hash
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." };
            }

            // Generate JWT security token
            string token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login successful.",
                Token = token,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Get secret key from appsettings config or fallback to a secure backup key during development
            var secretKey = _configuration["JwtSettings:Secret"] ?? "GTHR_SUPER_SECRET_SECURITY_KEY_LONG_STRING_2026";
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role) // Injects role for endpoint authorization policies
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token valid for 7 days
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}