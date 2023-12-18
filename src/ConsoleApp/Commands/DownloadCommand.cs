using CommandLine;

namespace ConsoleApp.Commands;

[Verb("download")]
public sealed class DownloadCommand : ICommand
{
    [Option("album")]
    public string Album { get; set; } = default!;

    private string _path = default!;

    [Option("path", Default = "")]
    public string Path
    {
        get => _path;
        set => _path = value.Trim('\"').Trim('\'');
    }

    [Option('v', "verbose", Default = false)]
    public bool Verbose { get; set; }
}