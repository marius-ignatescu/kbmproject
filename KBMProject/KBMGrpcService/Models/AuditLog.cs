namespace KBMGrpcService.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "ADDED", "UPDATED"
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object>? Changes { get; set; }
    }
}
