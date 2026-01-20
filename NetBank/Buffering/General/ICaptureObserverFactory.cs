namespace NetBank.Buffering.General;

public interface ICaptureObserverFactory<T> where T: ICaptureBuffer
{
    void Create(DoubleBuffer<T> buffer, Func<Task> OnShouldSwap);
}