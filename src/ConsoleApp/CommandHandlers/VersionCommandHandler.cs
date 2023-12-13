using ConsoleApp.Commands;

namespace ConsoleApp.CommandHandlers;

public sealed class VersionCommandHandler : ICommandHandler<VersionCommand>
{
    public async Task HandleAsync(VersionCommand command, CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync("Version: 1.0.0");
    }
}