using FxResults.Core;

namespace FxResults.UnitTest
{
    public class ResultExtensionsTestBase
    {
        protected List<string> ExecutionLog;

        [SetUp]
        public void BaseSetup()
        {
            ExecutionLog = new List<string>();
        }

        public static Task<Result<T>> AsTask<T>(Result<T> result)
            => Task.FromResult(result);
    }
}
