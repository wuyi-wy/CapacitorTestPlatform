namespace CapacitorTestPlatform.Core.Models;

public class DeviceTestResult
{
    public bool Success { get; set; }
    public Dictionary<string, object?> Data { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public static DeviceTestResult Ok(Dictionary<string, object?> data) => new() { Success = true, Data = data };
    public static DeviceTestResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
