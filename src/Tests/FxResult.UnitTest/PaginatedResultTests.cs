using FxResult.ResultExtensions;
using FxResult.ResultExtensions.Helpers;

namespace FxResult.UnitTest
{
    [TestFixture]
    public class PaginatedResultTests
    {
        [Test]
        public void ToPagedResult_IQueryable_BuildsCorrectResultWithMeta()
        {
            // Arrange
            var data = Enumerable.Range(1, 25).AsQueryable();

            // Act
            var result = data.ToPagedResult(page: 2, pageSize: 10);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Count, Is.EqualTo(10));
                Assert.That(result.Value.First(), Is.EqualTo(11));

                var meta = result.Meta;
                Assert.That(meta.Pagination.Page, Is.EqualTo(2));
                Assert.That(meta.Pagination.PageSize, Is.EqualTo(10));
                Assert.That(meta.Pagination.TotalCount, Is.EqualTo(25));
                Assert.That(meta.Pagination.TotalPages, Is.EqualTo(3));
                Assert.That(meta.Pagination.HasNextPage, Is.True);
                Assert.That(meta.Pagination.HasPreviousPage, Is.True);
            });
        }

        [Test]
        public void ToPagedResult_IEnumerable_BuildsCorrectResultWithMeta()
        {
            // Arrange
            var data = Enumerable.Range(1, 15);

            // Act
            var result = data.ToPagedResult(page: 1, pageSize: 5);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Count, Is.EqualTo(5));
                Assert.That(result.Value.First(), Is.EqualTo(1));

                var meta = result.Meta;
                Assert.That(meta.Pagination.Page, Is.EqualTo(1));
                Assert.That(meta.Pagination.PageSize, Is.EqualTo(5));
                Assert.That(meta.Pagination.TotalCount, Is.EqualTo(15));
                Assert.That(meta.Pagination.TotalPages, Is.EqualTo(3));
                Assert.That(meta.Pagination.HasNextPage, Is.True);
                Assert.That(meta.Pagination.HasPreviousPage, Is.False);
            });
        }

        [Test]
        public void ToPagedResult_AppendsCustomMetaData()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).AsQueryable();

            // Act
            var result = data.ToPagedResult(1, 5)
                .WithMetaData("apiVersion", "v1")
                .WithMetaData("requestedBy", "tester");

            // Assert
            var meta = result.Meta;
            Assert.That(meta.Additional["apiVersion"], Is.EqualTo("v1"));
            Assert.That(meta.Additional["requestedBy"], Is.EqualTo("tester"));
        }

        [TestCaseSource(nameof(NegativePagingParametersTestCases))]
        public void ToPagedResult_WithNegativeOrZeroParameters_DefaultsToMinimumValues(IEnumerable<int> source, int page, int pageSize, int expectedPage, int expectedPageSize, int expectedCount)
        {
            // Act
            var result = source.ToPagedResult(page, pageSize);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Count, Is.EqualTo(expectedCount));
                Assert.That(result.Meta.Pagination.Page, Is.EqualTo(expectedPage));
                Assert.That(result.Meta.Pagination.PageSize, Is.EqualTo(expectedPageSize));
            });
        }

        private static IEnumerable<object[]> NegativePagingParametersTestCases()
        {
            yield return new object[] { Enumerable.Range(1, 10), -1, 5, 1, 5, 5 };
            yield return new object[] { Enumerable.Range(1, 10), 1, -5, 1, 1, 1 };
            yield return new object[] { Enumerable.Range(1, 10), 0, 5, 1, 5, 5 };
            yield return new object[] { Enumerable.Range(1, 10), 1, 0, 1, 1, 1 };
        }
    }
}
