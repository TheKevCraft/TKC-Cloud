namespace TKC_Cloud.Services.Storage;

public interface IStorageServiceFactory
{
    IStorageService Create();
}