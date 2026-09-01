using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH2817A LCR数字电桥驱动
/// </summary>
public class TH2817ADriver : DeviceDriverBase
{
    public override string ModelName => "TH2817A";
    public override string DisplayName => "TH2817A LCR数字电桥";
    public override string Category => "LCR数字电桥";
    public override int DefaultBaudRate => 9600;

    public override List<string> ConfigurableParameters => new()
    {
        "Function", "Frequency", "ListFrequency"
    };

    public TH2817ADriver(ISerialPortService serialPortService) : base(serialPortService) { }

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
