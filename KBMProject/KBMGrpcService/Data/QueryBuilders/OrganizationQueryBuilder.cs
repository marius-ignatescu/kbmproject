using System.Linq.Dynamic.Core;
using KBMGrpcService.Models;

namespace KBMGrpcService.Data.QueryBuilders
{
    public static class OrganizationQueryBuilder
    {
        private static readonly HashSet<string> AllowedOrderColumns = new() { "Name", "Address", "CreatedAt", "UpdatedAt" };

        public static IQueryable<Organization> ApplyOrdering(IQueryable<Organization> query, string? orderBy, string? direction)
        {
            var column = string.IsNullOrWhiteSpace(orderBy) ? "CreatedAt" : orderBy;
            var orderDir = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";

            if (!AllowedOrderColumns.Contains(column))
            {
                column = "CreatedAt";
            }

            return query.OrderBy($"{column} {orderDir}");
        }

        public static IQueryable<Organization> ApplyFiltering(IQueryable<Organization> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var q = search.ToLower();
            return query.Where(o =>
                o.Name.ToLower().Contains(q) ||
                o.Address != null && o.Address.ToLower().Contains(q));
        }

        public static IQueryable<Organization> ApplyPaging(IQueryable<Organization> query, int page, int pageSize)
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
