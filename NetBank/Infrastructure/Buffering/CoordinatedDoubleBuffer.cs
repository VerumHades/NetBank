namespace NetBank.Buffering;

public class CoordinatedDoubleBuffer<T> : IDisposable where T : ICaptureBuffer
{
    private readonly DoubleBuffer<T> _buffer;
    private readonly IProcessor<T> _processor;
    private readonly SemaphoreSlim _swapLock = new(1, 1);
    private readonly IDisposable _observer; 

    public T Buffer => _buffer.Front;

    public CoordinatedDoubleBuffer(IFactory<T> factory, IProcessor<T> processor, ICaptureObserverFactory<T> observerFactory)
    {
        _buffer = new DoubleBuffer<T>(factory);
        _processor = processor;
        
        _observer = observerFactory.Create(_buffer, TrySwap);
    }

    public async Task<bool> TrySwap()
    {
        if (!await _swapLock.WaitAsync(0)) return false;

        try
        {
            _buffer.Swap(); 
            await _processor.Flush(_buffer.Back);
            return true; 
        }
        catch
        {
            return false;
        }
        finally
        {
            _swapLock.Release();
        }
    }

    public void Dispose()
    {
        _observer?.Dispose(); 
        
        _swapLock.Dispose();

        _buffer.Back.Clear();
        _buffer.Front.Clear();
    }
}