namespace NetBank.Buffering.General;

public  interface ICaptureBuffer
{
    public Action? NewClientListener { get; set; }
    public void Clear();
}