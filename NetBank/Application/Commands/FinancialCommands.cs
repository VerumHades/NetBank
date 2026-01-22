using NetBank.Types;

namespace NetBank.Application.Commands;


public class DepositCommand(IAccountService service, AccountIdentifier account, Amount amount) : ICommand
{
    public async Task<string> ExecuteAsync()
    {
        await service.Deposit(account, amount);
        return "AD";
    }
}

public class WithdrawCommand(IAccountService service, AccountIdentifier account, Amount amount) : ICommand
{
    public async Task<string> ExecuteAsync()
    {
        await service.Withdraw(account, amount);
        return "AW";
    }
}

public class GetBalanceCommand(IAccountService service, AccountIdentifier account) : ICommand
{
    public async Task<string> ExecuteAsync()
    {
        var balance = await service.Balance(account);
        return $"AB {balance}";
    }
}