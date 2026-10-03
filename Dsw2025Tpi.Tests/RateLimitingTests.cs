using System.Net;
using System.Net.Http.Json;
using Dsw2025Tpi.Api.Configurations;
using Dsw2025Tpi.Api.Errors;
using Dsw2025Tpi.Shared.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dsw2025Tpi.Tests;

// Cada test levanta su propio host: los contadores del rate limiter viven en
// memoria y no deben compartirse entre pruebas.
public sealed class RateLimitingTests
{
    private const int AuthLimit = 2;
    private const int GlobalLimit = 5;

    private static async Task<(WebApplication App, HttpClient Client)> StartAsync(int globalLimit = GlobalLimit)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RateLimiting:Global:PermitLimit"] = globalLimit.ToString(),
            ["RateLimiting:Global:WindowSeconds"] = "60",
            ["RateLimiting:Auth:PermitLimit"] = AuthLimit.ToString(),
            ["RateLimiting:Auth:WindowSeconds"] = "60",
        });
        builder.Services.AddApiRateLimiting(builder.Configuration);

        var app = builder.Build();
        app.UseRateLimiter();
        app.MapPost("/auth", () => "ok").RequireRateLimiting(RateLimitPolicies.Auth);
        app.MapGet("/open", () => "ok");
        await app.StartAsync();

        return (app, new HttpClient { BaseAddress = new Uri(app.Urls.Single()) });
    }

    [Fact]
    public async Task Auth_policy_rejects_the_request_after_the_limit_with_429_and_the_error_contract()
    {
        var (app, client) = await StartAsync();
        await using var _ = app;
        using var __ = client;

        for (var i = 0; i < AuthLimit; i++)
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/auth", null)).StatusCode);

        var rejected = await client.PostAsync("/auth", null);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.True(rejected.Headers.RetryAfter?.Delta > TimeSpan.Zero, "Debe indicar cuándo reintentar");

        var error = await rejected.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(RateLimitingExtensions.ErrorCode, error!.Code);
        Assert.Equal(429, error.Status);
        Assert.Equal(ErrorMessages.Get(RateLimitingExtensions.ErrorCode), error.Message);
        Assert.DoesNotContain("Unknown error code", error.Message);
    }

    [Fact]
    public async Task Global_limit_applies_to_endpoints_without_a_policy()
    {
        var (app, client) = await StartAsync();
        await using var _ = app;
        using var __ = client;

        for (var i = 0; i < GlobalLimit; i++)
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/open")).StatusCode);

        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.GetAsync("/open")).StatusCode);
    }

    // Las requests a /auth tambien consumen del limite global (incluso las que la
    // politica rechaza). Con un global holgado se aisla lo que se prueba aca:
    // la cuota de "auth" es aparte y agotarla no bloquea el resto de la API.
    [Fact]
    public async Task Exhausting_the_auth_policy_does_not_block_the_rest_of_the_api()
    {
        var (app, client) = await StartAsync(globalLimit: 100);
        await using var _ = app;
        using var __ = client;

        for (var i = 0; i <= AuthLimit; i++)
            await client.PostAsync("/auth", null);

        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsync("/auth", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/open")).StatusCode);
    }
}
