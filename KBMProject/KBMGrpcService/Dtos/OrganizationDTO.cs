using KBMGrpcService.Models;

namespace KBMGrpcService.Dtos
{
    public class OrganizationDTO : BaseDTO
    {
        public int OrganizationId { get; set; }
        public required string Name { get; set; }
        public string? Address { get; set; }
        public ICollection<User>? Users { get; set; }
    }
}
