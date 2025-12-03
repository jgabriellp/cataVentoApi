using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Dto.ResponseDto;

namespace CataVentoApi.Services.Interface
{
    public interface IUsuarioService
    {
        AuthResponseDto Login(LoginRequestDto loginRequestDto);
        Task<IEnumerable<UsuarioResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<IEnumerable<UsuarioResponseDto>> GetAllUsersByGroupIdAsync(long groupId);
        Task<UsuarioResponseDto> GetUserByIdAsync(long id);
        Task<UsuarioResponseDto> GetUserByEmailAsync(string email);
        Task<IEnumerable<UsuarioResponseDto>> GetUserByNameAsync(string name);
        Task<UsuarioResponseDto> CreateUserAsync(UsuarioRequestDto usuarioRequestDto);
        Task<bool> SetNewPassword(long userId, string oldPassword, string newPassword);
        Task<bool> UpdateUserAsync(long id, UsuarioRequestDto usuarioRequestDto);
        Task<bool> DeleteUserAsync(long id);
    }
}
