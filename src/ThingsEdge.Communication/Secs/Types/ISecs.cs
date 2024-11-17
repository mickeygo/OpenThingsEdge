using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Secs.Types;

/// <summary>
/// ISecs的接口信息，支持了将数据发送到对方，或是使用问答机制从设备获取数据<br />
/// The interface information of ISecs supports sending data to the other party, or using the question and answer mechanism to obtain data from the device
/// </summary>
public interface ISecs
{
    /// <summary>
    /// 将数据发送到设备方去，只是单纯的发送数据过去，并不等待设备的数据返回，返回是否发送成功。<br />
    /// Sending data to the device side simply sends the data to the past, and does not wait for the data from the device to return, and returns whether the transmission is successful.
    /// </summary>
    /// <param name="stream">功能码1</param>
    /// <param name="function">功能码2</param>
    /// <param name="data">原始的字节数据</param>
    /// <param name="back">是否必须返回，此标记仅仅是secs报文的是否返回标记，不表示问答模式</param>
    /// <returns>是否发送成功的结果对象</returns>
    OperateResult SendByCommand(byte stream, byte function, byte[] data, bool back);

    /// <summary>
    /// 将数据发送到设备方去，只是单纯的发送数据过去，并不等待设备的数据返回，返回是否发送成功。<br />
    /// Sending data to the device side simply sends the data to the past, and does not wait for the data from the device to return, and returns whether the transmission is successful.
    /// </summary>
    /// <param name="stream">功能码1</param>
    /// <param name="function">功能码2</param>
    /// <param name="data">Secs格式的对象信息</param>
    /// <param name="back">是否必须返回，此标记仅仅是secs报文的是否返回标记，不表示问答模式</param>
    /// <returns>是否发送成功的结果对象</returns>
    OperateResult SendByCommand(byte stream, byte function, SecsValue data, bool back);

    /// <summary>
    /// 根据指定的功能码将数据报文发送给设备，并且等待从SECS设备返回Secs消息，本访问机制是问答模式的。<br />
    /// Send the data message to the device according to the specified function code, and wait for the Secs message to be returned from the SECS device. This access mechanism is in question-and-answer mode.
    /// </summary>
    /// <param name="stream">功能码1</param>
    /// <param name="function">功能码2</param>
    /// <param name="data">原始的字节数据</param>
    /// <param name="back">是否必须返回，此标记仅仅是secs报文的是否返回标记，不表示问答模式</param>
    /// <returns>返回SECS消息结果对象</returns>
    OperateResult<SecsMessage> ReadSecsMessage(byte stream, byte function, byte[] data, bool back);

    /// <summary>
    /// 根据指定的功能码将数据报文发送给设备，并且等待从SECS设备返回Secs消息，本访问机制是问答模式的。<br />
    /// Send the data message to the device according to the specified function code, and wait for the Secs message to be returned from the SECS device. This access mechanism is in question-and-answer mode.
    /// </summary>
    /// <param name="stream">功能码1</param>
    /// <param name="function">功能码2</param>
    /// <param name="data">Secs格式的对象信息</param>
    /// <param name="back">是否必须返回，此标记仅仅是secs报文的是否返回标记，不表示问答模式</param>
    /// <returns>返回SECS消息结果对象</returns>
    OperateResult<SecsMessage> ReadSecsMessage(byte stream, byte function, SecsValue data, bool back);
}
