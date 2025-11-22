using System.ComponentModel.DataAnnotations;

namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class PostLikerRequestDto
    {
        [Required] public long PostId { get; set; }

        [Required] public long UsuarioId { get; set; }
    }
}
