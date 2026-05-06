using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.Notifications;
using System.Text.RegularExpressions;

namespace Explore.MessageCenterService.Infrastructure.Notifications;

public sealed partial class SimpleTemplateRenderer : ITemplateRenderer, IScopeDependency
{
    public Result<RenderedTemplateResult> Render(
        string? titleTemplate,
        string bodyTemplate,
        IReadOnlyDictionary<string, string> parameters)
    {
        var titleResult = RenderText(titleTemplate, parameters, "TitleTemplate");
        if (titleResult.IsFailure)
        {
            return Result.Failure<RenderedTemplateResult>(titleResult.Error);
        }

        var bodyResult = RenderText(bodyTemplate, parameters, "BodyTemplate");
        if (bodyResult.IsFailure)
        {
            return Result.Failure<RenderedTemplateResult>(bodyResult.Error);
        }

        return Result.Success(new RenderedTemplateResult(titleResult.Value, bodyResult.Value!));
    }

    private static Result<string?> RenderText(
        string? template,
        IReadOnlyDictionary<string, string> parameters,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return Result.Success<string?>(template);
        }

        var missingVariables = PlaceholderRegex()
            .Matches(template)
            .Select(match => match.Groups["key"].Value)
            .Where(key => !parameters.ContainsKey(key))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingVariables.Length > 0)
        {
            return Result.Failure<string?>(Error.Validation($"{fieldName} is missing parameters: {string.Join(", ", missingVariables)}."));
        }

        var renderedText = PlaceholderRegex().Replace(template, match =>
        {
            var key = match.Groups["key"].Value;
            return parameters.TryGetValue(key, out var value) ? value : match.Value;
        });

        return Result.Success<string?>(renderedText);
    }

    [GeneratedRegex("\\{\\{\\s*(?<key>[a-zA-Z0-9_.-]+)\\s*\\}\\}")]
    private static partial Regex PlaceholderRegex();
}

