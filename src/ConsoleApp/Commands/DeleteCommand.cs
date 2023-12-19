using CommandLine;

namespace ConsoleApp.Commands;

[Verb("delete")]
public sealed class DeleteCommand : ICommand
{
    [Option("album", Required = true)]
    public string Album { get; set; } = default!;
    
    [Option("photo", Required = false, Default = null)]
    public string? Photo { get; set; }
}