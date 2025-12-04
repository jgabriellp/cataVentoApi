using CataVentoApi.Entity.Enums;

namespace CataVentoApi.Entity.Dto.ResponseDto
{
    public class UsuarioResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public RoleEnum Role { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
        public List<int> GroupIds { get; set; } = new List<int>();
    }
}
