using CommandLine;

namespace ConsoleApp.Commands;

[Verb("list", HelpText = "Displays a list of photos if the `album` option is specified, and displays a list of albums if not specified")]
public sealed class ListCommand : ICommand
{
    [Option("album",
        Required = false,
        Default = null,
        HelpText = "Cloud album that stores photos")]
    public string? Album { get; set; }
}