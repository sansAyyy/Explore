using BuildingBlocks.Correlation.AspNetCore.Extensions;
using BuildingBlocks.DependencyInjection.Conventional.Extensions;
using BuildingBlocks.Logging.Serilog.Extensions;
using BuildingBlocks.OpenApi.AspNetCore.Extensions;
using BuildingBlocks.Security.Authorization.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogLogging(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddServiceOpenApi(options =>
{
    options.Title = "Message Center Service API";
    options.Description = "Notification template, dispatch and site message APIs.";
    options.UseBearerSecurity = true;
    options.BearerDescription = "JWT Bearer token for protected message center APIs.";
});
builder.Services.AddModules(builder.Configuration);
builder.Services.AddTokenPartyAuthorizationPolicies();
builder.Services.AddCorrelation();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCorrelation();

app.MapServiceOpenApi();

app.MapHealthChecks("/health");


app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


