using NetBank.Application.Commands;

namespace NetBank.Application;

public class SimpleOrchestrator: IOrchestrator
{
    private readonly ICommandParser _commandParser;
    private readonly ICommandFactory _commandFactory;

    public SimpleOrchestrator(ICommandParser commandParser, ICommandFactory commandFactory)
    {
        _commandParser = commandParser;
        _commandFactory = commandFactory;
    }

    public async Task<string> ExecuteTextCommand(string commandString)
    {
        try
        {
            var dto = _commandParser.ParseCommandToDTO(commandString);
            var command = _commandFactory.Create(dto);
            return await command.ExecuteAsync();
        }
        catch (Exception ex)
        {
            return  $"ER {ex.Message}";
        }
    }
}