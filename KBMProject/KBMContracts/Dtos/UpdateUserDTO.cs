namespace KBMContracts.Dtos
{
    public class UpdateUserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int OrganizationId { get; set; }
    }
}
