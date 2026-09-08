using System;
using System.Collections.Generic;
using HamaraCommerce.Common;

namespace HamaraCommerce.Models
{
    public class AdminPaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public string Action { get; set; } = "";
        public string? Controller { get; set; }
        public IDictionary<string, string?> RouteParams { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        public PaginationWindow Window => AdminPaginationHelper.CalculateWindow(CurrentPage, TotalPages);
    }
}
