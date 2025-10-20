using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskManagement.API.Repositories;
namespace TaskManagement.API.Services
{
    // Auth Service
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            var response = new ApiResponse<LoginResponseDto>();

            // Validate role
            if (registerDto.Role != "Employee" && registerDto.Role != "Manager")
            {
                response.Success = false;
                response.Message = "Invalid role. Must be 'Employee' or 'Manager'.";
                return response;
            }

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                response.Success = false;
                response.Message = "Email already exists.";
                return response;
            }

            // Create user
            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                Password = HashPassword(registerDto.Password),
                Role = registerDto.Role
            };

            await _userRepository.CreateAsync(user);

            // Generate token
            var token = GenerateJwtToken(user);

            response.Success = true;
            response.Message = "User registered successfully.";
            response.Data = new LoginResponseDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };

            return response;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var response = new ApiResponse<LoginResponseDto>();

            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null || !VerifyPassword(loginDto.Password, user.Password))
            {
                response.Success = false;
                response.Message = "Invalid email or password.";
                return response;
            }

            var token = GenerateJwtToken(user);

            response.Success = true;
            response.Message = "Login successful.";
            response.Data = new LoginResponseDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };

            return response;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                // Expires = DateTime.UtcNow.AddHours(double.Parse(jwtSettings["ExpiryInHours"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    // User Service
    public interface IUserService
    {
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync();
        Task<ApiResponse<UserDto>> GetUserByIdAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync()
        {
            var response = new ApiResponse<List<UserDto>>();
            var users = await _userRepository.GetAllAsync();

            response.Success = true;
            response.Data = users.Select(u => new UserDto
            {
                UserID = u.UserID,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            }).ToList();

            return response;
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int id)
        {
            var response = new ApiResponse<UserDto>();
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            response.Success = true;
            response.Data = new UserDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return response;
        }
    }
}