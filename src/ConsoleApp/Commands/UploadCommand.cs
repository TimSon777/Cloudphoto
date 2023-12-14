using CommandLine;

namespace ConsoleApp.Commands;

[Verb("upload", HelpText = "Command to send photos to cloud")]
public sealed class UploadCommand : ICommand
{
    [Option("album", Required = true, HelpText = "Directory to save photos")]
    public string Album { get; set; } = default!;

    private string _path = default!;

    [Option("path", Default = "")]
    public string Path
    {
        get => _path;
        set => _path = value.Trim('\"').Trim('\'');
    }
}