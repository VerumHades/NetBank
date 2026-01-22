namespace NetBank.Application.Commands;

public class GetBankTotalCommand(IAccountService service) : ICommand
{
    public async Task<string> ExecuteAsync()
    {
        var total = await service.BankTotal();
        return $"BA {total}";
    }
}

public class GetClientCountCommand(IAccountService service) : ICommand
{
    public async Task<string> ExecuteAsync()
    {
        var count = await service.BankNumberOfClients();
        return $"BN {count}";
    }
}