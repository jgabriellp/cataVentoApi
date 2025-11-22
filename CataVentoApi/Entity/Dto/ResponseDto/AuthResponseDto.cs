using CataVentoApi.Entity.Enums;

namespace CataVentoApi.Entity.Dto.ResponseDto
{
    public class AuthResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public RoleEnum Role { get; set; }
        public string Password { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
