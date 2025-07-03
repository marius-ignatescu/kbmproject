namespace KBMGrpcService.Models
{
    public class Organization : BaseModel
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
