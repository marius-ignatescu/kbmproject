namespace KBMContracts.Dtos
{
    public class UpdateOrganizationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
    }
}
