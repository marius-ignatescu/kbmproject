using System.Linq.Dynamic.Core;
using KBMGrpcService.Models;

namespace KBMGrpcService.Data.QueryBuilders
{
    public static class UserQueryBuilder
    {
        private static readonly HashSet<string> AllowedOrderColumns = new() { "Username", "Name", "Email", "CreatedAt" };

        public static IQueryable<User> ApplyOrdering(IQueryable<User> query, string? orderBy, string? direction, ILogger logger)
        {
            var column = string.IsNullOrWhiteSpace(orderBy) ? "CreatedAt" : orderBy;
            var orderDir = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";

            if (!AllowedOrderColumns.Contains(column))
            {
                logger.LogWarning("Invalid OrderBy column: {Column}. Defaulting to 'CreatedAt'.", column);
                column = "CreatedAt";
            }

            try
            {
                return query.OrderBy($"{column} {orderDir}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply ordering. Defaulting to 'CreatedAt'.");
                return query.OrderBy("CreatedAt");
            }
        }

        public static IEnumerable<User> ApplyFiltering(IEnumerable<User> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var q = search.ToLower();
            return query.Where(u =>
                u.Name.ToLower().Contains(q) ||
                u.Username.ToLower().Contains(q) ||
                u.Email.ToLower().Contains(q));
        }

        public static IEnumerable<User> ApplyPaging(IEnumerable<User> query, int page, int pageSize)
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
