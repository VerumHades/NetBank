namespace NetBank.Buffering;

public  interface ICaptureBuffer
{
    public Action? NewClientListener { get; set; }
    public bool HasPending { get; }
    public void Clear();
}