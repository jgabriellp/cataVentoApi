using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Dto.ResponseDto;

namespace CataVentoApi.Repositories.Interface
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAll(int pageNumber, int pageSize);
        Task<IEnumerable<Usuario>> GetAllUsersByGroupIdAsync(long groupId);
        Task<Usuario> GetById(long id);
        Task<Usuario> GetByEmail(string email);
        Task<IEnumerable<Usuario>> GetByName(string name);
        Task<Usuario> CreateAsync(Usuario usuario);
        Task<bool> UpdateAsync(Usuario usuario);
        Task<bool> DeleteAsync(long id);
    }
}
