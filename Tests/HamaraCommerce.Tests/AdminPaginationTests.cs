using System.Collections.Generic;
using System.Linq;
using HamaraCommerce.Common;
using HamaraCommerce.Models;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class AdminPaginationTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(50)]
        [InlineData(100)]
        public void Window_NeverExceedsSixNumericButtons_AcrossAllPageNumbers(int totalPages)
        {
            for (int page = -2; page <= totalPages + 3; page++)
            {
                var window = AdminPaginationHelper.CalculateWindow(page, totalPages);
                int numericCount = window.NumericButtonCount;

                Assert.True(numericCount <= 6,
                    $"Page {page} of {totalPages} produced {numericCount} numeric buttons, exceeding max 6.");

                var activeItem = window.Items.FirstOrDefault(i => i.IsActive);
                Assert.NotNull(activeItem);
                Assert.Equal(window.CurrentPage, activeItem.PageNumber);

                for (int i = 0; i < window.Items.Count - 1; i++)
                {
                    Assert.False(window.Items[i].IsEllipsis && window.Items[i + 1].IsEllipsis,
                        $"Consecutive ellipses detected at index {i} for page {page} of {totalPages}.");
                }
            }
        }

        [Fact]
        public void Window_With30Pages_FirstPage_RendersExactSixButtonsAndTrailingEllipsis()
        {
            var window = AdminPaginationHelper.CalculateWindow(1, 30);

            Assert.Equal(1, window.CurrentPage);
            Assert.Equal(30, window.TotalPages);
            Assert.False(window.HasPrevious);
            Assert.True(window.HasNext);

            var numericPages = window.Items.Where(i => !i.IsEllipsis).Select(i => i.PageNumber).ToList();
            Assert.Equal(6, numericPages.Count);
            Assert.Equal(new List<int> { 1, 2, 3, 4, 5, 30 }, numericPages);

            Assert.Single(window.Items, i => i.IsEllipsis);
            Assert.True(window.Items[window.Items.Count - 2].IsEllipsis);

            var activeItem = Assert.Single(window.Items, i => i.IsActive);
            Assert.Equal(1, activeItem.PageNumber);
        }

        [Fact]
        public void Window_With30Pages_Page4NearStart_KeepsPage4ActiveInFirstWindow()
        {
            var window = AdminPaginationHelper.CalculateWindow(4, 30);

            var numericPages = window.Items.Where(i => !i.IsEllipsis).Select(i => i.PageNumber).ToList();
            Assert.Equal(6, numericPages.Count);
            Assert.Equal(new List<int> { 1, 2, 3, 4, 5, 30 }, numericPages);

            var activeItem = Assert.Single(window.Items, i => i.IsActive);
            Assert.Equal(4, activeItem.PageNumber);
        }

        [Fact]
        public void Window_With30Pages_Page5TransitionsToMiddle_RendersTwoEllipsesAndSixButtons()
        {
            var window = AdminPaginationHelper.CalculateWindow(5, 30);

            var numericPages = window.Items.Where(i => !i.IsEllipsis).Select(i => i.PageNumber).ToList();
            Assert.Equal(6, numericPages.Count);
            Assert.Equal(new List<int> { 1, 4, 5, 6, 7, 30 }, numericPages);

            Assert.Equal(2, window.Items.Count(i => i.IsEllipsis));
            var activeItem = Assert.Single(window.Items, i => i.IsActive);
            Assert.Equal(5, activeItem.PageNumber);
        }

        [Fact]
        public void Window_With30Pages_Page15Middle_RendersCenteredFourWindowAndBothEllipses()
        {
            var window = AdminPaginationHelper.CalculateWindow(15, 30);

            var numericPages = window.Items.Where(i => !i.IsEllipsis).Select(i => i.PageNumber).ToList();
            Assert.Equal(6, numericPages.Count);
            Assert.Equal(new List<int> { 1, 14, 15, 16, 17, 30 }, numericPages);

            Assert.Equal(2, window.Items.Count(i => i.IsEllipsis));
            var activeItem = Assert.Single(window.Items, i => i.IsActive);
            Assert.Equal(15, activeItem.PageNumber);
        }

        [Fact]
        public void Window_With30Pages_Page27TransitionsToEnd_RendersLeadingEllipsisAndEndFiveWindow()
        {
            var window = AdminPaginationHelper.CalculateWindow(27, 30);

            var numericPages = window.Items.Where(i => !i.IsEllipsis).Select(i => i.PageNumber).ToList();
            Assert.Equal(6, numericPages.Count);
            Assert.Equal(new List<int> { 1, 26, 27, 28, 29, 30 }, numericPages);

            Assert.Single(window.Items, i => i.IsEllipsis);
            var activeItem = Assert.Single(window.Items, i => i.IsActive);
            Assert.Equal(27, activeItem.PageNumber);
        }

        [Fact]
        public void Window_With30Pages_LastPage30_KeepsPage30ActiveWithLeadingEllipsis()
        {
            var window = AdminPaginationHelper.CalculateWindow(30, 30);

            Assert.False(window.HasNext);
            Assert.True(window.HasPrevious);
            var numericPages = window.Items.Where(i => !i.IsEllipsis).Select(i => i.PageNumber).ToList();
            Assert.Equal(6, numericPages.Count);
            Assert.Equal(new List<int> { 1, 26, 27, 28, 29, 30 }, numericPages);

            var activeItem = Assert.Single(window.Items, i => i.IsActive);
            Assert.Equal(30, activeItem.PageNumber);
        }

        [Fact]
        public void Window_WithSixOrFewerPages_RendersAllNumbersWithoutEllipsis()
        {
            var win4 = AdminPaginationHelper.CalculateWindow(2, 4);
            Assert.Equal(4, win4.NumericButtonCount);
            Assert.DoesNotContain(win4.Items, i => i.IsEllipsis);
            Assert.Equal(new List<int> { 1, 2, 3, 4 }, win4.Items.Select(i => i.PageNumber).ToList());

            var win6 = AdminPaginationHelper.CalculateWindow(6, 6);
            Assert.Equal(6, win6.NumericButtonCount);
            Assert.DoesNotContain(win6.Items, i => i.IsEllipsis);
            Assert.Equal(new List<int> { 1, 2, 3, 4, 5, 6 }, win6.Items.Select(i => i.PageNumber).ToList());
            Assert.True(win6.Items.Last().IsActive);
        }

        [Fact]
        public void Window_SafelyHandlesZeroOrNegativePagesAndOutOfRange()
        {
            var winZero = AdminPaginationHelper.CalculateWindow(0, 0);
            Assert.Equal(1, winZero.TotalPages);
            Assert.Equal(1, winZero.CurrentPage);
            Assert.Single(winZero.Items);
            Assert.Equal(1, winZero.Items[0].PageNumber);

            var winNeg = AdminPaginationHelper.CalculateWindow(-99, 10);
            Assert.Equal(1, winNeg.CurrentPage);

            var winOver = AdminPaginationHelper.CalculateWindow(999, 10);
            Assert.Equal(10, winOver.CurrentPage);
        }

        [Fact]
        public void Window_SecondaryButtons_CorrectlyClassifiedForResponsiveDisplay()
        {
            var window = AdminPaginationHelper.CalculateWindow(15, 30);

            var p1 = window.Items.First(i => i.PageNumber == 1);
            var p30 = window.Items.First(i => i.PageNumber == 30);
            var p15 = window.Items.First(i => i.PageNumber == 15);

            Assert.False(p1.IsSecondary);
            Assert.False(p30.IsSecondary);
            Assert.False(p15.IsSecondary);

            var p14 = window.Items.First(i => i.PageNumber == 14);
            var p16 = window.Items.First(i => i.PageNumber == 16);
            var p17 = window.Items.First(i => i.PageNumber == 17);

            Assert.True(p14.IsSecondary);
            Assert.True(p16.IsSecondary);
            Assert.True(p17.IsSecondary);
        }
    }
}