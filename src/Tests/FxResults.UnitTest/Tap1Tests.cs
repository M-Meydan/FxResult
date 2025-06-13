using FxResults.Core;
using FxResults.Extensions;

namespace FxResults.UnitTest
{
    [TestFixture]
    public class Tap1Tests : ResultExtensionsTestBase
    {
        [Test]
        public void SyncTap_OnSuccess_ExecutesAction()
        {
            Result.Success(10)
                .Tap(x => ExecutionLog.Add($"Value: {x}"));

            Assert.That(ExecutionLog, Has.Exactly(1).EqualTo("Value: 10"));
        }

        [Test]
        public void SyncTap_OnFailure_DoesNotExecute()
        {
            Result.Fail<int>("Error")
                .Tap(x => ExecutionLog.Add("Should not run"));

            Assert.That(ExecutionLog, Is.Empty);
        }

        [Test]
        public async Task AsyncTap_OnSuccess_ExecutesAsyncAction()
        {
            await Result.Success(10).TapAsync(async x => {
                    await Task.Delay(10);
                    ExecutionLog.Add($"Async: {x}");
                });

            Assert.That(ExecutionLog, Has.Exactly(1).EqualTo("Async: 10"));
        }

        [Test]
        public async Task TaskTap_OnSuccess_ExecutesSyncAction()
        {
            await AsTask(Result.Success(10))
                .TapAsync(x => ExecutionLog.Add($"FromTask: {x}"));

            Assert.That(ExecutionLog, Has.Exactly(1).EqualTo("FromTask: 10"));
        }

        [Test]
        public async Task TaskTapAsync_ExecutesAsyncAction()
        {
            await AsTask(Result.Success(10))
                .TapAsync(async x => {
                    await Task.Delay(10);
                    ExecutionLog.Add($"AsyncTask: {x}");
                });

            Assert.That(ExecutionLog, Has.Exactly(1).EqualTo("AsyncTask: 10"));
        }
    }
}
