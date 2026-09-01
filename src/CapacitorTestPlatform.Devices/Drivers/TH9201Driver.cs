using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH9201 安规测试仪驱动
/// </summary>
public class TH9201Driver : DeviceDriverBase
{
    private int _stepNum = 1;

    public override string ModelName => "TH9201";
    public override string DisplayName => "TH9201 安规测试仪";
    public override string Category => "极壳耐压";
    public override int DefaultBaudRate => 19200;

    public override List<string> ConfigurableParameters => new()
    {
        "Step", "Volt", "Upper", "Lower", "Time"
    };

    public TH9201Driver(ISerialPortService serialPortService) : base(serialPortService) { }

    public override async Task<bool> ConfigureAsync(Dictionary<string, string> parameters)
    {
        try
        {
            if (parameters.TryGetValue("Step", out var step))
            {
                var funcNum = step switch
                {
                    "AC" => "1",
                    "DC" => "2",
                    "IR" => "3",
                    "OS" => "4",
                    _ => "1"
                };

                await SendAsync($":SOUR:SAFE:STEP {_stepNum}:FUNC {funcNum}");

                if (parameters.TryGetValue("Volt", out var volt))
                    await SendAsync($":SOUR:SAFE:STEP {_stepNum}:{step}:LEV {volt}");

                if (parameters.TryGetValue("Upper", out var upper))
                    await SendAsync($":SOUR:SAFE:STEP {_stepNum}:{step}:LIM:HIGH {upper}");

                var lower = parameters.GetValueOrDefault("Lower", "0");
                await SendAsync($":SOUR:SAFE:STEP {_stepNum}:{step}:LIM:LOW {lower}");

                if (parameters.TryGetValue("Time", out var time))
                    await SendAsync($":SOUR:SAFE:STEP {_stepNum}:{step}:TIME:TEST {time}");

                _stepNum++;
            }

            Notify("TH9201 参数配置完成");
            return true;
        }
        catch (Exception ex)
        {
            Notify($"TH9201 配置失败: {ex.Message}");
            return false;
        }
    }

    public override async Task<DeviceTestResult> MeasureAsync()
    {
        try
        {
            var result = await SendAsync(":TEST:FETCH?");
            var fetchValue = result.Trim();

            var displayResult = fetchValue switch
            {
                "1" => "PASS",
                "2" => "HIGH FAIL",
                "3" => "LOW FAIL",
                "4" => "ARC FAIL",
                "5" => "RANGE FAIL",
                _ => fetchValue
            };

            return DeviceTestResult.Ok(new Dictionary<string, object?>
            {
                ["Result"] = displayResult
            });
        }
        catch (Exception ex)
        {
            return DeviceTestResult.Fail(ex.Message);
        }
    }

    public void ResetStep()
    {
        _stepNum = 1;
    }
}
