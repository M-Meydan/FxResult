using FxResults.Core;
using FxResults.Extensions;

namespace FxResults.UnitTest
{
    [TestFixture]
    public class Then1Tests : ResultExtensionsTestBase
    {
        [Test]
        public void SyncThen_TransformsValue()
        {
            var result = Result.Success(5)
                .Then(x => x * 2);

            Assert.That(result.Value, Is.EqualTo(10));
        }

        [Test]
        public async Task AsyncThen_TransformsAsync()
        {
            var result = await Result.Success(5)
                .ThenAsync(async x => {
                    await Task.Delay(10);
                    return x * 2;
                });

            Assert.That(result.Value, Is.EqualTo(10));
        }

        [Test]
        public async Task TaskThen_TransformsSync()
        {
            var result = await AsTask(Result.Success(5))
                .Then(x => x * 2);

            Assert.That(result.Value, Is.EqualTo(10));
        }
    }
}
