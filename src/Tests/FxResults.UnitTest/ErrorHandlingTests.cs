using FxResults.Core;
using FxResults.Extensions;

namespace FxResults.UnitTest
{
    [TestFixture]
    public class ErrorHandlingTests : ResultExtensionsTestBase
    {
        [Test]
        public void Tap_WhenActionThrows_ReturnsError()
        {
            var result = Result.Success(10)
                                .Tap((x) => throw new Exception("Test error")); 

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error.Message, Is.EqualTo("Test error"));
            });
        }

        [Test]
        public async Task TapAsync_WhenActionThrows_ReturnsError()
        {
            var result = await Result.Success(10).TapAsync(async (x) =>
            {
                await Task.Delay(10);
                throw new Exception("Async error");
            });

            Assert.That(result.Error.Message, Is.EqualTo("Async error"));
        }

        [Test]
        public void Then_WhenTransformThrows_ReturnsError()
        {
            var result = Result.Success(10).ThenTry<int, int>(x =>
            {
                throw new Exception("Transform error");
            });

            Assert.That(result.Error.Message, Is.EqualTo("Transform error"));
        }
    }
}
