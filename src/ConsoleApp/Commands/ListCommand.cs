using CommandLine;

namespace ConsoleApp.Commands;

[Verb("list")]
public sealed class ListCommand : ICommand
{
    [Option("album", Default = null)]
    public string? Album { get; set; }
}