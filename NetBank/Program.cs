using NetBank.Buffering;
using NetBank.Storage;
using NetBank.Storage.Strategies;

namespace NetBank;

class Program
{
    private class BufferFactory: IFactory<AccountServiceCaptureBuffer>
    {
        public AccountServiceCaptureBuffer Create()
        {
            return new AccountServiceCaptureBuffer();
        }
    }

    private class ObserverFactory : ICaptureObserverFactory<AccountServiceCaptureBuffer>
    {
        public IDisposable Create(DoubleBuffer<AccountServiceCaptureBuffer> buffer, Func<Task<bool>> onShouldSwap)
        {
            return new TimedBufferObserver<AccountServiceCaptureBuffer>(onShouldSwap, buffer);
        }
    }
    static async Task Main(string[] args)
    {
        var bufferFactory = new BufferFactory();
        var observerFactory = new ObserverFactory(); 
        
        var inmemStorage = new InMemoryStorageStrategy();
        var processor = new StorageBufferProcessor(inmemStorage);
        
        var buffer = new CoordinatedDoubleBuffer<AccountServiceCaptureBuffer>(bufferFactory, processor, observerFactory);

        var account = await buffer.Buffer.CreateAccount();
        var total = await buffer.Buffer.BankTotal();
    }
}
