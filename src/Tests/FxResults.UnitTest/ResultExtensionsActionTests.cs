using NUnit.Framework;
using FxResults.Core;
using FxResults.Extensions;
using System;
using System.Threading.Tasks;


namespace FxResults.Tests
{
    [TestFixture]
    public class ResultExtensionsActionTests
    {
        [Test]
        public void Tap_OnSuccessResult_ExecutesActionAndReturnsOriginalResult()
        {
            // Arrange
            var value = 42;
            var result = Result.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = result.Tap(v => 
            {
                Assert.That(v, Is.EqualTo(value));
                actionExecuted = true;
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
                Assert.That(newResult, Is.EqualTo(result));
            });
        }
        
        [Test]
        public void Tap_OnFailureResult_DoesNotExecuteActionAndReturnsOriginalResult()
        {
            // Arrange
            var error = new Error("Test error");
            var result = Result.Fail<int>(error);
            var actionExecuted = false;
            
            // Act
            var newResult = result.Tap(v => 
            {
                actionExecuted = true;
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.False);
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
                Assert.That(newResult, Is.EqualTo(result));
            });
        }
        
        [Test]
        public void Tap_WithActionThatThrowsException_ReturnsFailureResult()
        {
            // Arrange
            var value = 42;
            var result = Result.Success(value);
            var exceptionMessage = "Action exception";
            
            // Act
            var newResult = result.Tap(v => 
            {
                throw new InvalidOperationException(exceptionMessage);
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error!.Message, Is.EqualTo(exceptionMessage));
                Assert.That(newResult.Error!.Code, Is.EqualTo("InvalidOperationException"));
            });
        }
        
        [Test]
        public async Task TapAsync_OnSuccessResult_ExecutesActionAndReturnsOriginalResult()
        {
            // Arrange
            var value = 42;
            var result = Result.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.TapAsync(async v => 
            {
                await Task.Delay(10); // Simulate async work
                Assert.That(v, Is.EqualTo(value));
                actionExecuted = true;
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
                Assert.That(newResult, Is.EqualTo(result));
            });
        }
        
        [Test]
        public async Task TapAsync_OnFailureResult_DoesNotExecuteActionAndReturnsOriginalResult()
        {
            // Arrange
            var error = new Error("Test error");
            var result = Result.Fail<int>(error);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.TapAsync(async v => 
            {
                await Task.Delay(10); // Simulate async work
                actionExecuted = true;
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.False);
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
                Assert.That(newResult, Is.EqualTo(result));
            });
        }
        
        [Test]
        public async Task TapAsync_WithActionThatThrowsException_ReturnsFailureResult()
        {
            // Arrange
            var value = 42;
            var result = Result.Success(value);
            var exceptionMessage = "Action exception";
            
            // Act
            var newResult = await result.TapAsync(async v => 
            {
                await Task.Delay(10); // Simulate async work
                throw new InvalidOperationException(exceptionMessage);
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error!.Message, Is.EqualTo(exceptionMessage));
                Assert.That(newResult.Error!.Code, Is.EqualTo("InvalidOperationException"));
            });
        }
        
        [Test]
        public void OnSuccess_ExecutesActionOnSuccessResult()
        {
            // Arrange
            var value = 42;
            var result = Result.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnSuccess(v => 
            {
                Assert.That(v, Is.EqualTo(value));
                actionExecuted = true;
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
            var result = Result.Fail<int>(error);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnSuccess(v => 
            {
                actionExecuted = true;
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
            var result = Result.Success(value);
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
            var result = Result.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnSuccessAsync(async v => 
            {
                await Task.Delay(10); // Simulate async work
                Assert.That(v, Is.EqualTo(value));
                actionExecuted = true;
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
            var result = Result.Fail<int>(error);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnSuccessAsync(async v => 
            {
                await Task.Delay(10); // Simulate async work
                actionExecuted = true;
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
            var result = Result.Success(value);
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
            var result = Result.Fail<int>(error);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnFailure(e => 
            {
                Assert.That(e, Is.EqualTo(error));
                actionExecuted = true;
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
            var result = Result.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnFailure(e => 
            {
                actionExecuted = true;
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
            var result = Result.Fail<int>(error);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnFailureAsync(async e => 
            {
                await Task.Delay(10); // Simulate async work
                Assert.That(e, Is.EqualTo(error));
                actionExecuted = true;
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
            var result = Result.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnFailureAsync(async e => 
            {
                await Task.Delay(10); // Simulate async work
                actionExecuted = true;
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
            var result = Result.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnFinally(r => 
            {
                Assert.That(r, Is.EqualTo(result));
                actionExecuted = true;
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
            var result = Result.Fail<int>(error);
            var actionExecuted = false;
            
            // Act
            var newResult = result.OnFinally(r => 
            {
                Assert.That(r, Is.EqualTo(result));
                actionExecuted = true;
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
            var result = Result.Success(value);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnFinallyAsync(async r => 
            {
                await Task.Delay(10); // Simulate async work
                Assert.That(r, Is.EqualTo(result));
                actionExecuted = true;
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
            var result = Result.Fail<int>(error);
            var actionExecuted = false;
            
            // Act
            var newResult = await result.OnFinallyAsync(async r => 
            {
                await Task.Delay(10); // Simulate async work
                Assert.That(r, Is.EqualTo(result));
                actionExecuted = true;
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True);
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
            });
        }
    }
}
