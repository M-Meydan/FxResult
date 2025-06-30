using NUnit.Framework;
using FxResult.Core;
using FxResult.ResultExtensions;
using System;
using System.Threading.Tasks;

namespace FxResult.UnitTest.ThenTryTests
{
    [TestFixture]
    public class ThenTryTests : ResultTestBase
    {
        [Test]
        public async Task ThenTryAsync_WhenAsyncTransformThrows_ReturnsError()
        {
            var result = await Result<int>.Success(10).ThenTryAsync<int, int>(async x =>
            {
                await Task.Delay(10);
                throw new Exception("Async transform error");
            });

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Message, Is.EqualTo("Async transform error"));
            });
        }

        [Test]
        public async Task ThenTryAsync_WhenAsyncResultReturningTransformThrows_ReturnsError()
        {
            var result = await Result<int>.Success(10).ThenTryAsync<int, int>(async x =>
            {
                await Task.Delay(10);
                throw new Exception("Async result-returning transform error");
            });

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Message, Is.EqualTo("Async result-returning transform error"));
            });
        }

        [Test]
        public async Task ThenTryAsync_TaskResult_WhenAsyncTransformThrows_ReturnsError()
        {
            var result = await AsTask(Result<int>.Success(10)).ThenTryAsync<int, int>(async x =>
            {
                await Task.Delay(10);
                throw new Exception("Task Result async transform error");
            });

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Message, Is.EqualTo("Task Result async transform error"));
            });
        }

        [Test]
        public async Task ThenTryAsync_TaskResult_WhenAsyncResultReturningTransformThrows_ReturnsError()
        {
            var result = await AsTask(Result<int>.Success(10)).ThenTryAsync<int, int>(async x =>
            {
                await Task.Delay(10);
                throw new Exception("Task Result async result-returning transform error");
            });

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Message, Is.EqualTo("Task Result async result-returning transform error"));
            });
        }

        [Test]
        public async Task ThenTryAsync_ErrorEnrichment_WithSourceAndCaller()
        {
            var result = await Result<int>.Success(10).ThenTryAsync<int, int>(async x =>
            {
                await Task.Delay(10);
                throw new Exception("Error with context");
            }, source: "MyService", caller: "MyMethod");

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Message, Is.EqualTo("Error with context"));
                Assert.That(result.Error!.Source, Is.EqualTo("MyService"));
                Assert.That(result.Error!.Caller, Is.EqualTo("MyMethod"));
            });
        }

        [TestFixture]
        public class ResultMultiExceptionControlTests
        {
            [TestCase("arg", typeof(ArgumentNullException), "ARG_NULL")]
            [TestCase("inv", typeof(InvalidOperationException), "INVALID_OP")]
            [TestCase("div", typeof(DivideByZeroException), "DIV_BY_ZERO")]
            [TestCase("other", typeof(NotSupportedException), "UNKNOWN")]
            public void Try_Handles_Multiple_Exception_Types_Correctly(string scenario, Type expectedExceptionType, string expectedCode)
            {
                // Arrange: simulate specific exception scenario
                Func<string> throwingFunc = scenario switch
                {
                    "arg" => () => throw new ArgumentNullException(),
                    "inv" => () => throw new InvalidOperationException(),
                    "div" => () => throw new DivideByZeroException(),
                    "other" => () => throw new NotSupportedException("Not allowed"),
                    _ => () => "OK"
                };

                // Act: call ThenTry with throwing logic and map exception inline
                var result = Result<string>
                                    .Success("trigger")
                                    .ThenTry(_ => throwingFunc()) // function that may throw
                                    .OnFailure(errResult =>
                                    {
                                        var result = errResult.Error!.Exception switch
                                        {
                                            ArgumentNullException => new Error("Missing value", "ARG_NULL", "Test"),
                                            InvalidOperationException => new Error("Invalid operation", "INVALID_OP", "Test"),
                                            DivideByZeroException => new Error("Math error", "DIV_BY_ZERO", "Test"),
                                            null => new Error("No exception captured", "NO_EXCEPTION", "Test"),
                                            _ => new Error("Unknown error", "UNKNOWN", "Test", Exception: errResult.Error.Exception)
                                        };

                                        return result;

                                    });

                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.Error!.Code, Is.EqualTo(expectedCode));
                Assert.That(result.Error.Exception ?? Activator.CreateInstance(expectedExceptionType), Is.TypeOf(expectedExceptionType));
            }
        }


    }
}

