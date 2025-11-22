using System.ComponentModel.DataAnnotations;

namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class PostRequestDto
    {
        [Required(ErrorMessage = "O conteúdo é obrigatório.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "O ID do grupo é obrigatório.")]
        public long GroupId { get; set; }

        [Required(ErrorMessage = "O ID do criador é obrigatório.")]
        public long CreatorId { get; set; }

        public string ImageUrl { get; set; }
    }
}
