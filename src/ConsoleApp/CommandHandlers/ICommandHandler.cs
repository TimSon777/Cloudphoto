using ConsoleApp.Commands;

namespace ConsoleApp.CommandHandlers;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    public Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}