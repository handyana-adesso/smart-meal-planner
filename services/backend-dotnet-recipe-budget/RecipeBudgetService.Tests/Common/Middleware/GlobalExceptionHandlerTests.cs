using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBudgetService.Common.Exceptions;
using RecipeBudgetService.Common.Middleware;

namespace RecipeBudgetService.Tests.Common.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock = new();
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _handler = new(_loggerMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_WhenNotFoundException_ShouldReturn404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new NotFoundException("Resource not found.");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        
        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task TryHandleAsync_WhenUnhandledException_ShouldReturn500()
    {         
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Unexpected error.");
        
        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
