using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Robot.YASKAWA.Helper;

/// <summary>
/// 安川机器人的高速以太网的辅助方法
/// </summary>
public class YRCHighEthernetHelper
{
    /// <summary>
    /// 构建完整的读取指令
    /// </summary>
    /// <param name="handle">处理分区，1:机器人控制 2:文件控制</param>
    /// <param name="requestID">请求ID， 客户端每次命令输出的时请增量</param>
    /// <param name="command">命令编号，相当于CIP通信的CLASS</param>
    /// <param name="dataAddress">数据队列编号，相当于CIP通信的Instance</param>
    /// <param name="dataAttribute">单元编号，相当于CIP通信协议的Attribute</param>
    /// <param name="dataHandle">处理请求，定义数据的请方法</param>
    /// <param name="dataPart">数据部分的内容</param>
    /// <returns>构建结果</returns>
    public static byte[] BuildCommand(byte handle, byte requestID, ushort command, ushort dataAddress, byte dataAttribute, byte dataHandle, byte[] dataPart)
    {
        var memoryStream = new MemoryStream();
        memoryStream.Write(Encoding.ASCII.GetBytes("YERC"));
        memoryStream.Write(new byte[4] { 32, 0, 0, 0 });
        memoryStream.Write(new byte[4] { 3, handle, 0, requestID });
        memoryStream.Write(new byte[4]);
        memoryStream.Write(Encoding.ASCII.GetBytes("99999999"));
        memoryStream.Write(BitConverter.GetBytes(command));
        memoryStream.Write(BitConverter.GetBytes(dataAddress));
        memoryStream.Write(new byte[4] { dataAttribute, dataHandle, 0, 0 });
        if (dataPart != null)
        {
            memoryStream.Write(dataPart);
        }
        var array = memoryStream.ToArray();
        array[6] = BitConverter.GetBytes(array.Length - 32)[0];
        array[7] = BitConverter.GetBytes(array.Length - 32)[1];
        return array;
    }

    /// <summary>
    /// 检查当前的机器人反馈的数据是否正确
    /// </summary>
    /// <param name="response">从机器人反馈的数据</param>
    /// <returns>是否检查正确</returns>
    public static OperateResult CheckResponseContent(byte[] response)
    {
        if (response[25] != 0)
        {
            var b = response[25];
            var affix = 0;
            if (b == 31)
            {
                affix = response[26] != 1 ? BitConverter.ToUInt16(response, 28) : response[28];
            }
            return new OperateResult(b, GetErrorText(b, affix));
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 根据状态信息及附加状态信息来获取错误的文本描述信息
    /// </summary>
    /// <param name="status">状态信息</param>
    /// <param name="affix">附加状态信息</param>
    /// <returns>错误的文本描述信息</returns>
    public static string GetErrorText(byte status, int affix)
    {
        return status switch
        {
            8 => "未定义被要求的命令。",
            9 => "检出无效的数据单元编号。",
            40 => "请求的数据排列编号不存在指定的命令。",
            31 => affix switch
            {
                4112 => "命令异常",
                4113 => "命令操作数异常",
                4114 => "命令操作值超出范围",
                4115 => "命令操作长异常",
                4128 => "设备的文件数太多。",
                8208 => "机器人动作中",
                8224 => "示教编程器HOLD停止中",
                8240 => "再线盒HOLD停止中",
                8256 => "外部HOLD中",
                8272 => "命令HOLD中",
                8288 => "发生错误报警中",
                8304 => "伺服ON中",
                8320 => "模式不同",
                8336 => "访问其他功能的文件中",
                8448 => "没有命令模式设定",
                8464 => "此数据不能访问",
                8480 => "此数据不能读取",
                8496 => "编辑中",
                8528 => "执行坐标变换功能中",
                12304 => "请接通伺服电源",
                12352 => "请确认原点位置",
                12368 => "请进行位置确认",
                12400 => "无法生成现在值",
                12832 => "收到锁定面板模式／循环禁止信号",
                12848 => "面板锁定 收到禁止启动信号",
                13136 => "没有示教用户坐标",
                13152 => "用户坐标文件被损坏",
                13168 => "控制轴组不同",
                13184 => "基座轴数据不同",
                13200 => "不可变换相对JOB ",
                13312 => "禁止调用主程序 （参数）",
                13328 => "禁止调用主程序 （动作中灯亮）",
                13344 => "禁止调用主程序 （示教锁定）",
                13360 => "未定义机器人间的校准",
                13392 => "不能接通伺服电源",
                13408 => "不能设定坐标系",
                16400 => "内存容量不足 （程序登录内存）",
                16402 => "内存容量不足 （ 变位机数据登录内存）",
                16416 => "禁止编辑程序",
                16432 => "存在相同名称的程序",
                16448 => "没有指定的程序",
                16480 => "请设定执行的程序",
                16672 => "位置数据被损坏",
                16688 => "位置数据部存在",
                16704 => "位置变量类型不同",
                16720 => "不是主程序的程序 END命令",
                16752 => "命令数据被损坏",
                16784 => "程序名中存在不合适的字符",
                16896 => "标签名中存在不合适的字符",
                16944 => "本系统中存在不能使用的命令",
                17440 => "转换的程序没有步骤",
                17456 => "此程序已全被转换",
                17536 => "请示教用户坐标",
                17552 => "相对JOB／ 独立控制功能未被许可",
                20752 => "语法错误 （命令的语法）",
                20768 => "变位机数据异常",
                20784 => "缺少NOP或者 END命令",
                20848 => "格式错误（违背写法）",
                20864 => "数据数不恰当",
                20992 => "超出数据范围",
                21264 => "语法错误 （ 命令以外）",
                21312 => "模拟命令指定错误",
                21360 => "存在条件数据记录错误",
                21392 => "存在程序数据记录错误",
                21552 => "系统数据不一致",
                21632 => "焊接机类型不一致",
                24592 => "机器人或工装轴动作中",
                24608 => "指定设备容量不足",
                24624 => "指定设备无法访问",
                24640 => "预想外自动备份要求",
                24656 => "CMOS 大小在RAM 区域超出",
                24672 => "电源接通时，无法确保内存",
                24688 => "备份文件信息访问异常",
                24704 => "备份文件排序（删除）失败",
                24720 => "备份文件排序（重命名）失败",
                24832 => "驱动名称超出规定值",
                24848 => "设备不同",
                24864 => "系统错误",
                24880 => "不可设定自动备份",
                24896 => "自动备份中不可手动备份",
                40960 => "未定义命令",
                40961 => "数据排列编号（ Instance） 异常",
                40962 => "单元编号（ Attribute） 异常",
                41216 => "响应数据部大小( 硬件限制值) 异常",
                41218 => "响应数据部大小(软件限制值)异常",
                45057 => "未定义位置变量",
                45058 => "禁止使用数据",
                45059 => "请求数据大小异常",
                45060 => "数据范围以外",
                45061 => "未设定数据",
                45062 => "未登录指定的用途",
                45063 => "未登录指定的机种",
                45064 => "控制轴组设定异常",
                45065 => "速度设定异常",
                45066 => "未设定动作速度",
                45067 => "动作坐标系设定异常",
                45068 => "形态设定异常",
                45069 => "工具编号设定异常",
                45070 => "用户编号设定异常",
                _ => StringResources.Language.UnknownError,
            },
            _ => StringResources.Language.UnknownError,
        };
    }
}
