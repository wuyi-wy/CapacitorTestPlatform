using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH2810B+ LCR数字电桥驱动
/// </summary>
public class TH2810BDriver : DeviceDriverBase
{
    public override string ModelName => "TH2810B";
    public override string DisplayName => "TH2810B+ LCR数字电桥";
    public override string Category => "LCR数字电桥";
    public override int DefaultBaudRate => 19200;

    public override List<string> ConfigurableParameters => new()
    {
        "Function", "Frequency", "Voltage", "Range", "Speed", "ListFrequency", "ListVoltage", "Mode"
    };

    public TH2810BDriver(ISerialPortService serialPortService) : base(serialPortService) { }

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

            if (parameters.TryGetValue("Range", out var range))
            {
                if (range == "AUTO")
                    await SendAsync("FUNC:IMP:RANG:AUTO ON");
                else
                    await SendAsync($"FUNC:IMP:RANG {range}");
            }

            if (parameters.TryGetValue("Speed", out var speed))
                await SendAsync($"APER {speed}");

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
                await SendAsync($"LIST:VOLT {listVolt}");
                isListMode = true;
            }

            if (isListMode)
            {
                if (parameters.TryGetValue("Mode", out var mode))
                    await SendAsync($"LIST:MODE {mode}");
            }

            await SendAsync("TRIG:Source BUS");

            Notify("TH2810B+ 参数配置完成");
            return true;
        }
        catch (Exception ex)
        {
            Notify($"TH2810B+ 配置失败: {ex.Message}");
            return false;
        }
    }

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

            var lc = "0";
            var ir = "0";

            for (int i = 0; i < parts.Length; i++)
            {
                if (i % 5 == 0)
                    lc = (double.Parse(parts[i]) * 1000000).ToString("F4");
                else if (i % 5 == 1)
                    ir = (double.Parse(parts[i]) * 1).ToString("F6");
            }

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
