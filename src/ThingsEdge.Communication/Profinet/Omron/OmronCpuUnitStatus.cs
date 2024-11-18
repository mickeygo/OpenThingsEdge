using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙Cpu的状态信息。
/// </summary>
public sealed class OmronCpuUnitStatus
{
    /// <summary>
    /// Run 或是 Stop
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// No 或是 Present
    /// </summary>
    public string BatteryStatus { get; set; }

    /// <summary>
    /// Normal 或是 Standby
    /// </summary>
    public string CpuStatus { get; set; }

    /// <summary>
    /// PROGRAM, MONITOR, RUN
    /// </summary>
    public string Mode { get; set; }

    /// <summary>
    /// Among errors that occur when the command is executed, the error code indicates the most serious. If there are no errors, it will be 0000 (hex)
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// Indicates messages from execution of FAL(006) or FALS(007). If there is no error message, 
    /// or if FAL(006) or FALS(007) are not being executed, 16 spaces( ASCII 20) will be returned.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 从原始的字节数组来实例化一个
    /// </summary>
    /// <param name="data">原始的字节数据</param>
    public OmronCpuUnitStatus(byte[] data)
    {
        Status = data[0].GetBoolByIndex(0) ? "Run" : "Stop";
        BatteryStatus = data[0].GetBoolByIndex(2) ? "Present" : "No";
        CpuStatus = data[0].GetBoolByIndex(7) ? "Standby" : "Normal";
        Mode = data[1] == 0 ? "PROGRAM" : data[1] == 2 ? "MONITOR" : data[1] == 4 ? "RUN" : "";
        ErrorCode = data[8] * 256 + data[9];
        if (ErrorCode > 0)
        {
            ErrorMessage = Encoding.ASCII.GetString(data, 10, 16).TrimEnd(' ', '\0');
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "OmronCpuUnitStatus[" + Status + "]";
    }
}
