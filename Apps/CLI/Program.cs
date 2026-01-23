using System.CommandLine;
using CLI.Commands;

// Root command
var rootCommand = new RootCommand("Blumberg CLI Management tools");

// Add migration commands
rootCommand.AddCommand(MigrationCommands.MigrationGenerate());
rootCommand.AddCommand(MigrationCommands.MigrationUp());
rootCommand.AddCommand(MigrationCommands.MigrationDown());
rootCommand.AddCommand(MigrationCommands.MigrationStatus());

// Run
return await rootCommand.InvokeAsync(args);
