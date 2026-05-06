using BuildingBlocks.Correlation.AspNetCore.Extensions;
using BuildingBlocks.Logging.Serilog.Extensions;
using BuildingBlocks.DependencyInjection.Conventional.Extensions;
using BuildingBlocks.OpenApi.AspNetCore.Extensions;
using BuildingBlocks.Security.Authorization.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogLogging(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddServiceOpenApi(options =>
{
    options.Title = "Admin Identity Service API";
    options.Description = "Admin authentication, authorization, role and permission APIs.";
    options.UseBearerSecurity = true;
    options.BearerDescription = "JWT Bearer token for protected admin APIs.";
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


