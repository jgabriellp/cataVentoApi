using System.ComponentModel.DataAnnotations;

namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class ImageDeleteRequestDto
    {
        [Required]
        public string PublicId { get; set; }
    }
}
