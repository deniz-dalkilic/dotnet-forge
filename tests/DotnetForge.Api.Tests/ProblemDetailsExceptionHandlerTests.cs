using DotnetForge.Api.Exceptions;
using DotnetForge.Api.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace DotnetForge.Api.Tests;

[TestClass]
public sealed class ProblemDetailsExceptionHandlerTests
{
    [TestMethod]
    public void ApplyExceptionResponse_SetsMappedStatusCodeForValidationException()
    {
        var context = new DefaultHttpContext();
        context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature
        {
            Error = new ApiValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["Name is required."]
            })
        });

        var statusCode = ProblemDetailsExceptionHandler.ApplyExceptionResponse(context);

        Assert.AreEqual(StatusCodes.Status400BadRequest, statusCode);
        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [TestMethod]
    public void ApplyExceptionResponse_ReturnsNullWhenNoExceptionFeatureExists()
    {
        var context = new DefaultHttpContext();

        var statusCode = ProblemDetailsExceptionHandler.ApplyExceptionResponse(context);

        Assert.IsNull(statusCode);
    }
}
