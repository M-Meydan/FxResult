using NUnit.Framework;
using FxResults.Core;
using FxResults.Extensions;
using System;
using System.Threading.Tasks;
using FxResults.Extensions.FailIf;


namespace FxResults.Tests
{
    [TestFixture]
    public class IntegrationAndComplexTests
    {
        [Test]
        public void ComplexChaining_SuccessPath_ExecutesAllOperationsCorrectly()
        {
            // Arrange
            var initialValue = 5;
            var operations = new List<string>();
            
            // Act
            var result = Result.Success(initialValue)
                .Then(v => 
                {
                    operations.Add("First transformation");
                    return v * 2;
                })
                .Tap(v => 
                {
                    operations.Add("Tap after first transformation");
                    Assert.That(v, Is.EqualTo(initialValue * 2));
                })
                .Then(v => 
                {
                    operations.Add("Second transformation");
                    return v.ToString();
                })
                .OnSuccess(v => 
                {
                    operations.Add("OnSuccess after second transformation");
                    Assert.That(v, Is.EqualTo((initialValue * 2).ToString()));
                });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo((initialValue * 2).ToString()));
                Assert.That(operations, Has.Count.EqualTo(4));
                Assert.That(operations[0], Is.EqualTo("First transformation"));
                Assert.That(operations[1], Is.EqualTo("Tap after first transformation"));
                Assert.That(operations[2], Is.EqualTo("Second transformation"));
                Assert.That(operations[3], Is.EqualTo("OnSuccess after second transformation"));
            });
        }
        
        [Test]
        public void ComplexChaining_FailureInMiddle_StopsChainAndPropagatesError()
        {
            // Arrange
            var initialValue = 5;
            var operations = new List<string>();
            var errorMessage = "Value cannot be even";
            
            // Act
            var result = Result.Success(initialValue)
                .Then(v => 
                {
                    operations.Add("First transformation");
                    return v * 2;
                })
                .FailIf(v => v % 2 == 0, "EVEN_NUMBER", errorMessage)
                .Then(v => 
                {
                    operations.Add("Second transformation - should not execute");
                    return v.ToString();
                })
                .OnFailure(e => 
                {
                    operations.Add("OnFailure handler");
                    Assert.That(e.Message, Is.EqualTo(errorMessage));
                });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Message, Is.EqualTo(errorMessage));
                Assert.That(operations, Has.Count.EqualTo(2));
                Assert.That(operations[0], Is.EqualTo("First transformation"));
                Assert.That(operations[1], Is.EqualTo("OnFailure handler"));
            });
        }
        
        [Test]
        public async Task ComplexChaining_MixedSyncAndAsync_ExecutesCorrectly()
        {
            // Arrange
            var initialValue = 5;
            var operations = new List<string>();
            
            // Act
            var result = await Result.Success(initialValue)
                .Then(v => 
                {
                    operations.Add("Sync transformation");
                    return v * 2;
                })
                .ThenAsync(async v => 
                {
                    await Task.Delay(10); // Simulate async work
                    operations.Add("Async transformation");
                    return v + 3;
                })
                .TapAsync(async v => 
                {
                    await Task.Delay(10); // Simulate async work
                    operations.Add("Async tap");
                    Assert.That(v, Is.EqualTo((initialValue * 2) + 3));
                });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo((initialValue * 2) + 3));
                Assert.That(operations, Has.Count.EqualTo(3));
                Assert.That(operations[0], Is.EqualTo("Sync transformation"));
                Assert.That(operations[1], Is.EqualTo("Async transformation"));
                Assert.That(operations[2], Is.EqualTo("Async tap"));
            });
        }

        [Test]
        public void RealWorldScenario_UserRegistration_HandlesSuccessPath()
        {
            // Arrange - Simulate a user registration flow
            var userData = new { Email = "test@example.com", Password = "Password123!" };
            var registeredUser = new { Id = Guid.NewGuid(), Email = userData.Email };
            var operations = new List<string>();
            
            // Act - Simulate the registration process with validation, creation, and notification
            var result = Result.Success(userData)
                // Validate input
                .Then(data => 
                {
                    operations.Add("Validating user data");
                    return data;
                })
                .FailIf(data => string.IsNullOrEmpty(data.Email), "INVALID_EMAIL", "Email is required")
                .FailIf(data => data.Password.Length < 8, "INVALID_PASSWORD", "Password must be at least 8 characters")
                // Check if user exists
                .Then(data => 
                {
                    operations.Add("Checking if user exists");
                    // Simulate database check - user doesn't exist
                    return data;
                })
                // Create user
                .Then(data => 
                {
                    operations.Add("Creating user record");
                    // Simulate user creation
                    return registeredUser;
                })
                // Send confirmation email
                .Tap(user => 
                {
                    operations.Add("Sending confirmation email");
                    // Simulate sending email
                });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(registeredUser));
                Assert.That(operations, Has.Count.EqualTo(4));
                Assert.That(operations[0], Is.EqualTo("Validating user data"));
                Assert.That(operations[1], Is.EqualTo("Checking if user exists"));
                Assert.That(operations[2], Is.EqualTo("Creating user record"));
                Assert.That(operations[3], Is.EqualTo("Sending confirmation email"));
            });
        }
        
        [Test]
        public void RealWorldScenario_UserRegistration_HandlesValidationFailure()
        {
            // Arrange - Simulate a user registration flow with invalid data
            var userData = new { Email = "", Password = "123" };
            var operations = new List<string>();
            
            // Act - Simulate the registration process with validation failure
            var result = Result.Success(userData)
                // Validate input
                .Then(data => 
                {
                    operations.Add("Validating user data");
                    return data;
                })
                .FailIf(data => string.IsNullOrEmpty(data.Email), "INVALID_EMAIL", "Email is required")
                .FailIf(data => data.Password.Length < 8, "INVALID_PASSWORD", "Password must be at least 8 characters")
                // These steps should not execute due to validation failure
                .Then(data => 
                {
                    operations.Add("Checking if user exists - should not execute");
                    return data;
                })
                .Then(data => 
                {
                    operations.Add("Creating user record - should not execute");
                    return new { Id = Guid.NewGuid(), Email = data.Email };
                })
                .OnFailure(error => 
                {
                    operations.Add($"Handling error: {error.Code}");
                });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error!.Code, Is.EqualTo("INVALID_EMAIL"));
                Assert.That(result.Error!.Message, Is.EqualTo("Email is required"));
                Assert.That(operations, Has.Count.EqualTo(2));
                Assert.That(operations[0], Is.EqualTo("Validating user data"));
                Assert.That(operations[1], Is.EqualTo("Handling error: INVALID_EMAIL"));
            });
        }
        
        [Test]
        public void EdgeCase_NestedResults_UnwrapsCorrectly()
        {
            // Arrange
            var initialValue = 5;
            
            // Act - Create a Result<Result<int>> and then unwrap it
            var nestedResult = Result.Success(Result.Success(initialValue));
            
            // Unwrap the nested result
            var unwrappedResult = nestedResult.Then(innerResult => innerResult);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(unwrappedResult.IsSuccess, Is.True);
                Assert.That(unwrappedResult.Value, Is.EqualTo(initialValue));
            });
        }
        
        [Test]
        public void EdgeCase_NestedResults_PropagatesInnerFailure()
        {
            // Arrange
            var error = new Error("Inner error", "INNER_ERROR");
            
            // Act - Create a Result<Result<int>> with inner failure and then unwrap it
            var nestedResult = Result.Success(Result.Fail<int>(error));
            
            // Unwrap the nested result
            var unwrappedResult = nestedResult.Then(innerResult => innerResult);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(unwrappedResult.IsSuccess, Is.False);
                Assert.That(unwrappedResult.Error!.Code, Is.EqualTo(error.Code));
                Assert.That(unwrappedResult.Error!.Message, Is.EqualTo(error.Message));
            });
        }
        
        [Test]
        public void EdgeCase_NestedResults_PropagatesOuterFailure()
        {
            // Arrange
            var error = new Error("Outer error", "OUTER_ERROR");
            
            // Act - Create a failed Result<Result<int>> and then try to unwrap it
            var nestedResult = Result.Fail<Result<int>>(error);
            
            // Try to unwrap the nested result
            var unwrappedResult = nestedResult.Then(innerResult => innerResult);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(unwrappedResult.IsSuccess, Is.False);
                Assert.That(unwrappedResult.Error!.Code, Is.EqualTo(error.Code));
                Assert.That(unwrappedResult.Error!.Message, Is.EqualTo(error.Message));
            });
        }
        
        [Test]
        public async Task EdgeCase_Concurrency_HandlesMultipleAsyncOperations()
        {
            // Arrange
            var values = new[] { 1, 2, 3, 4, 5 };
            var results = values.Select(v => Result.Success(v)).ToArray();
            
            // Act - Process multiple results concurrently
            var tasks = results.Select(async r => 
                await r.ThenAsync(async v => 
                {
                    await Task.Delay(10); // Simulate async work
                    return v * 2;
                })
            ).ToArray();
            
            var processedResults = await Task.WhenAll(tasks);
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(processedResults, Has.Length.EqualTo(values.Length));
                for (int i = 0; i < values.Length; i++)
                {
                    Assert.That(processedResults[i].IsSuccess, Is.True);
                    Assert.That(processedResults[i].Value, Is.EqualTo(values[i] * 2));
                }
            });
        }
        
        [Test]
        public void ComplexScenario_ConditionalProcessing_ExecutesCorrectBranch()
        {
            // Arrange
            var value = 42;
            var operations = new List<string>();
            
            // Act - Simulate a complex conditional processing chain
            var result = Result.Success(value)
                .Then(v => 
                {
                    if (v % 2 == 0)
                    {
                        operations.Add("Processing even number");
                        return Result.Success(v / 2);
                    }
                    else
                    {
                        operations.Add("Processing odd number");
                        return Result.Success(v * 3 + 1);
                    }
                })
                .Then(v => 
                {
                    if (v < 30)
                    {
                        operations.Add("Value is small");
                    }
                    else
                    {
                        operations.Add("Value is large");
                    }
                    return v;
                });
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(value / 2));
                Assert.That(operations, Has.Count.EqualTo(2));
                Assert.That(operations[0], Is.EqualTo("Processing even number"));
                Assert.That(operations[1], Is.EqualTo("Value is small"));
            });
        }
    }
}
