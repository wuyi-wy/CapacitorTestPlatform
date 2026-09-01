using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH2832 LCR 数字电桥驱动（增强版），支持电压/速度/量程配置及列表频率/电压扫描模式。
/// </summary>
public class TH2832Driver : DeviceDriverBase
{
    /// <summary>设备型号标识</summary>
    public override string ModelName => "TH2832";
    /// <summary>设备显示名称</summary>
    public override string DisplayName => "TH2832 LCR数字电桥";
    /// <summary>设备分类</summary>
    public override string Category => "LCR数字电桥";
    /// <summary>默认波特率</summary>
    public override int DefaultBaudRate => 115200;

    /// <summary>可配置参数列表</summary>
    public override List<string> ConfigurableParameters => new()
    {
        "Function", "Frequency", "Voltage", "Speed", "Range", "ListFrequency", "ListVoltage", "Mode"
    };

    /// <summary>
    /// 初始化 TH2832 驱动。
    /// </summary>
    /// <param name="serialPortService">串口通信服务</param>
    public TH2832Driver(ISerialPortService serialPortService) : base(serialPortService) { }

    /// <summary>
    /// 配置 TH2832 参数，设置测量功能、频率、电压、速度、量程，支持列表扫描模式。
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

            if (parameters.TryGetValue("Voltage", out var voltage))
                await SendAsync($"VOLTage {voltage}");

            if (parameters.TryGetValue("Speed", out var speed))
                await SendAsync($"APER {speed}");

            if (parameters.TryGetValue("Range", out var range))
            {
                if (range == "AUTO")
                    await SendAsync("FUNC:IMP:RANG:AUTO ON");
                else
                    await SendAsync($"FUNC:IMP:RANG {range}");
            }

            var isListMode = false;
            if (parameters.TryGetValue("ListFrequency", out var listFreq) && listFreq.Contains(','))
            {
                await SendAsync("DISPlay:PAGE LIST");
                await Task.Delay(100);
                await SendAsync($"LIST:FREQ {listFreq}");
                isListMode = true;
            }

            if (parameters.TryGetValue("ListVoltage", out var listVolt) && listVolt.Contains(','))
            {
                await SendAsync($"LIST:VOLT1 {listVolt}");
                isListMode = true;
            }

            if (isListMode)
            {
                await SendAsync("rs232:print on");
            }

            Notify("TH2832 参数配置完成");
            return true;
        }
        catch (Exception ex)
        {
            Notify($"TH2832 配置失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 执行一次电容测量，触发后读取 FETC? 数据并转换单位。
    /// </summary>
    /// <returns>包含电容和频率测量数据的结果</returns>
    public override async Task<DeviceTestResult> MeasureAsync()
    {
        try
        {
            await SendAsync("DISPlay:PAGE MEASurement");
            await Task.Delay(100);

            await SendAsync("TRIG");
            await Task.Delay(500);

            var result = await SendAsync("FETC?");
            var parts = result.Split(',');

            var lc = parts.Length > 0 ? (double.Parse(parts[0]) * 1000000).ToString("F3") : "0";
            var ir = parts.Length > 1 ? (double.Parse(parts[1]) * 1).ToString("F5") : "0";

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
