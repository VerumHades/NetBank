using NetBank.Infrastructure.Configuration;
using NetBank.Types;

namespace NetBank.Application.Commands;

public class GetBankCodeCommand(IAccountService service, Configuration config) : ICommand
{
    public Task<string> ExecuteAsync() => Task.FromResult($"BC {config.ServerIp}");
}

public class CreateAccountCommand(IAccountService service, Configuration config) : ICommand
{
    public async Task<string> ExecuteAsync()
    {
        var id = await service.CreateAccount();
        return $"AC {id}/{config.ServerIp}";
    }
}

public class RemoveAccountCommand(IAccountService service, AccountIdentifier account) : ICommand
{
    public async Task<string> ExecuteAsync()
    {
        await service.RemoveAccount(account);
        return "AR";
    }
}