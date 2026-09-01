using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH2683A 绝缘电阻测试仪驱动，通过 SCPI 指令配置充电/测量参数并采集绝缘电阻数据。
/// </summary>
public class TH2683ADriver : DeviceDriverBase
{
    /// <summary>设备型号标识</summary>
    public override string ModelName => "TH2683A";
    /// <summary>设备显示名称</summary>
    public override string DisplayName => "TH2683A 绝缘电阻测试仪";
    /// <summary>设备分类</summary>
    public override string Category => "绝缘电阻测试仪";
    /// <summary>默认波特率</summary>
    public override int DefaultBaudRate => 9600;

    /// <summary>可配置参数列表</summary>
    public override List<string> ConfigurableParameters => new()
    {
        "Voltage", "CurrentTime", "CheckTime", "CheckSpeed", "WaitTime", "Mode", "FreeTime"
    };

    /// <summary>
    /// 初始化 TH2683A 驱动。
    /// </summary>
    /// <param name="serialPortService">串口通信服务</param>
    public TH2683ADriver(ISerialPortService serialPortService) : base(serialPortService) { }

    /// <summary>
    /// 配置 TH2683A 参数，先检查设备状态，再依次设置电压、充电时间、测量时间等。
    /// </summary>
    /// <param name="parameters">参数名值对</param>
    /// <returns>配置是否成功</returns>
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

    /// <summary>
    /// 执行一次绝缘电阻测量，切换到测量页面后读取 FETC 数据。
    /// </summary>
    /// <returns>包含绝缘电阻测量数据的结果</returns>
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
