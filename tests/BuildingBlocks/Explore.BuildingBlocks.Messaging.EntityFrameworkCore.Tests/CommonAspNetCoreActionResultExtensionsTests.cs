using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class CommonAspNetCoreActionResultExtensionsTests
{
    [Fact]
    public void ToActionResult_ShouldMapFailureToProblemDetails()
    {
        var controller = CreateController();

        var result = controller.ToActionResult(
            Result.Failure(Error.Forbidden("Forbidden operation.")),
            controller.NoContent);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        Assert.Equal("Forbidden operation.", problemDetails.Title);
        Assert.Equal(ErrorCodes.Forbidden, problemDetails.Extensions["errorCode"]);
    }

    [Fact]
    public void ToActionResult_ShouldReturnOk_WhenGenericResultSucceeds()
    {
        var controller = CreateController();

        var result = controller.ToActionResult(Result.Success("payload"));

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("payload", okResult.Value);
    }

    private static ControllerBase CreateController()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();

        var provider = services.BuildServiceProvider();

        return new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = provider
                }
            }
        };
    }

    private sealed class TestController : ControllerBase
    {
    }
}

