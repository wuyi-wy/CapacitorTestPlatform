using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH2817A LCR 数字电桥驱动，支持测量功能/频率配置及列表频率扫描模式。
/// </summary>
public class TH2817ADriver : DeviceDriverBase
{
    /// <summary>设备型号标识</summary>
    public override string ModelName => "TH2817A";
    /// <summary>设备显示名称</summary>
    public override string DisplayName => "TH2817A LCR数字电桥";
    /// <summary>设备分类</summary>
    public override string Category => "LCR数字电桥";
    /// <summary>默认波特率</summary>
    public override int DefaultBaudRate => 9600;

    /// <summary>可配置参数列表</summary>
    public override List<string> ConfigurableParameters => new()
    {
        "Function", "Frequency", "ListFrequency"
    };

    /// <summary>
    /// 初始化 TH2817A 驱动。
    /// </summary>
    /// <param name="serialPortService">串口通信服务</param>
    public TH2817ADriver(ISerialPortService serialPortService) : base(serialPortService) { }

    /// <summary>
    /// 配置 TH2817A 参数，设置测量功能、频率，支持列表频率扫描模式。
    /// </summary>
    /// <param name="parameters">参数名值对</param>
    /// <returns>配置是否成功</returns>
    public override async Task<bool> ConfigureAsync(Dictionary<string, string> parameters)
    {
        try
        {
            await SendAsync("DISPlay:PAGE MeAsurement");
            await Task.Delay(100);

            if (parameters.TryGetValue("Function", out var func))
                await SendAsync($"FUNC:IMP {func}");

            if (parameters.TryGetValue("Frequency", out var freq))
                await SendAsync($"FREQUENCY {freq}");

            if (parameters.TryGetValue("ListFrequency", out var listFreq) && listFreq.Contains(','))
            {
                await SendAsync("DISPlay:PAGE LIST");
                await Task.Delay(100);
                await SendAsync($"LIST:FREQ {listFreq}");
            }

            await SendAsync("TRIG:Source BUS");

            Notify("TH2817A 参数配置完成");
            return true;
        }
        catch (Exception ex)
        {
            Notify($"TH2817A 配置失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 执行一次电容测量，触发 BUS 触发后读取 *OPC? 结果。
    /// </summary>
    /// <returns>包含电容和频率测量数据的结果</returns>
    public override async Task<DeviceTestResult> MeasureAsync()
    {
        try
        {
            await SendAsync("DISPlay:PAGE MeAsurement");
            await Task.Delay(100);

            await SendAsync("TRIG");
            await Task.Delay(500);

            var result = await SendAsync("*OPC?");
            var parts = result.Split(',');

            var lc = parts.Length > 0 ? parts[0] : "0";
            var ir = parts.Length > 1 ? parts[1] : "0";

            return DeviceTestResult.Ok(new Dictionary<string, object?>
            {
                ["Capacitance"] = lc,
                ["Frequency"] = ""
            });
        }
        catch (Exception ex)
        {
            return DeviceTestResult.Fail(ex.Message);
        }
    }
}
