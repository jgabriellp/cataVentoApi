using System.ComponentModel.DataAnnotations;

namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class GroupUpdateRequestDto
    {
        [Required] public string GroupName { get; set; }
    }
}
