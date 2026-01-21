namespace NetBank;

public interface IStorageProvider
{
    IAccountService GetStorage();
}