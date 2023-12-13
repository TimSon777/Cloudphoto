using CommandLine;

namespace ConsoleApp.Commands;

[Verb("version", true, HelpText = "Displays the program version")]
public sealed class VersionCommand : ICommand;