using NUnit.Framework;
using FxResults.Core;
using System;
using System.Linq;

namespace FxResults.Tests
{
    [TestFixture]
    public class PaginatedResultTests
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var items = new[] { "item1", "item2", "item3" };
            var totalCount = 10;
            var page = 2;
            var pageSize = 3;
            
            // Act
            var result = new PaginatedResult<string>(items, totalCount, page, pageSize);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Items, Is.EquivalentTo(items));
                Assert.That(result.TotalCount, Is.EqualTo(totalCount));
                Assert.That(result.Page, Is.EqualTo(page));
                Assert.That(result.PageSize, Is.EqualTo(pageSize));
            });
        }
        
        [Test]
        public void TotalPages_CalculatesCorrectly_WhenEvenlyDivisible()
        {
            // Arrange
            var items = new[] { "item1", "item2", "item3" };
            var totalCount = 9;
            var pageSize = 3;
            
            // Act
            var result = new PaginatedResult<string>(items, totalCount, 1, pageSize);
            
            // Assert
            Assert.That(result.TotalPages, Is.EqualTo(3));
        }
        
        [Test]
        public void TotalPages_CalculatesCorrectly_WhenNotEvenlyDivisible()
        {
            // Arrange
            var items = new[] { "item1", "item2", "item3" };
            var totalCount = 10;
            var pageSize = 3;
            
            // Act
            var result = new PaginatedResult<string>(items, totalCount, 1, pageSize);
            
            // Assert
            Assert.That(result.TotalPages, Is.EqualTo(4));
        }
        
        [Test]
        public void Constructor_WithEmptyItems_CreatesEmptyItemsList()
        {
            // Arrange
            var items = Array.Empty<string>();
            var totalCount = 0;
            var page = 1;
            var pageSize = 10;
            
            // Act
            var result = new PaginatedResult<string>(items, totalCount, page, pageSize);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.TotalCount, Is.EqualTo(totalCount));
                Assert.That(result.TotalPages, Is.EqualTo(0));
            });
        }
        
        [Test]
        public void Constructor_WithPageSizeOfOne_CalculatesTotalPagesCorrectly()
        {
            // Arrange
            var items = new[] { "item1" };
            var totalCount = 5;
            var page = 1;
            var pageSize = 1;
            
            // Act
            var result = new PaginatedResult<string>(items, totalCount, page, pageSize);
            
            // Assert
            Assert.That(result.TotalPages, Is.EqualTo(5));
        }
        
        [Test]
        public void Constructor_WithTotalCountEqualToPageSize_CalculatesTotalPagesCorrectly()
        {
            // Arrange
            var items = new[] { "item1", "item2", "item3" };
            var totalCount = 3;
            var page = 1;
            var pageSize = 3;
            
            // Act
            var result = new PaginatedResult<string>(items, totalCount, page, pageSize);
            
            // Assert
            Assert.That(result.TotalPages, Is.EqualTo(1));
        }
        
        [Test]
        public void Constructor_WithTotalCountLessThanPageSize_CalculatesTotalPagesCorrectly()
        {
            // Arrange
            var items = new[] { "item1", "item2" };
            var totalCount = 2;
            var page = 1;
            var pageSize = 5;
            
            // Act
            var result = new PaginatedResult<string>(items, totalCount, page, pageSize);
            
            // Assert
            Assert.That(result.TotalPages, Is.EqualTo(1));
        }
        
        [Test]
        public void Items_IsReadOnly()
        {
            // Arrange
            var items = new[] { "item1", "item2", "item3" };
            var result = new PaginatedResult<string>(items, 10, 1, 3);
            
            // Act & Assert
            Assert.That(result.Items, Is.TypeOf<System.Collections.ObjectModel.ReadOnlyCollection<string>>());
        }
    }
}
