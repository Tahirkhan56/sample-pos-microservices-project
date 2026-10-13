using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pos.BuildingBlocks.Web.Errors;
using Scalar.AspNetCore;

namespace Pos.BuildingBlocks.Web;

/// <summary>Conventions shared by every HTTP service so they behave identically behind the gateway and in Kubernetes.</summary>
public static class ServiceDefaults
{
    public const string LivenessPath = "/health/live";
    public const string ReadinessPath = "/health/ready";
    private const string ReadyTag = "ready";

    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services.AddOpenApi();
        builder.Services.AddHealthChecks();

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        // Liveness: the process is up. Readiness: dependencies (DB, broker) are reachable.
        app.MapHealthChecks(LivenessPath, new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks(ReadinessPath, new HealthCheckOptions { Predicate = check => check.Tags.Contains(ReadyTag) });

        app.MapGet("/", () => Results.Redirect(app.Environment.IsDevelopment() ? "/scalar" : LivenessPath))
            .ExcludeFromDescription();

        return app;
    }
}
