namespace NetBank.Types;

public record DefferedOperation<T>
{
    public TaskCompletionSource<T> TaskCompletionSource { get; } = new();
}