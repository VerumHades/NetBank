namespace NetBank.Application.Commands;

public interface ICommand
{
    Task<string> ExecuteAsync();
}