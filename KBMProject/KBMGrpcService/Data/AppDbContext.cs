using System.Text.Json;
using KBMGrpcService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KBMGrpcService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            if (auditEntries.Any())
            {
                await AuditLogs.AddRangeAsync(auditEntries);
                await base.SaveChangesAsync(cancellationToken); // Save audit logs
            }

            return result;
        }

        private List<AuditLog> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditLog>();
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries().Where(e =>
                         e.State == EntityState.Added ||
                         e.State == EntityState.Modified ||
                         e.State == EntityState.Deleted))
            {
                if (entry.Entity is AuditLog) continue; // Skip self

                var audit = new AuditLog
                {
                    TableName = entry.Metadata.GetTableName() ?? string.Empty,
                    Key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? string.Empty,
                    Action = entry.State.ToString().ToUpperInvariant(),
                    Timestamp = utcNow,
                    Changes = new Dictionary<string, object>()
                };

                foreach (var prop in entry.Properties)
                {
                    string propName = prop.Metadata.Name;
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            audit.Changes[propName] = prop.CurrentValue ?? string.Empty;
                            break;
                        case EntityState.Deleted:
                            audit.Changes[propName] = prop.OriginalValue ?? string.Empty;
                            break;
                        case EntityState.Modified:
                            if (!Equals(prop.OriginalValue, prop.CurrentValue))
                            {
                                audit.Changes[propName] = new
                                {
                                    Old = prop.OriginalValue,
                                    New = prop.CurrentValue
                                };
                            }
                            break;
                    }
                }

                audit.Changes = audit.Changes.Count != 0 ? audit.Changes : null;
                auditEntries.Add(audit);
            }

            return auditEntries;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var dictionaryConverter = new ValueConverter<Dictionary<string, object>?, string>(
                v => JsonSerializer.Serialize(v ?? new Dictionary<string, object>(), (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v ?? "{}", (JsonSerializerOptions?)null)
            );

            var dictionaryComparer = new ValueComparer<Dictionary<string, object>?>(
                (d1, d2) => JsonSerializer.Serialize(d1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(d2, (JsonSerializerOptions?)null),
                d => d == null ? 0 : JsonSerializer.Serialize(d, (JsonSerializerOptions?)null).GetHashCode(),
                d => d == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(d, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)
            );

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Changes)
                .HasConversion(dictionaryConverter)
                .Metadata.SetValueComparer(dictionaryComparer);
        }
    }
}
