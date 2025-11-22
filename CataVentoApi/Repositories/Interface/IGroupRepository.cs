using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;

namespace CataVentoApi.Repositories.Interface
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAllGroupsAsync(int pageNumber, int pageSize);
        Task<Group> GetGroupByIdAsync(long id);
        Task<IEnumerable<Group>> GetGroupsByUserIdAsync(long userId);
        Task<Group> GetGroupByNameAsync(string groupName);
        Task<Group> AddUsersToGroupAsync(long groupId, List<long> userIds);
        Task<Group> RemoveUsersFromGroupAsync(long groupId, List<long> userIds);
        Task<Group> CreateGroupAsync(Group group);
        Task<bool> UpdateGroupAsync(Group group);
        Task<bool> DeleteGroupAsync(long id);
    }
}
