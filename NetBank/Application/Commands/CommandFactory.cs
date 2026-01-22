using NetBank.Infrastructure.Configuration;
using NetBank.Infrastructure.Parsing;
using NetBank.Types;

namespace NetBank.Application.Commands;

public class CommandFactory(IProvider<IAccountService> serviceProvider, Configuration config): ICommandFactory
{
    public ICommand Create(object commandRecord)
    {
        var service = serviceProvider.Get();
        return commandRecord switch
        {
            BankCodeDto => new GetBankCodeCommand(service, config),
            CreateAccountDto => new CreateAccountCommand(service, config),

            DepositDto r => new DepositCommand(service, new AccountIdentifier(r.Account), new Amount(r.Amount)),
            WithdrawDto r => new WithdrawCommand(service, new AccountIdentifier(r.Account), new Amount(r.Amount)),
            BalanceDto r => new GetBalanceCommand(service, new AccountIdentifier(r.Account)),
            RemoveAccountDto r => new RemoveAccountCommand(service, new AccountIdentifier(r.Account)),

            BankTotalDto => new GetBankTotalCommand(service),
            BankClientsDto => new GetClientCountCommand(service),

            _ => throw new ArgumentException($"No Command mapped for type {commandRecord.GetType().Name}")
        };
    }
}