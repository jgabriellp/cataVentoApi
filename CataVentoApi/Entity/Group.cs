namespace CataVentoApi.Entity
{
    public class Group
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public List<long> UsuariosIds { get; set; } = new List<long>();
    }
}
