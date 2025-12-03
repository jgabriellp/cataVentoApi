using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Repositories.Interface;
using CataVentoApi.Services.Interface;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CataVentoApi.Services.Service
{
    public class NoticeService : INoticeService
    {
        private readonly INoticeRepository _noticeRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public NoticeService(INoticeRepository noticeRepository, IUsuarioRepository usuarioRepository)
        {
            _noticeRepository = noticeRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IEnumerable<Notice>> GetAllNoticesActiveAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;

            var notices = await _noticeRepository.GetAllActiveAsync(pageNumber, pageSize);

            return notices;
        }

        public async Task<IEnumerable<Notice>> GetAllNoticesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;

            var notices = await _noticeRepository.GetAllAsync(pageNumber, pageSize);

            return notices;
        }

        public async Task<IEnumerable<Notice>> GetByNoticeCreatorIdAsync(long creatorId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;

            var user = await _usuarioRepository.GetById(creatorId);

            if (user == null)
            {
                return null;
            }

            var notices = await _noticeRepository.GetByCreatorIdAsync(creatorId, pageNumber, pageSize);
            return notices;
        }

        public Task<IEnumerable<Notice>> GetFilteredAsync(string? title, bool? isActive)
        {
            throw new NotImplementedException();
        }

        public async Task<Notice> GetNoticeByIdAsync(long noticeId)
        {
            var notice = await _noticeRepository.GetByIdAsync(noticeId);

            if (notice == null)
            {
                return null;
            }

            return notice;
        }

        public async Task<long> AddNoticeAsync(NoticeResquestDto noticeResquestDto)
        {
            var creator = await _usuarioRepository.GetById(noticeResquestDto.CreatorId);

            if (creator == null)
            {
                return 0;
            }

            var notice = new Notice
            {
                Title = noticeResquestDto.Title,
                Content = noticeResquestDto.Content,
                DateCreated = noticeResquestDto.DateCreated,
                IsActive = noticeResquestDto.IsActive,
                PhotoUrl = noticeResquestDto.PhotoUrl,
                CreatorId = noticeResquestDto.CreatorId,
                Audiences = noticeResquestDto.Audiences.Select(role => new NoticeAudience
                    {
                        AudienceRole = role.AudienceRole
                    }).ToList()
            };

            var newNoticeId = await _noticeRepository.AddAsync(notice);

            return newNoticeId;
        }

        public async Task<bool> UpdateNoticeAsync(Notice notice)
        {
            var creator = await _usuarioRepository.GetById(notice.CreatorId);

            var existingNotice = await _noticeRepository.GetByIdAsync(notice.NoticeId);
            
            if (creator == null || existingNotice == null)
            {
                return false;
            }

            var result = await _noticeRepository.UpdateAsync(notice);
            return result;
        }

        public async Task<bool> DeleteNoticeAsync(long noticeId)
        {
            var existingNotice = await _noticeRepository.GetByIdAsync(noticeId);

            if (existingNotice == null)
            {
                return false;
            }

            var result = await _noticeRepository.DeleteAsync(noticeId);
            return result;
        }
    }
}
