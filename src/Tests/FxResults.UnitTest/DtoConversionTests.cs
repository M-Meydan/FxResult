using NUnit.Framework;
using FxResults.Api.Responses;
using FxResults.Core;
using FxResults.Extensions;
using System.Collections.Generic;

namespace FxResults.Tests
{
    [TestFixture]
    public class DtoConversionTests
    {
        [Test]
        public void ToPublicDto_WithError_ConvertsCorrectly()
        {
            // Arrange
            var error = new Error("Test error message", "TEST_ERROR", "TestSource");
            
            // Act
            var publicError = error.ToPublicDto();
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(publicError.Code, Is.EqualTo(error.Code));
                Assert.That(publicError.Message, Is.EqualTo(error.Message));
                Assert.That(publicError.Details, Has.Count.EqualTo(1));
                Assert.That(publicError.Details[0].Code, Is.EqualTo(error.Code));
                Assert.That(publicError.Details[0].Message, Is.EqualTo(error.Message));
                Assert.That(publicError.Details[0].Source, Is.EqualTo(error.Source));
            });
        }
        
        [Test]
        public void ToPublicDto_WithNullError_ReturnsDefaultPublicErrorResponse()
        {
            // Arrange
            Error? error = null;
            
            // Act
            var publicError = error.ToPublicDto();
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(publicError.Code, Is.EqualTo("UNKNOWN"));
                Assert.That(publicError.Message, Is.EqualTo("An error occurred."));
                Assert.That(publicError.Details, Is.Empty);
            });
        }
        
        [Test]
        public void ToPublicDto_WithErrorHavingNullProperties_HandlesNullsCorrectly()
        {
            // Arrange
            var error = new Error(null!, null!, null);
            
            // Act
            var publicError = error.ToPublicDto();
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(publicError.Code, Is.Null);
                Assert.That(publicError.Message, Is.Null);
                Assert.That(publicError.Details, Has.Count.EqualTo(1));
                Assert.That(publicError.Details[0].Code, Is.Null);
                Assert.That(publicError.Details[0].Message, Is.Null);
                Assert.That(publicError.Details[0].Source, Is.Null);
            });
        }
        
        [Test]
        public void ToResponseDto_WithSuccessResult_CreatesCorrectResultResponse()
        {
            // Arrange
            var value = "test value";
            var result = Result.Success(value);
            
            // Act
            var response = result.ToResponseDto();
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.Data, Is.EqualTo(value));
                Assert.That(response.Error, Is.Null);
                Assert.That(response.Meta, Is.Null);
            });
        }
        
        [Test]
        public void ToResponseDto_WithFailureResult_CreatesCorrectResultResponse()
        {
            // Arrange
            var error = new Error("Test error", "TEST_ERROR", "TestSource");
            var result = Result.Fail<string>(error);
            
            // Act
            var response = result.ToResponseDto();
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.Data, Is.Null);
                Assert.That(response.Error, Is.Not.Null);
                Assert.That(response.Error!.Code, Is.EqualTo(error.Code));
                Assert.That(response.Error!.Message, Is.EqualTo(error.Message));
                Assert.That(response.Meta, Is.Null);
            });
        }
        
        [Test]
        public void ToResponseDto_WithMetadata_IncludesMetadataInResponse()
        {
            // Arrange
            var value = "test value";
            var result = Result.Success(value);
            var meta = new { Count = 5, Page = 1 };
            
            // Act
            var response = result.ToResponseDto(meta);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.Data, Is.EqualTo(value));
                Assert.That(response.Error, Is.Null);
                Assert.That(response.Meta, Is.EqualTo(meta));
            });
        }
        
        [Test]
        public void ResultResponse_FromSuccess_CreatesCorrectResponse()
        {
            // Arrange
            var data = "test data";
            var meta = new { Count = 5 };
            
            // Act
            var response = ResultResponse<string>.FromSuccess(data, meta);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.Data, Is.EqualTo(data));
                Assert.That(response.Error, Is.Null);
                Assert.That(response.Meta, Is.EqualTo(meta));
            });
        }
        
        [Test]
        public void ResultResponse_FromError_CreatesCorrectResponse()
        {
            // Arrange
            var error = new Error("Test error", "TEST_ERROR", "TestSource");
            
            // Act
            var response = ResultResponse<string>.FromError(error);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.Data, Is.Null);
                Assert.That(response.Error, Is.Not.Null);
                Assert.That(response.Error!.Code, Is.EqualTo(error.Code));
                Assert.That(response.Error!.Message, Is.EqualTo(error.Message));
                Assert.That(response.Meta, Is.Null);
            });
        }
    }
}
