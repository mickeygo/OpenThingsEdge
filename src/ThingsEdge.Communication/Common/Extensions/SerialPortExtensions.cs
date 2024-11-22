using System.IO.Ports;
using System.Text.RegularExpressions;

namespace ThingsEdge.Communication.Common.Extensions;

internal static class SerialPortExtensions
{
    /// <summary>
    /// 使用格式化的串口参数信息来初始化串口的参数，举例：9600-8-N-1，分别表示波特率，数据位，奇偶校验，停止位，当然也可以携带串口名称，例如：COM3-9600-8-N-1，linux环境也是支持的。
    /// </summary>
    /// <remarks>
    /// 其中奇偶校验的字母可选，N:无校验，O：奇校验，E:偶校验，停止位可选 0, 1, 2, 1.5 四种选项。
    /// </remarks>
    /// <param name="serialPort">串口对象信息</param>
    /// <param name="format">格式化的参数内容，例如：9600-8-N-1</param>
    public static void IniSerialByFormatString(this SerialPort serialPort, string format)
    {
        var array = format.Split(['-', ';'], StringSplitOptions.RemoveEmptyEntries);
        if (array.Length != 0)
        {
            var num = 0;
            if (!Regex.IsMatch(array[0], "^[0-9]+$"))
            {
                serialPort.PortName = array[0];
                num = 1;
            }
            if (num < array.Length)
            {
                serialPort.BaudRate = Convert.ToInt32(array[num++]);
            }
            if (num < array.Length)
            {
                serialPort.DataBits = Convert.ToInt32(array[num++]);
            }
            if (num < array.Length)
            {
                serialPort.Parity = array[num++].ToUpper() switch
                {
                    "E" => Parity.Even,
                    "O" => Parity.Odd,
                    "N" => Parity.None,
                    _ => Parity.Space,
                };
            }
            if (num < array.Length)
            {
                serialPort.StopBits = array[num++] switch
                {
                    "0" => StopBits.None,
                    "2" => StopBits.Two,
                    "1" => StopBits.One,
                    _ => StopBits.OnePointFive,
                };
            }
        }
    }

    /// <summary>
    /// 将一个串口对象的基本配置参数进行格式化字符串，例如 COM3-9600-8-N-1。
    /// </summary>
    /// <remarks>
    /// 其中奇偶校验的字母可选，N:无校验，O：奇校验，E:偶校验，停止位可选 0, 1, 2, 1.5 四种选项。
    /// </remarks>
    /// <param name="serialPort">串口对象信息</param>
    /// <returns>串口对的格式化字符串信息</returns>
    public static string ToFormatString(this SerialPort serialPort)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(serialPort.PortName);
        stringBuilder.Append('-');
        stringBuilder.Append(serialPort.BaudRate);
        stringBuilder.Append('-');
        stringBuilder.Append(serialPort.DataBits);
        stringBuilder.Append('-');
        switch (serialPort.Parity)
        {
            case Parity.None:
                stringBuilder.Append('N');
                break;
            case Parity.Even:
                stringBuilder.Append('E');
                break;
            case Parity.Odd:
                stringBuilder.Append('O');
                break;
            case Parity.Space:
                stringBuilder.Append('S');
                break;
            default:
                stringBuilder.Append('M');
                break;
        }
        stringBuilder.Append('-');
        switch (serialPort.StopBits)
        {
            case StopBits.None:
                stringBuilder.Append('0');
                break;
            case StopBits.One:
                stringBuilder.Append('1');
                break;
            case StopBits.Two:
                stringBuilder.Append('2');
                break;
            default:
                stringBuilder.Append("1.5");
                break;
        }
        return stringBuilder.ToString();
    }
}
