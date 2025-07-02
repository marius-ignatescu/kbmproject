namespace KBMGrpcService.Models
{
    public class User : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }
}
