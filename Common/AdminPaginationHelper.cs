using System;
using System.Collections.Generic;

namespace HamaraCommerce.Common
{
    public enum PaginationItemType
    {
        Page,
        Ellipsis
    }

    public class PaginationItem
    {
        public PaginationItemType Type { get; set; }
        public int PageNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsEllipsis => Type == PaginationItemType.Ellipsis;
        public bool IsSecondary { get; set; }
    }

    public class PaginationWindow
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public int PreviousPage => Math.Max(1, CurrentPage - 1);
        public int NextPage => Math.Min(TotalPages, CurrentPage + 1);
        public List<PaginationItem> Items { get; set; } = new();

        public int NumericButtonCount => Items.FindAll(i => !i.IsEllipsis).Count;
    }

    public static class AdminPaginationHelper
    {
        public const int MaxNumericButtons = 6;

        public static PaginationWindow CalculateWindow(int requestedPage, int totalPages)
        {
            if (totalPages <= 0) totalPages = 1;
            int currentPage = Math.Clamp(requestedPage, 1, totalPages);

            var window = new PaginationWindow
            {
                CurrentPage = currentPage,
                TotalPages = totalPages
            };

            if (totalPages <= MaxNumericButtons)
            {
                for (int p = 1; p <= totalPages; p++)
                {
                    window.Items.Add(new PaginationItem
                    {
                        Type = PaginationItemType.Page,
                        PageNumber = p,
                        IsActive = (p == currentPage),
                        IsSecondary = (p != 1 && p != totalPages && p != currentPage)
                    });
                }
                return window;
            }

            // When totalPages > 6: Always produce exactly 6 numeric buttons
            if (currentPage <= 4)
            {
                // Case 1: 1, 2, 3, 4, 5, ..., totalPages
                for (int p = 1; p <= 5; p++)
                {
                    window.Items.Add(new PaginationItem
                    {
                        Type = PaginationItemType.Page,
                        PageNumber = p,
                        IsActive = (p == currentPage),
                        IsSecondary = (p != 1 && p != currentPage)
                    });
                }
                window.Items.Add(new PaginationItem { Type = PaginationItemType.Ellipsis });
                window.Items.Add(new PaginationItem
                {
                    Type = PaginationItemType.Page,
                    PageNumber = totalPages,
                    IsActive = (totalPages == currentPage),
                    IsSecondary = false
                });
            }
            else if (currentPage >= totalPages - 3)
            {
                // Case 2: 1, ..., totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages
                window.Items.Add(new PaginationItem
                {
                    Type = PaginationItemType.Page,
                    PageNumber = 1,
                    IsActive = (1 == currentPage),
                    IsSecondary = false
                });
                window.Items.Add(new PaginationItem { Type = PaginationItemType.Ellipsis });
                for (int p = totalPages - 4; p <= totalPages; p++)
                {
                    window.Items.Add(new PaginationItem
                    {
                        Type = PaginationItemType.Page,
                        PageNumber = p,
                        IsActive = (p == currentPage),
                        IsSecondary = (p != totalPages && p != currentPage)
                    });
                }
            }
            else
            {
                // Case 3 (Middle): 1, ..., C - 1, C, C + 1, C + 2, ..., totalPages
                window.Items.Add(new PaginationItem
                {
                    Type = PaginationItemType.Page,
                    PageNumber = 1,
                    IsActive = false,
                    IsSecondary = false
                });
                window.Items.Add(new PaginationItem { Type = PaginationItemType.Ellipsis });

                for (int p = currentPage - 1; p <= currentPage + 2; p++)
                {
                    window.Items.Add(new PaginationItem
                    {
                        Type = PaginationItemType.Page,
                        PageNumber = p,
                        IsActive = (p == currentPage),
                        IsSecondary = (p != currentPage)
                    });
                }

                window.Items.Add(new PaginationItem { Type = PaginationItemType.Ellipsis });
                window.Items.Add(new PaginationItem
                {
                    Type = PaginationItemType.Page,
                    PageNumber = totalPages,
                    IsActive = false,
                    IsSecondary = false
                });
            }

            return window;
        }
    }
}
