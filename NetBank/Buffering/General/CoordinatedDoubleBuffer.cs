namespace NetBank.Buffering.General;

public class CoordinatedDoubleBuffer<T> where T : ICaptureBuffer
{
    private readonly DoubleBuffer<T> _buffer;
    private readonly IProcessor<T> _processor;
    public T Buffer => _buffer.Front;

    public CoordinatedDoubleBuffer(IFactory<T> factory, IProcessor<T> processor, ICaptureObserverFactory<T> observerFactory)
    {
        _buffer = new DoubleBuffer<T>(factory);
        _processor = processor;
        
        observerFactory.Create(_buffer, Swap);
    }

    private async Task Swap()
    {
        _buffer.Swap();
        await _processor.Flush(_buffer.Back);
    }
}