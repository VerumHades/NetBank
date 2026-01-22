namespace NetBank.Application.Commands;

public interface ICommandFactory
{
    public ICommand Create(object commandRecord);
}