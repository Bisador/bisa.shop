using CatalogService.Api;
using CatalogService.DependencyInjection;
using CheckoutService.DependencyInjection;
using InventoryService.DependencyInjection;
using Scalar.AspNetCore;
using Shared.Application.Behavior;
using Shared.Presentation.Cors;
using Shared.Presentation.ExceptionHandling; 
using Shared.Telemetry;
using WebApplication.Configurations;
using static Microsoft.AspNetCore.Builder.WebApplication;

var builder = CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
// Configure Services.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection not found");
builder.Services
    .AddCheckoutService(connectionString)
    .AddInventoryService(connectionString)
    .AddCatalogService(connectionString)
    .AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainExceptionPipelineBehavior<,>))
    .AddEndpointsApiExplorer()
    .AddCorsServices();
    // .AddApplicationInsightsTelemetry();
    // .AddHealthChecksServices(connectionString);

builder.Services.RegisterEventHandling(builder.Environment, "");

//
// builder.Logging.AddConsole();
// builder.Logging.AddDebug();
//
// var logger = builder.Logging.CreateLogger("Startup");
// logger.LogInformation("Environment: {env}", builder.Environment.EnvironmentName);
// logger.LogInformation("Services loaded: Catalog, Inventory, Checkout");

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Shop Service API");
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
} 
app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseCustomExceptionHandler();
}
app.UseCorrelationId();

app.MapCatalogEndpoints();
// app.UseCustomHealthChecks(pageTitle: "Shali Shop Health check.");
// app.MapCustomHealthChecks();
app.UseCustomCors();

app.Run();