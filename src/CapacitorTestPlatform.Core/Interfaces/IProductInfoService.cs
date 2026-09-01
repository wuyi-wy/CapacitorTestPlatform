namespace CapacitorTestPlatform.Core.Interfaces;

/// <summary>
/// 产品信息服务接口，查询产品型号对应的详细信息。
/// </summary>
public interface IProductInfoService
{
    /// <summary>根据产品型号查询产品信息</summary>
    Task<Models.ProductInfo?> GetProductInfoAsync(string productModel);
}
