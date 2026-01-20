using NetBank.Buffering.General;

namespace NetBank.Storage;

public class TimedBufferObserver<T> : IDisposable where T: class, ICaptureBuffer
{
    private readonly Func<Task> _onShouldSwap;
    private readonly DoubleBuffer<T> _buffer;
    private readonly TimeSpan _swapTimeout = TimeSpan.FromMilliseconds(150);
    
    private CancellationTokenSource? _timerCancellation;
    private readonly object _lock = new();

    public TimedBufferObserver(Func<Task> onShouldSwap, DoubleBuffer<T> buffer)
    {
        _onShouldSwap = onShouldSwap;
        _buffer = buffer;

        _buffer.Front.NewClientListener = OnNewClientDetected;
        _buffer.Back.NewClientListener = OnNewClientDetected;
    }

    private void OnNewClientDetected()
    {
        lock (_lock)
        {
            if (_timerCancellation != null) return;

            _timerCancellation = new CancellationTokenSource();

            _ = StartSwapTimer(_timerCancellation.Token);
        }
    }

    private async Task StartSwapTimer(CancellationToken token)
    {
        try
        {
            await Task.Delay(_swapTimeout, token);
            await _onShouldSwap();
        }
        catch (OperationCanceledException)
        {
            
        }
        finally
        {
            lock (_lock)
            {
                _timerCancellation?.Dispose();
                _timerCancellation = null;
            }
        }
    }

    public void Dispose()
    {
        _timerCancellation?.Cancel();
        _timerCancellation?.Dispose();
        
        _buffer.Front.NewClientListener = null;
        _buffer.Back.NewClientListener = null;
    }
}