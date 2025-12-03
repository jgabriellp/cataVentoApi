namespace CataVentoApi.Repositories.Interface
{
    public interface INoticeAudienceRepository
    {
        Task<bool> AddAudiencesAsync(long noticeId, IEnumerable<short> audienceRoles);
        Task<bool> RemoveAllAudiencesAsync(long noticeId);
        Task<IEnumerable<short>> GetRolesByNoticeIdAsync(long noticeId);
        Task<bool> HasRoleAsync(long noticeId, short audienceRole);
    }
}
