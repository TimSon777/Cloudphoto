using CommandLine;

namespace ConsoleApp.Commands;

[Verb("delete", HelpText = "Command to delete photos or albums from the cloud")]
public sealed class DeleteCommand : ICommand
{
    [Option("album",
        Required = true,
        HelpText = "Cloud album that stores photos")]
    public string Album { get; set; } = default!;

    [Option("photo",
        Required = false,
        Default = null,
        HelpText = "Photo to be deleted. If not specified, the album specified by the `album` option will be deleted")]
    public string? Photo { get; set; }
}
