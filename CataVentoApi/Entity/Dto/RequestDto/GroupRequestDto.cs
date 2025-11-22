using System.ComponentModel.DataAnnotations;

namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class GroupRequestDto
    {
        [Required] public string GroupName { get; set; }
        [Required] public List<long> UsuariosIds { get; set; } = new List<long>();
    }
}
