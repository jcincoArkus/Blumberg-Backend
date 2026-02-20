using System.CommandLine;
using Adapters.Config;
using Adapters.Database;
using Adapters.Logger;
using Adapters.Permissions;
using CLI.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

var config = ConfigLoader.Load();

var logger = LoggerSetup.CreateCliLogger(config.Log);

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSerilog(logger);
});

// Create service provider for permission commands
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddSerilog(logger));
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(config.Database.GetConnectionString()));
services.AddCasbinAuthorization();
var serviceProvider = services.BuildServiceProvider();

var rootCommand = new RootCommand("Blumberg CLI Management tools");

// 5. Add migration commands
rootCommand.AddCommand(MigrationCommands.MigrationGenerate(loggerFactory));
rootCommand.AddCommand(MigrationCommands.MigrationUp(loggerFactory));
rootCommand.AddCommand(MigrationCommands.MigrationDown(loggerFactory));
rootCommand.AddCommand(MigrationCommands.MigrationStatus(loggerFactory));

// 6. Add database commands
rootCommand.AddCommand(DatabaseCommands.NukeAndPave(loggerFactory));

// 7. Add OpenAPI commands
rootCommand.AddCommand(OpenApiCommands.GenerateOpenApi(loggerFactory));

// 8. Add Permission commands
rootCommand.AddCommand(PermissionCommands.GeneratePermissions(loggerFactory));
rootCommand.AddCommand(PermissionCommands.InitializePermissions(serviceProvider, loggerFactory));
rootCommand.AddCommand(PermissionCommands.VerifyPermissions(serviceProvider, loggerFactory));

// 9. Add Ingestion simulator (dev)
rootCommand.AddCommand(IngestionCommands.Simulate(loggerFactory, config));

// 10. Run
try { return await rootCommand.InvokeAsync(args); }
finally
{
    await serviceProvider.DisposeAsync();
    await Log.CloseAndFlushAsync();
}
