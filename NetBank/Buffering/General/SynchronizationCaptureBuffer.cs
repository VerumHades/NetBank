namespace NetBank.Buffering.General;

public class SynchronizationCaptureBuffer<T> where T : ICaptureBuffer
{
    private readonly DoubleBuffer<T> _buffer;
    private readonly IProcessor<T> _processor;

    public SynchronizationCaptureBuffer(IFactory<T> factory, IProcessor<T> processor, ICaptureObserver<T> observer)
    {
        _buffer = new DoubleBuffer<T>(factory);
        _processor = processor;
        
        observer.OnShouldSwap = Swap;
        observer.SetBuffer(_buffer);
    }

    private async Task Swap()
    {
        _buffer.Swap();
        await _processor.Flush(_buffer.Back);
    }
}