using System.Data;

namespace CataVentoApi.Repositories.Interface
{
    public interface INoticeAudienceRepository
    {
        //Task<bool> AddAudiencesAsync(long noticeId, IEnumerable<short> audienceRoles);
        Task<bool> AddAudiencesAsync(
            long noticeId,
            IEnumerable<short> audienceRoles,
            IDbConnection connection,
            IDbTransaction transaction);
        Task<bool> RemoveAllAudiencesAsync(long noticeId);
        Task<IEnumerable<short>> GetRolesByNoticeIdAsync(long noticeId);
        Task<bool> HasRoleAsync(long noticeId, short audienceRole);
    }
}
