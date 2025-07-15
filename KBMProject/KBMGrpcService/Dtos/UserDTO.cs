using KBMGrpcService.Models;

namespace KBMGrpcService.Dtos
{
    public class UserDTO : BaseDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }
}
