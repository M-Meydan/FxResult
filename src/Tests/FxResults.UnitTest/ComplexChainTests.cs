using FxResults.Core;
using FxResults.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FxResults.UnitTest
{
    [TestFixture]
    public class ComplexChainTests : ResultExtensionsTestBase
    {
        [Test]
        public async Task MixedChain_ExecutesInCorrectOrder()
        {
            var result = await Result.Success(5)
                .Then(x => x * 2)
                .Tap(x => ExecutionLog.Add($"SyncTap: {x}"))
                .ThenAsync(async x => {
                    await Task.Delay(10);
                    return x + 3;
                })
                .TapAsync(async x => {
                    await Task.Delay(10);
                    ExecutionLog.Add($"AsyncTap: {x}");
                })
                .Then(x => x * 2);

            Assert.Multiple(() => {
                Assert.That(result.Value, Is.EqualTo(26));
                Assert.That(ExecutionLog, Is.EqualTo(new[] {
                "SyncTap: 10",
                "AsyncTap: 13"
            }));
            });
        }

        [Test]
        public void FailedChain_ShortCircuits()
        {
            var result = Result.Success(10).Then(x => {
                    ExecutionLog.Add("Step1");
                    return x * 2;
                })
                .Then(x => {
                    ExecutionLog.Add("Step2");
                    return Result.Fail<int>("Failed");
                })
                .Then(x => {
                    ExecutionLog.Add("Step3");
                    return x + 100;
                });

            Assert.Multiple(() => {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(ExecutionLog, Is.EqualTo(new[] {
                "Step1",
                "Step2"
            }));
            });
        }
    }
}
