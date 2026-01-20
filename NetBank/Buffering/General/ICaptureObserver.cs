namespace NetBank.Buffering.General;

public interface ICaptureObserver<T> where T: ICaptureBuffer
{
    public Func<Task> OnShouldSwap { set; }
    void SetBuffer(DoubleBuffer<T> buffer);
}