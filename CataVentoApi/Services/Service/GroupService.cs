using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Repositories.Interface;
using CataVentoApi.Services.Interface;

namespace CataVentoApi.Services.Service
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public GroupService(IGroupRepository groupRepository, IUsuarioRepository usuarioRepository)
        {
            _groupRepository = groupRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IEnumerable<Group>> GetAllGroupsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;

            return await _groupRepository.GetAllGroupsAsync(pageNumber, pageSize);
        }

        public async Task<Group> GetGroupByIdAsync(long id)
        {
            var group = await _groupRepository.GetGroupByIdAsync(id);
            if(group == null)
            {
                return null;
            }
            return group;
        }

        public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(long userId)
        {
            return await _groupRepository.GetGroupsByUserIdAsync(userId);
        }

        public async Task<Group> GetGroupByNameAsync(string groupName)
        {
            var group = await _groupRepository.GetGroupByNameAsync(groupName);
            if(group == null)
            {
                return null;
            }
            return group;
        }

        public async Task<Group> AddUsersToGroupAsync(long groupId, List<long> userIds)
        {
            if(await _groupRepository.GetGroupByIdAsync(groupId) == null)
            {
                return null;
            }

            foreach(var userId in userIds)
            {
                if(await _usuarioRepository.GetById(userId) == null)
                {
                    return null;
                }
            }

            return await _groupRepository.AddUsersToGroupAsync(groupId, userIds);
        }

        public async Task<Group> RemoveUsersFromGroupAsync(long groupId, List<long> userIds)
        {
            if (await _groupRepository.GetGroupByIdAsync(groupId) == null)
            {
                return null;
            }

            foreach (var userId in userIds)
            {
                if (await _usuarioRepository.GetById(userId) == null)
                {
                    return null;
                }
            }

            return await _groupRepository.RemoveUsersFromGroupAsync(groupId, userIds);
        }

        public async Task<Group> CreateGroupAsync(GroupRequestDto groupRequestDto)
        {
            var group = await _groupRepository.GetGroupByNameAsync(groupRequestDto.GroupName);
            if (group != null)
            {
                return null;
            }

            foreach (var userId in groupRequestDto.UsuariosIds)
            {
                if (await _usuarioRepository.GetById(userId) == null)
                {
                    return null;
                }
            }


            var newGroup = new Group
            {
                GroupName = groupRequestDto.GroupName,
                UsuariosIds = groupRequestDto.UsuariosIds
            };

            var createdGroup = await _groupRepository.CreateGroupAsync(newGroup);
            await _groupRepository.AddUsersToGroupAsync(createdGroup.GroupId, groupRequestDto.UsuariosIds);

            return createdGroup;
        }

        public async Task<bool> UpdateGroupAsync(int id, GroupUpdateRequestDto groupUpdateRequestDto)
        {
            var group = await _groupRepository.GetGroupByIdAsync(id);
            if (group == null)
            {
                return false;
            }

            var updatedGroup = new Group
            {
                GroupId = id,
                GroupName = groupUpdateRequestDto.GroupName
            };

            return await _groupRepository.UpdateGroupAsync(updatedGroup); // atualiza o nome do grupo
        }

        public async Task<bool> DeleteGroupAsync(long id)
        {
            var group = await _groupRepository.GetGroupByIdAsync(id);
            if (group == null)
            {
                return false;
            }

            return await _groupRepository.DeleteGroupAsync(id);
        }
    }
}
