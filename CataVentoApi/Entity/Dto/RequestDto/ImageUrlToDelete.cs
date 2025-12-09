using System.ComponentModel.DataAnnotations;

namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class ImageUrlToDelete
    {
        [Required] public string ImageUrl { get; set; }
    }
}
