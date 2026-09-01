using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH2683A 绝缘电阻测试仪驱动
/// </summary>
public class TH2683ADriver : DeviceDriverBase
{
    public override string ModelName => "TH2683A";
    public override string DisplayName => "TH2683A 绝缘电阻测试仪";
    public override string Category => "绝缘电阻测试仪";
    public override int DefaultBaudRate => 9600;

    public override List<string> ConfigurableParameters => new()
    {
        "Voltage", "CurrentTime", "CheckTime", "CheckSpeed", "WaitTime", "Mode", "FreeTime"
    };

    public TH2683ADriver(ISerialPortService serialPortService) : base(serialPortService) { }

    public override async Task<bool> ConfigureAsync(Dictionary<string, string> parameters)
    {
        try
        {
            var status = await SendAsync("SYSTem:STSTus?");
            if (status.Contains("DISCharging") || status.Contains("TESTing"))
            {
                Notify("设备正在放电或测试中，请稍候...");
                return false;
            }

            await SendAsync("DISPlay:PAGE MSETup");
            await Task.Delay(100);

            if (parameters.TryGetValue("Voltage", out var voltage))
                await SendAsync($":FUNCtion:OVOLTage {voltage}");

            if (parameters.TryGetValue("CurrentTime", out var currentTime))
                await SendAsync($":FUNCtion:CTIMe {currentTime}");

            if (parameters.TryGetValue("CheckTime", out var checkTime))
                await SendAsync($":FUNCtion:MTIMe {checkTime}");

            if (parameters.TryGetValue("CheckSpeed", out var checkSpeed))
                await SendAsync($":FUNCtion:MSPeed {checkSpeed}");

            if (parameters.TryGetValue("Mode", out var mode))
                await SendAsync($":FUNCtion:MMode {mode}");

            if (parameters.TryGetValue("WaitTime", out var waitTime))
                await SendAsync($":FUNCtion:WTIMe {waitTime}");

            if (parameters.TryGetValue("FreeTime", out var freeTime))
                await SendAsync($":FUNCtion:DTIMe {freeTime}");

            Notify("TH2683A 参数配置完成");
            return true;
        }
        catch (Exception ex)
        {
            Notify($"TH2683A 配置失败: {ex.Message}");
            return false;
        }
    }

    public override async Task<DeviceTestResult> MeasureAsync()
    {
        try
        {
            await SendAsync("DISP:PAGE MEAS");
            await Task.Delay(200);

            var result = await SendAsync("fetc?");
            var parts = result.Split(',');

            var lc = parts.Length > 0 ? (double.Parse(parts[0]) * 1000000).ToString("F3") : "0";
            var ir = parts.Length > 1 ? (double.Parse(parts[1]) / 1000000).ToString("F5") : "0";

            return DeviceTestResult.Ok(new Dictionary<string, object?>
            {
                ["InsulationResistance"] = ir
            });
        }
        catch (Exception ex)
        {
            return DeviceTestResult.Fail(ex.Message);
        }
    }
}
