using System.CommandLine;
using Adapters.Database;
using Adapters.Database.Seeders;

namespace CLI.Commands;

/// <summary>
/// Database management commands
/// </summary>
public static class DatabaseCommands
{
    /// <summary>
    /// Nuke and pave command - drops database, recreates schema, and seeds data
    /// </summary>
    public static Command NukeAndPave()
    {


        var command = new Command("nukeAndPave", "Drop database, recreate schema, and seed data");

        command.SetHandler(async () =>
        {
            try
            {
                await using var context = ApplicationDbContextFactory.Create();

                Console.WriteLine();
                await SeederRunner.NukeAndPaveAsync(context);
                Console.WriteLine();
                Console.WriteLine("✅ Database nuked and paved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return command;
    }
}

