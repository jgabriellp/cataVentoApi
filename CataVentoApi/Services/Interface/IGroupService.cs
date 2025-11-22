using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;

namespace CataVentoApi.Services.Interface
{
    public interface IGroupService
    {
        Task<IEnumerable<Group>> GetAllGroupsAsync(int pageNumber, int pageSize);
        Task<Group> GetGroupByIdAsync(long id);
        Task<IEnumerable<Group>> GetGroupsByUserIdAsync(long userId);
        Task<Group> GetGroupByNameAsync(string groupName);
        Task<Group> AddUsersToGroupAsync(long groupId, List<long> userIds);
        Task<Group> RemoveUsersFromGroupAsync(long groupId, List<long> userIds);
        Task<Group> CreateGroupAsync(GroupRequestDto groupRequestDto);
        Task<bool> UpdateGroupAsync(long id, GroupUpdateRequestDto groupUpdateRequestDto);
        Task<bool> DeleteGroupAsync(long id);
    }
}
