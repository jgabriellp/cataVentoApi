using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Dto.ResponseDto;
using CataVentoApi.Repositories.Interface;
using CataVentoApi.Services.Interface;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CataVentoApi.Services.Service
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        public UsuarioService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        private UsuarioResponseDto MapToResponse(Usuario usuario)
        {
            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Name = usuario.Name,
                LastName = usuario.LastName,
                Role = usuario.Role,
                Email = usuario.Email,
                PhotoUrl = usuario.PhotoUrl,
                GroupIds = usuario.GroupIds
            };
        }

        public AuthResponseDto Login(LoginRequestDto loginRequestDto)
        {
            var user = _usuarioRepository.GetByEmail(loginRequestDto.Email);
            if (user == null || user.Result == null || !BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, user.Result.Password))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, user.Result.Email),
                }),
                Expires = DateTime.UtcNow.AddMinutes(720),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string userToken = tokenHandler.WriteToken(token);

            var userPassword = user.Result.Password;

            var userAccess = new AuthResponseDto
            {
                Id = user.Result.Id,
                Name = user.Result.Name,
                Email = user.Result.Email,
                PhotoUrl = user.Result.PhotoUrl,
                Role = user.Result.Role,
                Password = user.Result.Password,
                Token = userToken
            };

            return userAccess;
        }

        public async Task<IEnumerable<UsuarioResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;

            var users = await _usuarioRepository.GetAll(pageNumber, pageSize);
            return users.Select(MapToResponse);
        }

        public async Task<IEnumerable<UsuarioResponseDto>> GetAllUsersByGroupIdAsync(long groupId)
        {
            var users = await _usuarioRepository.GetAllUsersByGroupIdAsync(groupId);
            return users.Select(MapToResponse);
        }

        public async Task<IEnumerable<UsuarioResponseDto>> GetUserByNameAsync(string name)
        {
            var users = await _usuarioRepository.GetByName(name);
            return users.Select(MapToResponse);
        }

        public async Task<UsuarioResponseDto> GetUserByEmailAsync(string email)
        {
            var user = await _usuarioRepository.GetByEmail(email);
            return user != null ? MapToResponse(user) : null;
        }

        public async Task<UsuarioResponseDto> GetUserByIdAsync(long id)
        {
            var user = await _usuarioRepository.GetById(id);
            return user != null ? MapToResponse(user) : null;
        }

        public async Task<UsuarioResponseDto> CreateUserAsync(UsuarioRequestDto usuarioRequestDto)
        {
            if(await _usuarioRepository.GetByEmail(usuarioRequestDto.Email) != null)
            {
                return null;
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuarioRequestDto.Password);

            var user = new Usuario
            {
                Name = usuarioRequestDto.Name,
                LastName = usuarioRequestDto.LastName,
                Role = usuarioRequestDto.Role,
                Email = usuarioRequestDto.Email,
                Password = hashedPassword,
                PhotoUrl = usuarioRequestDto.PhotoUrl
            };

            var createdUser = await _usuarioRepository.CreateAsync(user);
            return MapToResponse(createdUser);
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            var user = await _usuarioRepository.GetById(id);
            if (user == null)
            {
                return false;
            }

            return await _usuarioRepository.DeleteAsync(id);
        }

        public async Task<bool> SetNewPassword(long userId, string oldPassword, string newPassword)
        {
            var user = await _usuarioRepository.GetById(userId);

            if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            {
                return false;
            }

            string hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            
            user.Password = hashedNewPassword;

            return await _usuarioRepository.UpdateAsync(user);
        }

        public async Task<bool> UpdateUserAsync(long id, UsuarioRequestDto usuarioRequestDto)
        {
            var existingUser = await _usuarioRepository.GetById(id);
            if (existingUser == null)
            {
                return false;
            }

            if(existingUser.Email != usuarioRequestDto.Email)
            {
                var existingUserByEmail = await _usuarioRepository.GetByEmail(usuarioRequestDto.Email);
                if (existingUserByEmail != null)
                {
                    return false;
                }
            }

            string passwordToStore = existingUser.Password;
            if (!string.IsNullOrWhiteSpace(usuarioRequestDto.Password))
            {
                passwordToStore = BCrypt.Net.BCrypt.HashPassword(usuarioRequestDto.Password);
            }

            var user = new Usuario
            {
                Id = id,
                Name = usuarioRequestDto.Name,
                LastName = usuarioRequestDto.LastName,
                Role = usuarioRequestDto.Role,
                Email = usuarioRequestDto.Email,
                Password = passwordToStore,
                PhotoUrl = usuarioRequestDto.PhotoUrl
            };

            return await _usuarioRepository.UpdateAsync(user);
        }
    }
}
