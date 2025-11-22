using CataVentoApi.Entity.Enums;

namespace CataVentoApi.Entity
{
    public class Usuario
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public RoleEnum Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhotoUrl { get; set; }
        public List<long> GroupIds { get; set; } = new List<long>();
    }
}
