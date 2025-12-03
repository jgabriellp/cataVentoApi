using System.ComponentModel.DataAnnotations;

namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class UpdatePasswordRequestDto
    {
        [Required] public string oldPassword { get; set; }
        [Required] public string newPassword { get; set; }
    }
}
