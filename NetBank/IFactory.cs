namespace NetBank;

public interface IFactory<T>
{
    T Create();
}