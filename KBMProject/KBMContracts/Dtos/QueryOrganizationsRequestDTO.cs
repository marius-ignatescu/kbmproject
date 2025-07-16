namespace KBMContracts.Dtos
{
    public class QueryOrganizationsRequestDTO
    {
        public string? QueryString { get; set; }
        public string? OrderBy { get; set; }
        public string? Direction { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
