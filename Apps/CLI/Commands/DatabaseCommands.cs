using System.CommandLine;
using Adapters.Database;
using Adapters.Database.Seeders;
using Microsoft.Extensions.Logging;

namespace CLI.Commands;

/// <summary>
/// Database management commands
/// </summary>
public static class DatabaseCommands
{
    /// <summary>
    /// Nuke and pave command - drops database, recreates schema, and seeds data
    /// </summary>
    public static Command NukeAndPave(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("NukeAndPave");
        var command = new Command("nukeAndPave", "Drop database, recreate schema, and seed data");

        command.SetHandler(async () =>
        {
            try
            {
                await using var context = ApplicationDbContextFactory.Create();

                logger.LogInformation("Starting database nuke and pave operation");
                await SeederRunner.NukeAndPaveAsync(context, loggerFactory);
                logger.LogInformation("Database nuked and paved successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to nuke and pave database");
                Environment.Exit(1);
            }
        });

        return command;
    }
}

