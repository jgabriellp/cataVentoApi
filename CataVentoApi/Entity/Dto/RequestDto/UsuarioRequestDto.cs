using CataVentoApi.Entity.Enums;
using System.ComponentModel.DataAnnotations;

namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class UsuarioRequestDto
    {
        [Required] public string Name { get; set; }
        public string LastName { get; set; }
        [Required] public RoleEnum Role { get; set; }
        [Required] [EmailAddress] public string Email { get; set; }
        public string Password { get; set; }
        public string PhotoUrl { get; set; }
    }
}
