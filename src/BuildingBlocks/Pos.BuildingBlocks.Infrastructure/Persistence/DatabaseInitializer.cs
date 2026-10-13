using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pos.BuildingBlocks.Infrastructure.Persistence;

public static partial class DatabaseInitializer
{
    private const int MaxAttempts = 20;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Creates the database schema if it does not exist, retrying while the database container starts.
    /// Production systems should replace this with EF Core migration bundles run as a Kubernetes Job.
    /// </summary>
    public static async Task InitializeDatabaseAsync<TDbContext>(
        this IServiceProvider services,
        Func<TDbContext, CancellationToken, Task>? seed = null,
        CancellationToken cancellationToken = default)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        await using AsyncServiceScope scope = services.CreateAsyncScope();
        TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                if (seed is not null)
                {
                    await seed(dbContext, cancellationToken);
                }

                LogInitialized(logger, typeof(TDbContext).Name);
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts && exception is not OperationCanceledException)
            {
                LogRetry(logger, exception, typeof(TDbContext).Name, attempt, MaxAttempts);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Database for {Context} is ready")]
    private static partial void LogInitialized(ILogger logger, string context);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Database for {Context} not ready (attempt {Attempt}/{MaxAttempts})")]
    private static partial void LogRetry(ILogger logger, Exception exception, string context, int attempt, int maxAttempts);
}
