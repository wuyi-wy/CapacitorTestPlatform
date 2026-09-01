using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Services;

/// <summary>
/// 设备业务服务，提供可用设备查询、设备驱动创建以及测试项列定义。
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly IDeviceFactory _deviceFactory;

    /// <summary>
    /// 初始化设备服务。
    /// </summary>
    /// <param name="deviceFactory">设备工厂，用于创建各型号设备驱动实例</param>
    public DeviceService(IDeviceFactory deviceFactory)
    {
        _deviceFactory = deviceFactory;
    }

    /// <summary>
    /// 获取所有可用设备信息列表。
    /// </summary>
    /// <returns>设备信息列表，包含型号、分类等基本信息</returns>
    public async Task<List<DeviceInfo>> GetAvailableDevicesAsync()
    {
        return await Task.FromResult(_deviceFactory.GetAllDevices());
    }

    /// <summary>
    /// 根据设备型号创建对应的设备驱动实例。
    /// </summary>
    /// <param name="deviceModel">设备型号（如 TH2689、TH2817A 等）</param>
    /// <returns>设备驱动实例，型号不存在时返回 null</returns>
    public async Task<IDeviceDriver?> GetDriverAsync(string deviceModel)
    {
        return await Task.FromResult(_deviceFactory.Create(deviceModel));
    }

    /// <summary>
    /// 根据测试项和设备型号获取数据表格的列定义。
    /// 不同测试项对应不同的列结构，用于动态生成 DataGrid 列。
    /// </summary>
    /// <param name="testItem">测试项名称（如"电容量"、"漏电流"、"绝缘外套的绝缘电阻"等）</param>
    /// <param name="deviceModel">设备型号</param>
    /// <returns>列定义列表，包含表头、属性名、单位和列宽</returns>
    public List<TestColumnDefinition> GetColumnsForTestItem(string testItem, string deviceModel)
    {
        return testItem switch
        {
            "电容量" => new List<TestColumnDefinition>
            {
                new() { Header = "C", PropertyName = "Capacitance", Unit = "μF", Width = 150 },
                new() { Header = "频率", PropertyName = "Frequency", Unit = "Hz", Width = 120 }
            },
            "损耗角正切值(ESR)" or "损耗角正切" => new List<TestColumnDefinition>
            {
                new() { Header = "损耗", PropertyName = "Loss", Unit = "", Width = 150 },
                new() { Header = "频率", PropertyName = "Frequency", Unit = "Hz", Width = 120 }
            },
            "等效串联电阻(ESR)" or "ESR" => new List<TestColumnDefinition>
            {
                new() { Header = "ESR", PropertyName = "ESR", Unit = "MΩ", Width = 150 },
                new() { Header = "频率", PropertyName = "Frequency", Unit = "Hz", Width = 120 }
            },
            "阻抗" => new List<TestColumnDefinition>
            {
                new() { Header = "阻抗", PropertyName = "Impedance", Unit = "MΩ", Width = 150 },
                new() { Header = "频率", PropertyName = "Frequency", Unit = "Hz", Width = 120 }
            },
            "绝缘外套的绝缘电阻" => new List<TestColumnDefinition>
            {
                new() { Header = "绝缘电阻", PropertyName = "InsulationResistance", Unit = "MΩ", Width = 180 }
            },
            "漏电流" => new List<TestColumnDefinition>
            {
                new() { Header = "IL正向", PropertyName = "ILForward", Unit = "μA", Width = 120 },
                new() { Header = "HL正向", PropertyName = "HLForward", Unit = "S", Width = 100 },
                new() { Header = "IL反向", PropertyName = "ILReverse", Unit = "μA", Width = 120 },
                new() { Header = "HL反向", PropertyName = "HLReverse", Unit = "S", Width = 100 }
            },
            _ => new List<TestColumnDefinition>()
        };
    }
}
