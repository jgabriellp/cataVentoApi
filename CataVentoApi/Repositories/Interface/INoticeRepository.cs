using CataVentoApi.Entity;

namespace CataVentoApi.Repositories.Interface
{
    public interface INoticeRepository
    {
        Task<Notice> GetByIdAsync(long noticeId);
        Task<long> AddAsync(Notice notice);
        Task<bool> UpdateAsync(Notice notice);
        Task<bool> DeleteAsync(long noticeId);
        Task<IEnumerable<Notice>> GetAllAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Notice>> GetAllActiveAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Notice>> GetByCreatorIdAsync(long creatorId, int pageNumber, int pageSize);
        Task<IEnumerable<Notice>> GetFilteredAsync(string? title, bool? isActive);
    }
}
