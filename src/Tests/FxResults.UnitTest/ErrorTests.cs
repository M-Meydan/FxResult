using NUnit.Framework;
using FxResults.Core;
using System;

namespace FxResults.Tests
{
    [TestFixture]
    public class ErrorTests
    {
        [Test]
        public void Constructor_WithMessageOnly_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Test error message";
            
            // Act
            var error = new Error(message);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(error.Message, Is.EqualTo(message));
                Assert.That(error.Code, Is.EqualTo("error"));
                Assert.That(error.Source, Is.Null);
                Assert.That(error.Exception, Is.Null);
            });
        }
        
        [Test]
        public void Constructor_WithMessageAndCode_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Test error message";
            var code = "TEST_ERROR";
            
            // Act
            var error = new Error(message, code);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(error.Message, Is.EqualTo(message));
                Assert.That(error.Code, Is.EqualTo(code));
                Assert.That(error.Source, Is.Null);
                Assert.That(error.Exception, Is.Null);
            });
        }
        
        [Test]
        public void Constructor_WithAllParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Test error message";
            var code = "TEST_ERROR";
            var source = "TestSource";
            var exception = new ArgumentException("Test exception");
            
            // Act
            var error = new Error(message, code,Source: source,Exception: exception);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(error.Message, Is.EqualTo(message));
                Assert.That(error.Code, Is.EqualTo(code));
                Assert.That(error.Source, Is.EqualTo(source));
                Assert.That(error.Exception, Is.EqualTo(exception));
            });
        }
        
        [Test]
        public void Create_FactoryMethod_CreatesErrorCorrectly()
        {
            // Arrange
            var message = "Test error message";
            var code = "TEST_ERROR";
            var source = "TestSource";
            var caller = "TestCaller";
            var exception = new ArgumentException("Test exception");

            // Act
            var error = new Error(message, code, Source: source,Caller: caller, Exception: exception);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(error.Message, Is.EqualTo(message));
                Assert.That(error.Code, Is.EqualTo(code));
                Assert.That(error.Source, Is.EqualTo(source));
                Assert.That(error.Caller, Is.EqualTo(caller));
                Assert.That(error.Exception, Is.EqualTo(exception));
            });
        }
        
        [Test]
        public void WithCaller_WhenSourceIsNull_AddsSource()
        {
            // Arrange
            var error = new Error("Test error");
            string source =null;
            var caller = "TestCaller";
            
            // Act
            var result = error.WithContext(caller, source);

            // Assert
            Assert.IsNotNull(result.Source);
        }
        
        [Test]
        public void WithCaller_WhenSourceIsNotNull_DoesNotChangeSource()
        {
            //var originalSource = "OriginalSource";
            var error = new Error("Test error","TEST_ERROR");
            var source = "TestSource";
            var caller = "TestCaller";

            // Act
            var result = error.WithContext(caller, source);

            // Assert
            Assert.That(result.Source, Is.EqualTo(source));
        }
        
        [Test]
        public void ImplicitConversion_FromString_CreatesErrorWithMessage()
        {
            // Arrange
            var message = "Test error message";
            
            // Act
            Error error = message;
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(error.Message, Is.EqualTo(message));
                Assert.That(error.Code, Is.EqualTo("error"));
                Assert.That(error.Source, Is.Null);
                Assert.That(error.Exception, Is.Null);
            });
        }
        
        [Test]
        public void Constructor_WithNullMessage_AcceptsNullMessage()
        {
            // Arrange & Act
            var error = new Error(null!);
            
            // Assert
            Assert.That(error.Message, Is.Null);
        }
        
        [Test]
        public void Constructor_WithNullCode_AcceptsNullCode()
        {
            // Arrange & Act
            var error = new Error("Test message", null!);
            
            // Assert
            Assert.That(error.Code, Is.Null);
        }
        
        [Test]
        public void Constructor_WithNullException_AcceptsNullException()
        {
            // Arrange & Act
            var error = new Error("Test message", "TEST_CODE", "TestSource", null);
            
            // Assert
            Assert.That(error.Exception, Is.Null);
        }
    }
}
