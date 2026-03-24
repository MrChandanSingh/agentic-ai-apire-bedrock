using AspireApp.BedRock.SonetOps.ApiService.Middleware;
using Microsoft.AspNetCore.Http;

namespace AspireApp.BedRock.SonetOps.Tests;

[TestClass]
public class SecurityHeadersMiddlewareTests
{
    [TestMethod]
    public async Task SecurityHeaders_AreAddedToResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(next: (innerHttpContext) =>
        {
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.IsTrue(context.Response.Headers.ContainsKey("X-Content-Type-Options"));
        Assert.AreEqual("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        
        Assert.IsTrue(context.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.AreEqual("DENY", context.Response.Headers["X-Frame-Options"]);
        
        Assert.IsTrue(context.Response.Headers.ContainsKey("X-XSS-Protection"));
        Assert.AreEqual("1; mode=block", context.Response.Headers["X-XSS-Protection"]);
        
        Assert.IsTrue(context.Response.Headers.ContainsKey("Content-Security-Policy"));
    }

    [TestMethod]
    public async Task NextDelegate_IsInvoked()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var delegateInvoked = false;
        
        var middleware = new SecurityHeadersMiddleware(next: (innerHttpContext) =>
        {
            delegateInvoked = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.IsTrue(delegateInvoked);
    }
}