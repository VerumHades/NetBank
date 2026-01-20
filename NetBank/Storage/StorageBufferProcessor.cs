using NetBank.Caching;
using NetBank.ErrorHandling;
using NetBank.Types;

namespace NetBank.Storage;

public class StorageBufferProcessor : IProcessor<StorageCaptureBuffer>
{
    private readonly IStorageStrategy _storageStrategy;
    private readonly Cache<AccountIdentifier, Amount> _cache;

    public StorageBufferProcessor(IStorageStrategy storageStrategy)
    {
        _storageStrategy = storageStrategy;
        _cache = new Cache<AccountIdentifier, Amount>(100, new LruEvictionPolicy<AccountIdentifier>());
    }

    public async Task Flush(StorageCaptureBuffer capture)
    {
        await PrefetchAccountsIntoCache(capture.TouchedAccounts);
        
        await CreateAccounts(capture.CreationOperations);
        await _storageStrategy.RemoveAccounts(FilterValidRemovals(capture.RemoveOperations));
        
        await _storageStrategy.DepositAll(FilterValidDeposits(capture.DepositOperations));
        await _storageStrategy.WithdrawAll(FilterValidWithdrawals(capture.WithdrawOperations));
        
        await ResolveBankTotalRequests(capture.BankTotalRequests);
        await ResolveBankClientCountRequests(capture.ClientNumberRequests);
        ResolveBalanceRequests(capture.BalanceRequests);
    }

    private async Task PrefetchAccountsIntoCache(IEnumerable<AccountIdentifier> touchedAccounts)
    {
        var accountList = touchedAccounts.ToList();
        if (_cache.MaximumCapacity < accountList.Count)
            _cache.MaximumCapacity = accountList.Count;

        var accounts = await _storageStrategy.BalanceAll(accountList);
        foreach (var (account, amount) in accounts)
            _cache.Set(account, amount);
    }

    private async Task CreateAccounts(IReadOnlyList<TaskCompletionSource<AccountIdentifier>> creationOps)
    {
        var accounts = await _storageStrategy.CreateAccounts(creationOps.Count);
        var accountList = accounts.ToList();

        for (int i = 0; i < creationOps.Count; i++)
        {
            if (i < accountList.Count){
                var id = accountList[i];
                creationOps[i].SetResult(id);
                _cache.Set(id, new Amount(0));
            }
            else
                creationOps[i].SetException(CreateException(ErrorOrigin.System, "Failed to create account."));
        }
    }

    private List<AccountAndAmount> FilterValidDeposits(IEnumerable<(TaskCompletionSource, AccountIdentifier, Amount)> ops)
    {
        var valid = new List<AccountAndAmount>();
        foreach (var (tcs, id, amount) in ops)
        {
            if (_cache.TryGet(id, out Amount balance))
            {
                var newBalance = balance + amount;
                _cache.Set(id, newBalance);
                
                valid.Add(new AccountAndAmount(id, amount));
                tcs.SetResult();
            }
            else
            {
                tcs.SetException(CreateException(ErrorOrigin.Client, "Account does not exist."));
            }
        }
        return valid;
    }

    private List<AccountAndAmount> FilterValidWithdrawals(IEnumerable<(TaskCompletionSource, AccountIdentifier, Amount)> ops)
    {
        var valid = new List<AccountAndAmount>();
        foreach (var (tcs, id, amount) in ops)
        {
            if (!_cache.TryGet(id, out var balance))
            {
                tcs.SetException(CreateException(ErrorOrigin.Client, "Account does not exist."));
            }
            else if (balance < amount)
            {
                tcs.SetException(CreateException(ErrorOrigin.Client, "Insufficient funds."));
            }
            else
            {
                var newBalance = balance - amount;
                _cache.Set(id, newBalance);
                
                tcs.SetResult();
                valid.Add(new AccountAndAmount(id, amount));
            }
        }
        return valid;
    }

    private List<AccountIdentifier> FilterValidRemovals(IEnumerable<(TaskCompletionSource, AccountIdentifier)> ops)
    {
        var valid = new List<AccountIdentifier>();
        foreach (var (tcs, id) in ops)
        {
            if (_cache.TryGet(id, out _))
            {
                _cache.Remove(id);
                
                valid.Add(id);
                tcs.SetResult();
            }
            else
            {
                tcs.SetException(CreateException(ErrorOrigin.Client, "Account does not exist."));
            }
        }
        return valid;
    }

    private async Task ResolveBankTotalRequests(IEnumerable<TaskCompletionSource<Amount>> requests)
    {
        var total = await _storageStrategy.BankTotal();
        foreach (var tcs in requests) tcs.SetResult(total);
    }

    private async Task ResolveBankClientCountRequests(IEnumerable<TaskCompletionSource<int>> requests)
    {
        var count = await _storageStrategy.BankNumberOfClients();
        foreach (var tcs in requests) tcs.SetResult(count);
    }

    private void ResolveBalanceRequests(IEnumerable<(TaskCompletionSource<Amount>, AccountIdentifier)> requests)
    {
        foreach (var (tcs, id) in requests)
        {
            if (_cache.TryGet(id, out var amount))
                tcs.SetResult(amount);
            else
                tcs.SetException(CreateException(ErrorOrigin.Client, "Account does not exist."));
        }
    }

    private static ModuleException CreateException(ErrorOrigin origin, string message) =>
        new(new ModuleErrorIdentifier(Module.StorageProcessor), origin, message);
    
}