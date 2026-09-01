using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH2689 漏电流测试仪驱动
/// </summary>
public class TH2689Driver : DeviceDriverBase
{
    public override string ModelName => "TH2689";
    public override string DisplayName => "TH2689 漏电流测试仪";
    public override string Category => "漏电流测试仪";
    public override int DefaultBaudRate => 19200;

    public override List<string> ConfigurableParameters => new()
    {
        "Voltage", "Current", "Function", "Speed", "Range", "ChgTime", "Dweli", "RangeAuto"
    };

    public TH2689Driver(ISerialPortService serialPortService) : base(serialPortService) { }

    public override async Task<bool> ConfigureAsync(Dictionary<string, string> parameters)
    {
        try
        {
            await SendAsync(":DISPlay:LCTest");
            await Task.Delay(100);

            if (parameters.TryGetValue("Voltage", out var voltage))
                await SendAsync($":LCTest:SOURce:VOLTage {voltage}");

            if (parameters.TryGetValue("Current", out var current))
                await SendAsync($":LCTest:SOURce:CURRent {current}");

            if (parameters.TryGetValue("Function", out var func))
                await SendAsync($":LCTest:CONFigure:FUNCtion {func}");

            if (parameters.TryGetValue("Speed", out var speed))
                await SendAsync($":LCTest:CONFigure:SPEed {speed}");

            if (parameters.TryGetValue("Range", out var range))
                await SendAsync($":LCTest:CONFigure:RANGe {range}");

            if (parameters.TryGetValue("ChgTime", out var chgTime))
                await SendAsync($":LCTest:CONFigure:CHGTime {chgTime}");

            if (parameters.TryGetValue("Dweli", out var dweli))
                await SendAsync($":LCTest:CONFigure:DWELl {dweli}");

            if (parameters.TryGetValue("RangeAuto", out var rangeAuto))
                await SendAsync($":LCTest:CONFigure:RANGe:AUTO {rangeAuto}");

            Notify("TH2689 参数配置完成");
            return true;
        }
        catch (Exception ex)
        {
            Notify($"TH2689 配置失败: {ex.Message}");
            return false;
        }
    }

    public override async Task<DeviceTestResult> MeasureAsync()
    {
        try
        {
            var ir = await SendAsync(":LCTest:MEASure:IR?");
            var lc = await SendAsync(":LCTest:MEASure:LC?");

            return DeviceTestResult.Ok(new Dictionary<string, object?>
            {
                ["ILForward"] = ir,
                ["HLForward"] = "",
                ["ILReverse"] = "",
                ["HLReverse"] = ""
            });
        }
        catch (Exception ex)
        {
            return DeviceTestResult.Fail(ex.Message);
        }
    }
}
