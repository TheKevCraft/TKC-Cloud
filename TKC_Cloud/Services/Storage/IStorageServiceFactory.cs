namespace TKC_Cloud.Services.Storage;

public interface IStorageServiceFactory
{
    IStorageService Create();
    IStorageService Create(string provider);
}