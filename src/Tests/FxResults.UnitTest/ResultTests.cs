using FxResults.Core;

namespace FxResults.UnitTest
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

        [TestCase("Test error", "FAIL", "TestSource")]
        public void Constructor_Failure_SetsProperties(string message, string code, string source)
        {
            var error = new Error(message, code, source);
            var result = new Result<string>(error);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(error));
            Assert.Throws<InvalidOperationException>(() => _ = result.Value);
        }


        [Test]
        public void Fail_WithMessage_CreatesFailureResult()
        {
            // Arrange
            var message = "Test error message";
            
            // Act
            var result = Result<int>.Fail(message);
            
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
        public void TryGetValue_OnSuccessResult_ReturnsTrueAndValue()
        {
            // Arrange
            var value = "test value";
            var result = Result<string>.Success(value);
            
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
            var result = Result<string>.Fail(error);
            
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
        public void Success_WithDefaultValue_CreatesSuccessResultWithDefault()
        {
            // Arrange & Act
            var result = Result<int>.Success(default);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(default(int)));
            });
        }

        [Test]
        public void Success_WithNullValue_TryGetValueAlsoReturnsTrueAndNull()
        {
            var result = Result<string>.Success(null!);

            var success = result.TryGetValue(out var value);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.Null);
                Assert.That(success, Is.True);
                Assert.That(value, Is.Null);
            });
        }

        [Test]
        public void Try_ReturnsSuccess_WhenFuncSucceeds()
        {
            var result = Result<string>.Try(() => "hello");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo("hello"));
        }

        [Test]
        public void Try_ReturnsFailure_WhenFuncThrows()
        {
            var result = Result<string>.Try(() => throw new InvalidOperationException("bad op"));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error!.Message, Is.EqualTo("bad op"));
            Assert.That(result.Error.Code, Is.EqualTo("InvalidOperationException"));
        }

        [Test]
        public async Task TryAsync_ReturnsSuccess_WhenFuncSucceeds()
        {
            var result = await Result<string>.TryAsync(() => Task.FromResult("async"));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo("async"));
        }

        [Test]
        public async Task TryAsync_ReturnsFailure_WhenFuncThrows()
        {
            var result = await Result<string>.TryAsync(() => throw new Exception("boom"));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error!.Message, Is.EqualTo("boom"));
            Assert.That(result.Error.Code, Is.EqualTo("Exception"));
        }

        [Test]
        public void Try_Action_ReturnsSuccess_WhenNoException()
        {
            var result = Result<Unit>.Try(() => Unit.Value);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(Unit.Value));
        }

        [Test]
        public void Try_Action_ReturnsFailure_WhenExceptionThrown()
        {
            var result = Result<Unit>.Try(() => throw new ApplicationException("fail"));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error!.Message, Is.EqualTo("fail"));
        }

        [Test]
        public async Task TryAsync_Action_ReturnsSuccess_WhenNoException()
        {
            // Fix: Adjust the lambda expression to return the correct type
            var result = await Result<Unit>.TryAsync(async () =>
            {
                await Task.Delay(1); // Simulate async operation
                return Unit.Value; // Return Unit.Value directly
            });

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(Unit.Value));
        }

        [Test]
        public async Task TryAsync_Action_ReturnsFailure_WhenExceptionThrown()
        {
            var result = await Result<Unit>.TryAsync(() => throw new Exception("error"));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error!.Message, Is.EqualTo("error"));
        }

        [Test]
        public void Try_WithInvalidOperationException_ReturnsFailureWithExceptionDetails()
        {
            // Arrange
            var exceptionMessage = "Invalid operation occurred";
            
            // Act
            var result = Result<string>.Try(() => throw new InvalidOperationException(exceptionMessage));
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.Error!.Message, Is.EqualTo(exceptionMessage));
                Assert.That(result.Error.Code, Is.EqualTo("InvalidOperationException"));
                Assert.Throws<InvalidOperationException>(() => _ = result.Value);
            });
        }

        [Test]
        public void Try_WithArgumentException_ReturnsFailureWithExceptionDetails()
        {
            // Arrange
            var paramName = "testParam";
            var exceptionMessage = "Invalid argument provided";
            
            // Act
            var result = Result<string>.Try(() => throw new ArgumentException(exceptionMessage, paramName));
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.Error!.Message, Does.Contain(exceptionMessage));
                Assert.That(result.Error.Code, Is.EqualTo("ArgumentException"));
            });
        }

        [Test]
        public async Task TryAsync_WithAggregateException_ReturnsFailureWithInnerExceptionDetails()
        {
            // Arrange
            var innerExceptionMessage = "Inner exception occurred";
            
            // Act
            var result = await Result<string>.TryAsync(async () =>
            {
                await Task.Delay(1);
                throw new AggregateException(new InvalidOperationException(innerExceptionMessage));
            });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.Error!.Message, Is.EqualTo($"One or more errors occurred. ({innerExceptionMessage})"));

                Assert.That(result.Error.Code, Is.EqualTo("AggregateException"));
            });
        }
    }
}
