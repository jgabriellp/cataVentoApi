using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;

namespace CataVentoApi.Services.Interface
{
    public interface INoticeService
    {
        Task<Notice> GetNoticeByIdAsync(long noticeId);
        Task<long> AddNoticeAsync(NoticeResquestDto noticeResquestDto);
        Task<bool> UpdateNoticeAsync(Notice notice);
        Task<bool> DeleteNoticeAsync(long noticeId);
        Task<IEnumerable<Notice>> GetAllNoticesAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Notice>> GetAllNoticesActiveAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Notice>> GetByNoticeCreatorIdAsync(long creatorId, int pageNumber, int pageSize);
        Task<IEnumerable<Notice>> GetFilteredAsync(string? title, bool? isActive);
    }
}
