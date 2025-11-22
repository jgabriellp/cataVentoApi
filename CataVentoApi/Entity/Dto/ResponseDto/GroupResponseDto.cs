using System.ComponentModel.DataAnnotations;

namespace CataVentoApi.Entity.Dto.ResponseDto
{
    public class GroupResponseDto
    {
        [Required] public long GroupId { get; set; }
        [Required] public string GroupName { get; set; }
        [Required] public List<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
