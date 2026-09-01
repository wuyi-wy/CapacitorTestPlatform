using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH9201 安规测试仪驱动，用于极壳耐压测试，支持 AC/DC/IR/OS 多步骤配置。
/// </summary>
public class TH9201Driver : DeviceDriverBase
{
    /// <summary>当前测试步骤编号，每次配置递增</summary>
    private int _stepNum = 1;

    /// <summary>设备型号标识</summary>
    public override string ModelName => "TH9201";
    /// <summary>设备显示名称</summary>
    public override string DisplayName => "TH9201 安规测试仪";
    /// <summary>设备分类</summary>
    public override string Category => "极壳耐压";
    /// <summary>默认波特率</summary>
    public override int DefaultBaudRate => 19200;

    /// <summary>可配置参数列表</summary>
    public override List<string> ConfigurableParameters => new()
    {
        "Step", "Volt", "Upper", "Lower", "Time"
    };

    /// <summary>
    /// 初始化 TH9201 驱动。
    /// </summary>
    /// <param name="serialPortService">串口通信服务</param>
    public TH9201Driver(ISerialPortService serialPortService) : base(serialPortService) { }

    /// <summary>
    /// 配置 TH9201 测试步骤，设置功能类型、电压、上下限及测试时间。每调用一次步骤号自动递增。
    /// </summary>
    /// <param name="parameters">参数名值对（Step: AC/DC/IR/OS, Volt, Upper, Lower, Time）</param>
    /// <returns>配置是否成功</returns>
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

    /// <summary>
    /// 执行一次耐压测试，读取测试结果并转换为可读状态（PASS/HIGH FAIL/LOW FAIL 等）。
    /// </summary>
    /// <returns>包含测试结果的测量数据</returns>
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

    /// <summary>
    /// 重置测试步骤编号为 1，用于新一轮测试配置。
    /// </summary>
    public void ResetStep()
    {
        _stepNum = 1;
    }
}
