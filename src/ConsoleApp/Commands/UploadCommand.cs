using CommandLine;

namespace ConsoleApp.Commands;

[Verb("upload", HelpText = "Command to send photos to cloud")]
public sealed class UploadCommand : ICommand
{
    [Option("album",
        Required = true,
        HelpText = "Cloud Album to save photos")]
    public string Album { get; set; } = default!;

    private string _path = default!;

    [Option("path",
        Required = false,
        Default = "",
        HelpText = "If set, then all photos will be saved into it. Otherwise the current directory will be used")]
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