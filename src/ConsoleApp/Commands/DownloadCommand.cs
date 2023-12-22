using CommandLine;

namespace ConsoleApp.Commands;

[Verb("download", HelpText = "Command to upload photos from cloud")]
public sealed class DownloadCommand : ICommand
{
    [Option("album",
        Required = true,
        HelpText = "Cloud album that stores photos")]
    public string Album { get; set; } = default!;

    private string _path = default!;

    [Option("path",
        Required = false,
        Default = "",
        HelpText = "The folder from which photos will be sent to the cloud. If not specified, the current directory will be used")]
    public string Path
    {
        get => _path;
        set => _path = value.Trim('\"').Trim('\'');
    }

    [Option('v', "verbose",
        Required = false,
        Default = false,
        HelpText = "Process output")]
    public bool Verbose { get; set; }
}