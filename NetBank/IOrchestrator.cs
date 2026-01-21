namespace NetBank;

public interface IOrchestrator
{
    Task<string> ExecuteTextCommand(string command);
}