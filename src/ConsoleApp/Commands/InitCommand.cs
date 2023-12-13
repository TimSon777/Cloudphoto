using CommandLine;

namespace ConsoleApp.Commands;

[Verb("init", HelpText = "Initialize Yandex Cloud Config")]
public sealed class InitCommand : ICommand;