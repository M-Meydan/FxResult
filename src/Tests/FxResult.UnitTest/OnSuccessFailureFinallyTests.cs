using FxResult.Core;
using FxResult.ResultExtensions;


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
            var error = new Error("Test error");
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
            var error = new Error("Test error");
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
            var error = new Error("Test error");
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
            var error = new Error("Test error");
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
            var error = new Error("Test error");
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
            var error = new Error("Test error");
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
                return new Error("replaced", "REPLACED");
            });

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(modified.IsFailure, Is.True);
                Assert.That(modified.Error!.Message, Is.EqualTo("replaced"));
                Assert.That(modified.Error!.Code, Is.EqualTo("REPLACED"));
            });
        }


    }
}
