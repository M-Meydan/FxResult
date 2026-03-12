using FxResult.Core;
using FxResult.ResultExtensions;
using FxResult.ResultExtensions.SideEffects;


namespace FxResult.UnitTest
{
    [TestFixture]
    public class OnSuccessFailureFinallyTests
    {        
        [Test]
        public void OnSuccess_ExecutesActionOnSuccessResult()
        {
            // Arrange
            var value = 42;
            var result = Result<int>.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnSuccess(v => 
            {
                Assert.That(v.Value, Is.EqualTo(value));
                actionExecuted = true;
                return v; // Return the value to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public void OnSuccess_DoesNotExecuteActionOnFailureResult()
        {
            // Arrange
            var error = new Error("TEST", "Test error");
            var result = Result<int>.Fail(error);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnSuccess(v => 
            {
                actionExecuted = true;
                return v; // Return the value to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.False);
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
            });
        }
        
        [Test]
        public void OnSuccess_WithActionThatThrowsException_ReturnsFailureResult()
        {
            // Arrange
            var value = 42;
            var result = Result<int>.Success(value);
            var exceptionMessage = "Action exception";
            
            // Act
            var newResult = result.OnSuccess(v => 
            {
                throw new InvalidOperationException(exceptionMessage);
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error!.Message, Is.EqualTo(exceptionMessage));
                Assert.That(newResult.Error!.Code, Is.EqualTo("ON_SUCCESS_ERROR"));
            });
        }
        
        [Test]
        public async Task OnSuccessAsync_ExecutesActionOnSuccessResult()
        {
            // Arrange
            var value = 42;
            var result = Result<int>.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnSuccessAsync(async v => 
            {
                await Task.Delay(10); // Simulate async work
                Assert.That(v.Value, Is.EqualTo(value));
                actionExecuted = true;

                return v; // Return the value to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public async Task OnSuccessAsync_DoesNotExecuteActionOnFailureResult()
        {
            // Arrange
            var error = new Error("TEST", "Test error");
            var result = Result<int>.Fail(error);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnSuccessAsync(async v => 
            {
                await Task.Delay(10); // Simulate async work
                actionExecuted = true;
                return v; // Return the value to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.False);
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
            });
        }
        
        [Test]
        public async Task OnSuccessAsync_WithActionThatThrowsException_ReturnsFailureResult()
        {
            // Arrange
            var value = 42;
            var result = Result<int>.Success(value);
            var exceptionMessage = "Action exception";
            
            // Act
            var newResult = await result.OnSuccessAsync(async v => 
            {
                await Task.Delay(10); // Simulate async work
                throw new InvalidOperationException(exceptionMessage);
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error!.Message, Is.EqualTo(exceptionMessage));
                Assert.That(newResult.Error!.Code, Is.EqualTo("ON_SUCCESS_ASYNC_ERROR"));
            });
        }
        
        [Test]
        public void OnFailure_ExecutesActionOnFailureResult()
        {
            // Arrange
            var error = new Error("TEST", "Test error");
            var result = Result<int>.Fail(error);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnFailure(e => 
            {
                Assert.That(e.Error, Is.EqualTo(error));
                actionExecuted = true;
                return e; // Return the error to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
            });
        }
        
        [Test]
        public void OnFailure_DoesNotExecuteActionOnSuccessResult()
        {
            // Arrange
            var value = 42;
            var result = Result<int>.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnFailure(e => 
            {
                actionExecuted = true;
                return e; // Return the error to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.False);
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public async Task OnFailureAsync_ExecutesActionOnFailureResult()
        {
            // Arrange
            var error = new Error("TEST", "Test error");
            var result = Result<int>.Fail(error);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnFailureAsync(async e => 
            {
                await Task.Delay(10); // Simulate async work
                Assert.That(e.Error, Is.EqualTo(error));
                actionExecuted = true;
                return e; // Return the error to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
            });
        }
        
        [Test]
        public async Task OnFailureAsync_DoesNotExecuteActionOnSuccessResult()
        {
            // Arrange
            var value = 42;
            var result = Result<int>.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnFailureAsync(async e => 
            {
                await Task.Delay(10); // Simulate async work
                actionExecuted = true;
                return e; // Return the error to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.False);
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public void OnFinally_ExecutesActionOnSuccessResult()
        {
            // Arrange
            var value = 42;
            var result = Result<int>.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnFinally(r => 
            {
                Assert.That(r, Is.EqualTo(result));
                actionExecuted = true;
                return r; // Return the result to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public void OnFinally_ExecutesActionOnFailureResult()
        {
            // Arrange
            var error = new Error("TEST", "Test error");
            var result = Result<int>.Fail(error);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnFinally(r => 
            {
                Assert.That(r, Is.EqualTo(result));
                actionExecuted = true;
                return r; // Return the result to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
            });
        }
        
        [Test]
        public async Task OnFinallyAsync_ExecutesActionOnSuccessResult()
        {
            // Arrange
            var value = 42;
            var result = Result<int>    .Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnFinallyAsync(async r => 
            {
                await Task.Delay(10); // Simulate async work
                Assert.That(r, Is.EqualTo(result));
                actionExecuted = true;
                return r; // Return the result to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public async Task OnFinallyAsync_ExecutesActionOnFailureResult()
        {
            // Arrange
            var error = new Error("TEST", "Test error");
            var result = Result<int>.Fail(error);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnFinallyAsync(async r => 
            {
                await Task.Delay(10); // Simulate async work
                Assert.That(r, Is.EqualTo(result));
                actionExecuted = true;
                return r; // Return the result to keep the result type consistent
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
            });
        }

        [Test]
        public void OnFailure_ModifiesErrorToCustom()
        {
            // Arrange
            var original = Result<int>.Fail("initial fail");

            // Act
            var modified = original.OnFailure(_ =>
            {
                return new Error("REPLACED", "replaced");
            });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(modified.IsFailure, Is.True);
                Assert.That(modified.Error!.Message, Is.EqualTo("replaced"));
                Assert.That(modified.Error!.Code, Is.EqualTo("REPLACED"));
            });
        }

        [Test]
        public void OnSuccess_ShouldTransformValue_WhenSuccess()
        {
            var result = Result<int>.Success(2);
            var next = result.OnSuccess(r => r.Then(x => x + 1));

            Assert.That(next.IsSuccess, Is.True);
            Assert.That(next.Value, Is.EqualTo(3));
        }

        [Test]
        public void OnFailure_ShouldRecoverToSuccess_WhenFailure()
        {
            var result = Result<int>.Fail(new Error("E", "m"));
            var next = result.OnFailure(_ => Result<int>.Success(99));

            Assert.That(next.IsSuccess, Is.True);
            Assert.That(next.Value, Is.EqualTo(99));
        }

        [Test]
        public void OnFinally_ShouldEnrichError_Always()
        {
            var result = Result<int>.Fail(new Error("E", "m"));
            var next = result.OnFinally(r =>
                Result<int>.Fail(r.Error.WithContext(source: "S", caller: "C")));

            Assert.Multiple(() =>
            {
                Assert.That(next.IsFailure, Is.True);
                Assert.That(next.Error.Source, Is.EqualTo("S"));
                Assert.That(next.Error.Caller, Is.EqualTo("C"));
            });
        }

        [Test]
        public async Task OnSuccessAsync_WithCancellationToken_ExecutesAction()
        {
            var result = Result<int>.Success(42);
            using var cts = new CancellationTokenSource();
            var observed = CancellationToken.None;

            var next = await result.OnSuccessAsync(async (r, ct) =>
            {
                await Task.Yield();
                observed = ct;
                return r;
            }, cts.Token);

            Assert.Multiple(() =>
            {
                Assert.That(next.IsSuccess, Is.True);
                Assert.That(observed, Is.EqualTo(cts.Token));
            });
        }

        [Test]
        public async Task OnSuccessAsync_WithCancellationToken_SkipsOnFailure()
        {
            var result = Result<int>.Fail("fail");
            var called = false;

            var next = await result.OnSuccessAsync(async (r, ct) =>
            {
                await Task.Yield();
                called = true;
                return r;
            });

            Assert.Multiple(() =>
            {
                Assert.That(called, Is.False);
                Assert.That(next.IsFailure, Is.True);
            });
        }

        [Test]
        public async Task OnFailure_TaskResult_ExecutesOnFailure()
        {
            var task = Task.FromResult(Result<int>.Fail(new Error("E", "m")));
            var called = false;

            var next = await task.OnFailure(r =>
            {
                called = true;
                return Result<int>.Success(99);
            });

            Assert.Multiple(() =>
            {
                Assert.That(called, Is.True);
                Assert.That(next.IsSuccess, Is.True);
                Assert.That(next.Value, Is.EqualTo(99));
            });
        }

        [Test]
        public async Task OnFailure_TaskResult_SkipsOnSuccess()
        {
            var task = Task.FromResult(Result<int>.Success(42));
            var called = false;

            var next = await task.OnFailure(r =>
            {
                called = true;
                return r;
            });

            Assert.Multiple(() =>
            {
                Assert.That(called, Is.False);
                Assert.That(next.IsSuccess, Is.True);
                Assert.That(next.Value, Is.EqualTo(42));
            });
        }

        [Test]
        public async Task OnFailureAsync_WithCancellationToken_ExecutesOnFailure()
        {
            var result = Result<int>.Fail(new Error("E", "m"));
            using var cts = new CancellationTokenSource();
            var observed = CancellationToken.None;

            var next = await result.OnFailureAsync(async (r, ct) =>
            {
                await Task.Yield();
                observed = ct;
                return Result<int>.Success(99);
            }, cts.Token);

            Assert.Multiple(() =>
            {
                Assert.That(next.IsSuccess, Is.True);
                Assert.That(next.Value, Is.EqualTo(99));
                Assert.That(observed, Is.EqualTo(cts.Token));
            });
        }

        [Test]
        public async Task OnFailureAsync_WithCancellationToken_SkipsOnSuccess()
        {
            var result = Result<int>.Success(42);
            var called = false;

            var next = await result.OnFailureAsync(async (r, ct) =>
            {
                await Task.Yield();
                called = true;
                return r;
            });

            Assert.Multiple(() =>
            {
                Assert.That(called, Is.False);
                Assert.That(next.IsSuccess, Is.True);
            });
        }

        [Test]
        public async Task OnFinally_TaskResult_ExecutesAlways()
        {
            var task = Task.FromResult(Result<int>.Fail(new Error("E", "m")));
            var called = false;

            var next = await task.OnFinally(r =>
            {
                called = true;
                return r;
            });

            Assert.Multiple(() =>
            {
                Assert.That(called, Is.True);
                Assert.That(next.IsFailure, Is.True);
            });
        }

        [Test]
        public async Task OnFinallyAsync_TaskResult_ExecutesAlways()
        {
            var task = Task.FromResult(Result<int>.Success(42));
            var called = false;

            var next = await task.OnFinallyAsync(async r =>
            {
                await Task.Yield();
                called = true;
                return r;
            });

            Assert.Multiple(() =>
            {
                Assert.That(called, Is.True);
                Assert.That(next.IsSuccess, Is.True);
                Assert.That(next.Value, Is.EqualTo(42));
            });
        }

        [Test]
        public async Task OnFinallyAsync_WithCancellationToken_PassesToken()
        {
            var result = Result<int>.Success(42);
            using var cts = new CancellationTokenSource();
            var observed = CancellationToken.None;

            var next = await result.OnFinallyAsync(async (r, ct) =>
            {
                await Task.Yield();
                observed = ct;
                return r;
            }, cts.Token);

            Assert.Multiple(() =>
            {
                Assert.That(next.IsSuccess, Is.True);
                Assert.That(observed, Is.EqualTo(cts.Token));
            });
        }

        [Test]
        public async Task OnFinallyAsync_TaskResult_WithCancellationToken_PassesToken()
        {
            var task = Task.FromResult(Result<int>.Fail(new Error("E", "m")));
            using var cts = new CancellationTokenSource();
            var observed = CancellationToken.None;

            var next = await task.OnFinallyAsync(async (r, ct) =>
            {
                await Task.Yield();
                observed = ct;
                return r;
            }, cts.Token);

            Assert.Multiple(() =>
            {
                Assert.That(next.IsFailure, Is.True);
                Assert.That(observed, Is.EqualTo(cts.Token));
            });
        }

    }
}
