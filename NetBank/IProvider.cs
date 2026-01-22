namespace NetBank;

public interface IProvider<T>
{
    T Get();
}