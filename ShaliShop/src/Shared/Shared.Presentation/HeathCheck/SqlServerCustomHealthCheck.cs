// using Microsoft.Data.SqlClient;
// using Microsoft.Extensions.Diagnostics.HealthChecks;
//
// namespace Shared.Presentation.HeathCheck;
//
// public class SqlServerCustomHealthCheck(string connectionString) : IHealthCheck
// {
//     public async Task<HealthCheckResult> CheckHealthAsync(
//         HealthCheckContext context,
//         CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             await using var connection = new SqlConnection(connectionString);
//             await connection.OpenAsync(cancellationToken);
//             return HealthCheckResult.Healthy("SQL Server is reachable.");
//         }
//         catch (Exception ex)
//         {
//             return HealthCheckResult.Unhealthy($"SQL Server connection failed: {ex.Message}", ex);
//         }
//     }
// }