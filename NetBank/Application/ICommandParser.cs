namespace NetBank.Application;

public interface ICommandParser
{
    object ParseCommandToDTO(string command);
}