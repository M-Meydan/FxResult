using FxResult.Core;

namespace FxResult.UnitTest
{
    public class ResultTestBase
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
