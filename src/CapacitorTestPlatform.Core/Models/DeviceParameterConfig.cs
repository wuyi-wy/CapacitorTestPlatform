namespace CapacitorTestPlatform.Core.Models;

/// <summary>
/// 设备参数配置元数据 - 定义每个参数的UI表现
/// </summary>
public class DeviceParameterConfig
{
    public string ParameterName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string InputType { get; set; } = "TextBox"; // TextBox / ComboBox
    public List<string> Options { get; set; } = new();
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? Unit { get; set; }
}

/// <summary>
/// 设备完整配置描述
/// </summary>
public class DeviceConfigProfile
{
    public string DeviceModel { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DefaultBaudRate { get; set; }
    public bool IsMock { get; set; }
    public List<DeviceParameterConfig> Parameters { get; set; } = new();
    public List<string> OutputFields { get; set; } = new();

    public static List<DeviceConfigProfile> GetAllProfiles()
    {
        return new List<DeviceConfigProfile>
        {
            new()
            {
                DeviceModel = "TH2689", DisplayName = "TH2689 漏电流测试仪",
                Category = "漏电流测试仪", DefaultBaudRate = 19200,
                OutputFields = new() { "LC", "IR", "LCf", "IRf" },
                Parameters = new()
                {
                    new() { ParameterName = "Voltage", DisplayName = "测试电压", InputType = "TextBox", Placeholder = "1~800", Unit = "V" },
                    new() { ParameterName = "Current", DisplayName = "测试电流", InputType = "TextBox", Placeholder = "0.5~500" },
                    new() { ParameterName = "Function", DisplayName = "测试模式", InputType = "ComboBox", Options = new() { "SEQ", "STEP", "CONT" }, DefaultValue = "SEQ" },
                    new() { ParameterName = "Speed", DisplayName = "测试速度", InputType = "ComboBox", Options = new() { "FAST", "MEDium", "SLOW" }, DefaultValue = "FAST" },
                    new() { ParameterName = "Range", DisplayName = "电流量程", InputType = "ComboBox", Options = new() { "2uA", "20uA", "200uA", "2mA", "20mA" }, DefaultValue = "2uA" },
                    new() { ParameterName = "ChgTime", DisplayName = "充电时间", InputType = "TextBox", Placeholder = "0~999", Unit = "s" },
                    new() { ParameterName = "Dweli", DisplayName = "延迟时间", InputType = "TextBox", Placeholder = "0.2~999", Unit = "s" },
                    new() { ParameterName = "RangeAuto", DisplayName = "自动量程", InputType = "ComboBox", Options = new() { "OFF", "ON" }, DefaultValue = "OFF" }
                }
            },
            new()
            {
                DeviceModel = "TH2683A", DisplayName = "TH2683A 绝缘电阻测试仪",
                Category = "绝缘电阻测试仪", DefaultBaudRate = 9600,
                OutputFields = new() { "LC", "IR" },
                Parameters = new()
                {
                    new() { ParameterName = "Voltage", DisplayName = "测试电压", InputType = "ComboBox", Options = new() { "100", "500" }, DefaultValue = "500", Unit = "V" },
                    new() { ParameterName = "CurrentTime", DisplayName = "加压时间", InputType = "TextBox", Unit = "s" },
                    new() { ParameterName = "CheckTime", DisplayName = "检测时间", InputType = "TextBox", Unit = "s" },
                    new() { ParameterName = "CheckSpeed", DisplayName = "检测速度", InputType = "ComboBox", Options = new() { "FAST", "SLOW" }, DefaultValue = "FAST" },
                    new() { ParameterName = "WaitTime", DisplayName = "等待时间", InputType = "TextBox", Unit = "s" },
                    new() { ParameterName = "Mode", DisplayName = "测试模式", InputType = "ComboBox", Options = new() { "SINGle", "CONTinuous" }, DefaultValue = "SINGle" },
                    new() { ParameterName = "FreeTime", DisplayName = "放电时间", InputType = "TextBox", Unit = "s" }
                }
            },
            new()
            {
                DeviceModel = "TH2817A", DisplayName = "TH2817A LCR数字电桥",
                Category = "LCR数字电桥", DefaultBaudRate = 9600,
                OutputFields = new() { "LC", "IR" },
                Parameters = new()
                {
                    new() { ParameterName = "Function", DisplayName = "阻抗功能", InputType = "ComboBox", Options = new() { "CPD", "CSD", "CSRS", "LPQ", "LSRS", "ZTD", "ZTR", "RX" }, DefaultValue = "CSD" },
                    new() { ParameterName = "Frequency", DisplayName = "测试频率", InputType = "ComboBox", Options = new() { "MIN", "100", "120", "200", "400", "800", "1k", "2k", "4k", "8k", "10k", "20k", "40k", "80k", "100k" }, DefaultValue = "120", Unit = "Hz" },
                    new() { ParameterName = "ListFrequency", DisplayName = "扫频列表", InputType = "TextBox", Placeholder = "100,1k,10k,100k（逗号分隔）" }
                }
            },
            new()
            {
                DeviceModel = "TH2832", DisplayName = "TH2832 LCR数字电桥",
                Category = "LCR数字电桥", DefaultBaudRate = 115200,
                OutputFields = new() { "LC", "IR" },
                Parameters = new()
                {
                    new() { ParameterName = "Function", DisplayName = "阻抗功能", InputType = "ComboBox", Options = new() { "CPD", "CSD", "CSRS", "LSRS", "LPQ", "ZTD", "ZTR", "RX" }, DefaultValue = "CSD" },
                    new() { ParameterName = "Frequency", DisplayName = "测试频率", InputType = "ComboBox", Options = new() { "MIN", "100", "120", "200", "400", "800", "1k", "2k", "4k", "8k", "10k", "20k", "40k", "80k", "100k" }, DefaultValue = "120", Unit = "Hz" },
                    new() { ParameterName = "Voltage", DisplayName = "测试电压", InputType = "ComboBox", Options = new() { "10mV", "20mV", "100mV", "200mV", "300mV", "500mV", "1V", "2V" }, DefaultValue = "1V" },
                    new() { ParameterName = "Speed", DisplayName = "测量速率", InputType = "ComboBox", Options = new() { "FAST", "MED", "SLOW" }, DefaultValue = "FAST" },
                    new() { ParameterName = "Range", DisplayName = "量程", InputType = "ComboBox", Options = new() { "AUTO", "3", "10", "30", "100", "300", "1000", "3000", "10000", "30000", "100000" }, DefaultValue = "AUTO" },
                    new() { ParameterName = "ListFrequency", DisplayName = "扫频频率", InputType = "TextBox", Placeholder = "100,1k,10k,100k" },
                    new() { ParameterName = "ListVoltage", DisplayName = "扫频电压", InputType = "TextBox", Placeholder = "100mV,500mV,1V" }
                }
            },
            new()
            {
                DeviceModel = "TH9201", DisplayName = "TH9201 安规测试仪",
                Category = "极壳耐压", DefaultBaudRate = 19200,
                OutputFields = new() { "Result" },
                Parameters = new()
                {
                    new() { ParameterName = "Step", DisplayName = "测试步骤", InputType = "ComboBox", Options = new() { "AC", "DC", "IR", "OS" }, DefaultValue = "AC" },
                    new() { ParameterName = "Volt", DisplayName = "测试电压", InputType = "TextBox", Unit = "V" },
                    new() { ParameterName = "Upper", DisplayName = "上限值", InputType = "TextBox" },
                    new() { ParameterName = "Lower", DisplayName = "下限值", InputType = "TextBox", DefaultValue = "0" },
                    new() { ParameterName = "Time", DisplayName = "测试时间", InputType = "TextBox", Unit = "s" }
                }
            },
            new()
            {
                DeviceModel = "TH2810B", DisplayName = "TH2810B+ LCR数字电桥",
                Category = "LCR数字电桥", DefaultBaudRate = 19200,
                OutputFields = new() { "LC", "IR" },
                Parameters = new()
                {
                    new() { ParameterName = "Function", DisplayName = "阻抗功能", InputType = "ComboBox", Options = new() { "CSD" }, DefaultValue = "CSD" },
                    new() { ParameterName = "Frequency", DisplayName = "测试频率", InputType = "ComboBox", Options = new() { "100", "120", "1k", "10k" }, DefaultValue = "120", Unit = "Hz" },
                    new() { ParameterName = "Voltage", DisplayName = "测试电压", InputType = "ComboBox", Options = new() { "10mV", "20mV", "50mV", "100mV", "250mV", "500mV", "1V" }, DefaultValue = "1V" },
                    new() { ParameterName = "Range", DisplayName = "量程", InputType = "ComboBox", Options = new() { "AUTO", "3", "10", "30", "100", "300", "1000", "3000", "10000", "30000", "100000" }, DefaultValue = "AUTO" },
                    new() { ParameterName = "Speed", DisplayName = "测量速率", InputType = "ComboBox", Options = new() { "FAST", "MED", "SLOW" }, DefaultValue = "FAST" },
                    new() { ParameterName = "ListFrequency", DisplayName = "扫频频率", InputType = "TextBox", Placeholder = "100,1k,10k" },
                    new() { ParameterName = "ListVoltage", DisplayName = "扫频电压", InputType = "TextBox", Placeholder = "100mV,500mV,1V" },
                    new() { ParameterName = "Mode", DisplayName = "列表模式", InputType = "ComboBox", Options = new() { "SEQ", "STEP" }, DefaultValue = "SEQ" }
                }
            },
            new()
            {
                DeviceModel = "MOCK", DisplayName = "模拟设备",
                Category = "模拟设备", DefaultBaudRate = 9600, IsMock = true,
                OutputFields = new() { "Data1", "Data2", "Data3", "Data4" },
                Parameters = new()
                {
                    new() { ParameterName = "Function", DisplayName = "测试功能", InputType = "ComboBox", Options = new() { "CSD", "CPD", "ZTD" }, DefaultValue = "CSD" },
                    new() { ParameterName = "Frequency", DisplayName = "测试频率", InputType = "ComboBox", Options = new() { "120", "1k", "10k", "100k" }, DefaultValue = "120", Unit = "Hz" },
                    new() { ParameterName = "Count", DisplayName = "采集次数", InputType = "TextBox", DefaultValue = "5" }
                }
            }
        };
    }
}
