// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Diagnostics.HealthChecks;
//
// namespace Shared.Presentation.HeathCheck;
//
// public static class HealthCheckExtensions
// {
//     public static IHealthChecksBuilder AddHealthChecksServices(this IServiceCollection services, string connectionString)
//     {
//         services.AddSingleton<IHealthCheck>(sp => new SqlServerCustomHealthCheck(connectionString));
//         
//         services.AddHealthChecksUI()
//             .AddInMemoryStorage();
//         return services
//             .AddHealthChecks()
//             //.AddCheck<SqlServerCustomHealthCheck>("SQL Server")
//             .AddSqlServer(connectionString, name: "SQL Server Connection");
//         ;
//     }
//
//     public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app, string path = "/healthcheck", string pageTitle = "Health check")
//     {
//         return app.UseHealthChecksUI(options =>
//         {
//             options.UIPath = "/health-ui";
//             options.PageTitle = pageTitle;
//         });
//     }
//
//     public static IEndpointRouteBuilder MapCustomHealthChecks(this IEndpointRouteBuilder app, string path = "/healthz")
//     {
//         app.MapGet(path, async (HealthCheckService service) =>
//         {
//             var report = await service.CheckHealthAsync();
//             return Results.Ok(new
//             {
//                 Status = report.Status.ToString(),
//                 Checks = report.Entries.Select(e => new
//                 {
//                     Name = e.Key,
//                     Status = e.Value.Status.ToString(),
//                     Description = e.Value.Description
//                 }),
//                 Duration = report.TotalDuration
//             });
//         });
//
//         return app;
//     }
// }