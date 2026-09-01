using CapacitorTestPlatform.Core.Interfaces;
using CapacitorTestPlatform.Core.Models;

namespace CapacitorTestPlatform.Devices.Drivers;

/// <summary>
/// TH2689 漏电流测试仪驱动，通过 SCPI 指令配置电压/电流/速度等参数并采集漏电流数据。
/// </summary>
public class TH2689Driver : DeviceDriverBase
{
    /// <summary>设备型号标识</summary>
    public override string ModelName => "TH2689";
    /// <summary>设备显示名称</summary>
    public override string DisplayName => "TH2689 漏电流测试仪";
    /// <summary>设备分类</summary>
    public override string Category => "漏电流测试仪";
    /// <summary>默认波特率</summary>
    public override int DefaultBaudRate => 19200;

    /// <summary>可配置参数列表</summary>
    public override List<string> ConfigurableParameters => new()
    {
        "Voltage", "Current", "Function", "Speed", "Range", "ChgTime", "Dweli", "RangeAuto"
    };

    /// <summary>
    /// 初始化 TH2689 驱动。
    /// </summary>
    /// <param name="serialPortService">串口通信服务</param>
    public TH2689Driver(ISerialPortService serialPortService) : base(serialPortService) { }

    /// <summary>
    /// 配置 TH2689 参数，依次设置电压、电流、功能、速度、量程等。
    /// </summary>
    /// <param name="parameters">参数名值对</param>
    /// <returns>配置是否成功</returns>
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

    /// <summary>
    /// 执行一次漏电流测量，读取 IR 和 LC 值。
    /// </summary>
    /// <returns>包含漏电流测量数据的结果</returns>
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
