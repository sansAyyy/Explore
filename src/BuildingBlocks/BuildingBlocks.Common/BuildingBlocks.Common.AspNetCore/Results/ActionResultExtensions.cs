using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Common.AspNetCore.Results;

public static class ActionResultExtensions
{
    public static IActionResult ToActionResult(this ControllerBase controller, Result result, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess();
        }

        return controller.Problem(
            statusCode: MapStatusCode(result.Error.Code),
            title: result.Error.Message,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = result.Error.Code
            });
    }

    public static IActionResult ToActionResult<T>(
        this ControllerBase controller,
        Result<T> result,
        Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess is null
                ? controller.Ok(result.Value)
                : onSuccess(result.Value!);
        }

        return controller.Problem(
            statusCode: MapStatusCode(result.Error.Code),
            title: result.Error.Message,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = result.Error.Code
            });
    }

    private static int MapStatusCode(string errorCode)
    {
        return errorCode switch
        {
            ErrorCodes.BadRequest => StatusCodes.Status400BadRequest,
            ErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
            ErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
            ErrorCodes.NotFound => StatusCodes.Status404NotFound,
            ErrorCodes.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
    }
}

