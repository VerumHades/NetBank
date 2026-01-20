using NetBank.Buffering.General;
using NetBank.Storage;

namespace NetBank;

class Program
{
    public class BufferFactory: IFactory<StorageCaptureBuffer>
    {
        public StorageCaptureBuffer Create()
        {
            return new StorageCaptureBuffer();
        }
    }

    public class ObserverFactory : ICaptureObserverFactory<StorageCaptureBuffer>
    {
        public void Create(DoubleBuffer<StorageCaptureBuffer> buffer, Func<Task> OnShouldSwap)
        {
            new TimedBufferObserver<StorageCaptureBuffer>(OnShouldSwap, buffer);
        }
    }
    static async Task Main(string[] args)
    {
        var bufferFactory = new BufferFactory();
        var observerFactory = new ObserverFactory(); 
        
        var inmemStorage = new InMemoryStorageStrategy();
        var processor = new StorageBufferProcessor(inmemStorage);
        
        var buffer = new CoordinatedDoubleBuffer<StorageCaptureBuffer>(bufferFactory, processor, observerFactory);

        var account = await buffer.Buffer.CreateAccount();
        var total = await buffer.Buffer.BankTotal();
    }
}
