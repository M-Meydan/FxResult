using NUnit.Framework;
using FxResults.Core;
using FxResults.Extensions;
using System;
using System.Threading.Tasks;
using FxResults.Extensions.FailIf;

namespace FxResults.Tests
{
    [TestFixture]
    public class ResultExtensionsBasicTests
    {
        [Test]
        public void FailIfNull_WithNonNullValue_ReturnsSuccessResult()
        {
            // Arrange
            string value = "test";
            
            // Act
            var result = value.FailIfNull("Value cannot be null");
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public void FailIfNull_WithNullValue_ReturnsFailureResult()
        {
            // Arrange
            string? value = null;
            var errorMessage = "Value cannot be null";
            var errorCode = "NULL_VALUE";
            var source = "TestSource";
            
            // Act
            var result = value.FailIfNull(errorMessage, errorCode, source);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Message, Is.EqualTo(errorMessage));
                Assert.That(result.Error!.Code, Is.EqualTo(errorCode));
                Assert.That(result.Error!.Source, Is.EqualTo(source));
            });
        }
        
        [Test]
        public void FailIf_WithPredicateFalse_ReturnsOriginalResult()
        {
            // Arrange
            var value = 42;
            var result = Result.Success(value);
            
            // Act
            var newResult = result.FailIf(v => v < 0, "NEGATIVE", "Value cannot be negative");
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public void FailIf_WithPredicateTrue_ReturnsFailureResult()
        {
            // Arrange
            var value = -5;
            var result = Result.Success(value);
            var errorCode = "NEGATIVE";
            var errorMessage = "Value cannot be negative";
            
            // Act
            var newResult = result.FailIf(v => v < 0, errorCode, errorMessage);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error!.Code, Is.EqualTo(errorCode));
                Assert.That(newResult.Error!.Message, Is.EqualTo(errorMessage));
            });
        }
        
        [Test]
        public void FailIf_OnAlreadyFailedResult_ReturnsOriginalResult()
        {
            // Arrange
            var originalError = new Error("Original error");
            var result = Result.Fail<int>(originalError);
            
            // Act
            var newResult = result.FailIf(v => v < 0, "NEGATIVE", "Value cannot be negative");
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(originalError));
            });
        }
        
        [Test]
        public void FailIf_WithConditionFalse_ReturnsOriginalResult()
        {
            // Arrange
            var value = 42;
            var result = Result.Success(value);
            
            // Act
            var newResult = result.FailIf(false, "CONDITION_FAILED", "Condition failed");
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public void FailIf_WithConditionTrue_ReturnsFailureResult()
        {
            // Arrange
            var value = 42;
            var result = Result.Success(value);
            var errorCode = "CONDITION_FAILED";
            var errorMessage = "Condition failed";
            
            // Act
            var newResult = result.FailIf(true, errorCode, errorMessage);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error!.Code, Is.EqualTo(errorCode));
                Assert.That(newResult.Error!.Message, Is.EqualTo(errorMessage));
            });
        }
        
        [Test]
        public void FailIf_WithCustomErrorFactory_UsesFactoryForError()
        {
            // Arrange
            var value = -5;
            var result = Result.Success(value);
            var customError = new Error("Custom error message", "CUSTOM_CODE", "CustomSource");
            
            // Act
            var newResult = result.FailIf(v => v < 0, _ => customError);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(customError));
            });
        }
        
        [Test]
        public void Then_WithValueTransformation_TransformsValue()
        {
            // Arrange
            var value = 5;
            var result = Result.Success(value);
            
            // Act
            var newResult = result.Then(v => v * 2);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value * 2));
            });
        }
        
        [Test]
        public void Then_OnFailedResult_PropagatesError()
        {
            // Arrange
            var error = new Error("Test error");
            var result = Result.Fail<int>(error);
            
            // Act
            var newResult = result.Then(v => v * 2);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error!.Message, Is.EqualTo(error.Message));
                // The error should have the caller name added
                Assert.That(newResult.Error!.Source, Is.Not.Null);
            });
        }
        
        [Test]
        public void Then_WithResultTransformation_ReturnsTransformedResult()
        {
            // Arrange
            var value = 5;
            var result = Result.Success(value);
            
            // Act
            var newResult = result.Then(v => Result.Success(v.ToString()));
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value.ToString()));
            });
        }
        
        [Test]
        public void Then_WithResultTransformationToFailure_ReturnsFailureResult()
        {
            // Arrange
            var value = 5;
            var result = Result.Success(value);
            var error = new Error("Transformation error");
            
            // Act
            var newResult = result.Then(v => Result.Fail<string>(error));
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
            });
        }
        
        [Test]
        public async Task ThenAsync_WithValueTransformation_TransformsValue()
        {
            // Arrange
            var value = 5;
            var result = Result.Success(value);
            
            // Act
            var newResult = await result.ThenAsync(v => Task.FromResult(v * 2));
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value * 2));
            });
        }
        
        [Test]
        public async Task ThenAsync_OnFailedResult_PropagatesError()
        {
            // Arrange
            var error = new Error("Test error");
            var result = Result.Fail<int>(error);
            
            // Act
            var newResult = await result.ThenAsync(v => Task.FromResult(v * 2));
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error!.Message, Is.EqualTo(error.Message));
                // The error should have the caller name added
                Assert.That(newResult.Error!.Source, Is.Not.Null);
            });
        }
        
        [Test]
        public async Task ThenAsync_WithResultTransformation_ReturnsTransformedResult()
        {
            // Arrange
            var value = 5;
            var result = Result.Success(value);
            
            // Act
            var newResult = await result.ThenAsync(v => Task.FromResult(Result.Success(v.ToString())));
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.True);
                Assert.That(newResult.Value, Is.EqualTo(value.ToString()));
            });
        }
        
        [Test]
        public async Task ThenAsync_WithResultTransformationToFailure_ReturnsFailureResult()
        {
            // Arrange
            var value = 5;
            var result = Result.Success(value);
            var error = new Error("Transformation error");
            
            // Act
            var newResult = await result.ThenAsync(v => Task.FromResult(Result.Fail<string>(error)));
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(newResult.IsSuccess, Is.False);
                Assert.That(newResult.Error, Is.EqualTo(error));
            });
        }
    }
}
