namespace CapacitorTestPlatform.Core.Interfaces;

public interface IProductInfoService
{
    Task<Models.ProductInfo?> GetProductInfoAsync(string productModel);
}
