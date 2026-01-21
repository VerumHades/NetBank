namespace NetBank.Buffering;

public interface ICaptureObserverFactory<T> where T: ICaptureBuffer
{
    IDisposable Create(DoubleBuffer<T> buffer, Func<Task<bool>> onShouldSwap);
}