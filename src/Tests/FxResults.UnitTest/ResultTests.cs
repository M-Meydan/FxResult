using NUnit.Framework;
using FxResults.Core;
using System;

namespace FxResults.Tests
{
    [TestFixture]
    public class ResultTests
    {
        [Test]
        public void Success_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var value = "test value";
            
            // Act
            var result = new Result<string>(value);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(value));
                Assert.That(result.Error, Is.Null);
            });
        }
        
        [Test]
        public void Failure_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var error = new Error("Test error", "TEST_ERROR", "TestSource");
            
            // Act
            var result = new Result<string>(error);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo(error));
                Assert.Throws<InvalidOperationException>(() => _ = result.Value);
            });
        }
        
        [Test]
        public void Success_FactoryMethod_CreatesSuccessResult()
        {
            // Arrange
            var value = 42;
            
            // Act
            var result = Result.Success(value);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(value));
                Assert.That(result.Error, Is.Null);
            });
        }
        
        [Test]
        public void Fail_WithError_CreatesFailureResult()
        {
            // Arrange
            var error = new Error("Test error", "TEST_ERROR");
            
            // Act
            var result = Result.Fail<int>(error);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo(error));
                Assert.Throws<InvalidOperationException>(() => _ = result.Value);
            });
        }
        
        [Test]
        public void Fail_WithMessage_CreatesFailureResult()
        {
            // Arrange
            var message = "Test error message";
            
            // Act
            var result = Result.Fail<int>(message);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Message, Is.EqualTo(message));
                Assert.That(result.Error!.Code, Is.EqualTo("error"));
                Assert.Throws<InvalidOperationException>(() => _ = result.Value);
            });
        }
        
        [Test]
        public void Fail_WithException_CreatesFailureResult()
        {
            // Arrange
            var exception = new ArgumentException("Invalid argument");
            var source = "TestSource";
            
            // Act
            var result = Result.Fail<int>(exception, source);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Message, Is.EqualTo(exception.Message));
                Assert.That(result.Error!.Code, Is.EqualTo(exception.GetType().Name));
                Assert.That(result.Error!.Source, Is.EqualTo(source));
                Assert.That(result.Error!.Exception, Is.EqualTo(exception));
                Assert.Throws<InvalidOperationException>(() => _ = result.Value);
            });
        }
        
        [Test]
        public void TryGetValue_OnSuccessResult_ReturnsTrueAndValue()
        {
            // Arrange
            var value = "test value";
            var result = Result.Success(value);
            
            // Act
            var success = result.TryGetValue(out var retrievedValue);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(retrievedValue, Is.EqualTo(value));
            });
        }
        
        [Test]
        public void TryGetValue_OnFailureResult_ReturnsFalseAndDefault()
        {
            // Arrange
            var error = new Error("Test error");
            var result = Result.Fail<string>(error);
            
            // Act
            var success = result.TryGetValue(out var retrievedValue);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(success, Is.False);
                Assert.That(retrievedValue, Is.Null);
            });
        }
        
        [Test]
        public void ImplicitConversion_FromValue_CreatesSuccessResult()
        {
            // Arrange
            int value = 42;
            
            // Act
            Result<int> result = value;
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(value));
            });
        }
        
        [Test]
        public void ImplicitConversion_FromError_CreatesFailureResult()
        {
            // Arrange
            var error = new Error("Test error");
            
            // Act
            Result<int> result = error;
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo(error));
            });
        }
        
        [Test]
        public void Success_WithNullValue_CreatesSuccessResultWithNull()
        {
            // Arrange & Act
            var result = Result.Success<string>(null!);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.Null);
            });
        }
        
        [Test]
        public void Success_WithDefaultValue_CreatesSuccessResultWithDefault()
        {
            // Arrange & Act
            var result = Result.Success(default(int));
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(default(int)));
            });
        }
        
        [Test]
        public void TryGetValue_WithNullValue_ReturnsTrueAndNull()
        {
            // Arrange
            var result = Result.Success<string>(null!);
            
            // Act
            var success = result.TryGetValue(out var retrievedValue);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(retrievedValue, Is.Null);
            });
        }
    }
}
