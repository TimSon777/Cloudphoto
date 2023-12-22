using CommandLine;

namespace ConsoleApp.Commands;

[Verb("mksite", HelpText = "Provides a link to a photo album")]
public sealed class MakeSiteCommand : ICommand;