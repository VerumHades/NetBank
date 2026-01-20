namespace NetBank;

public interface IProcessor<T>
{
    Task Flush(T value);
}