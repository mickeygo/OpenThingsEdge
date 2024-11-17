namespace ThingsEdge.Communication.HslCommunication;

/// <summary>
/// 操作结果的类，只带有成功标志和错误信息。
/// </summary>
/// <remarks>
/// 当 <see cref="IsSuccess" /> 为 True 时，忽略 <see cref="Message" /> 及 <see cref="ErrorCode" /> 的值
/// </remarks>
public class OperateResult
{
    /// <summary>
    /// 指示本次操作是否成功。
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 具体的错误描述。
    /// </summary>
    public string Message { get; set; } = StringResources.Language.UnknownError;

    /// <summary>
    /// 具体的错误代码。
    /// </summary>
    /// <remarks>
    /// 默认的错误码值为 10000，此时需要看 <see cref="Message" /> 的消息来查看错误信息，如果由于网络问题导致的错误，错误码小于 0，
    /// 其他情况的错误都是对应协议的错误码，有些错误码的描述信息可以看 <see cref="Message" />，如果消息也是未知的话，此时需要根据错误码找对应协议的手册来确认错误消息。
    /// </remarks>
    public int ErrorCode { get; set; } = 10000;

    /// <summary>
    /// 实例化一个默认的结果对象
    /// </summary>
    public OperateResult()
    {
    }

    /// <summary>
    /// 使用指定的消息实例化一个默认的结果对象
    /// </summary>
    /// <param name="msg">错误消息</param>
    public OperateResult(string msg)
    {
        Message = msg;
    }

    /// <summary>
    /// 使用错误代码，消息文本来实例化对象
    /// </summary>
    /// <param name="err">错误代码</param>
    /// <param name="msg">错误消息</param>
    public OperateResult(int err, string msg)
    {
        ErrorCode = err;
        Message = msg;
    }

    /// <summary>
    /// 获取错误代号及文本描述。
    /// </summary>
    /// <returns>包含错误码及错误消息</returns>
    public string ToMessageShowString()
    {
        return $"{StringResources.Language.ErrorCode}:{ErrorCode}{Environment.NewLine}{StringResources.Language.TextDescription}:{Message}";
    }

    /// <summary>
    /// 从另一个结果类中拷贝错误信息，主要是针对错误码和错误消息。
    /// </summary>
    /// <typeparam name="TResult">支持结果类及派生类</typeparam>
    /// <param name="result">结果类及派生类的对象</param>
    public void CopyErrorFromOther<TResult>(TResult result) where TResult : OperateResult
    {
        if (result != null)
        {
            ErrorCode = result.ErrorCode;
            Message = result.Message;
        }
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，如果当前结果为失败，则返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T">结果类型</typeparam>
    /// <param name="content">如果操作成功将赋予的结果内容</param>
    /// <returns>最终的结果类对象</returns>
    public OperateResult<T> Convert<T>(T content)
    {
        return IsSuccess ? CreateSuccessResult(content) : CreateFailedResult<T>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，直接返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T">结果类型</typeparam>
    /// <returns>最终失败的结果类对象</returns>
    public OperateResult<T> ConvertFailed<T>()
    {
        return CreateFailedResult<T>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，如果当前结果为失败，则返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <param name="content1">如果操作成功将赋予的结果内容一</param>
    /// <param name="content2">如果操作成功将赋予的结果内容二</param>
    /// <returns>最终的结果类对象</returns>
    public OperateResult<T1, T2> Convert<T1, T2>(T1 content1, T2 content2)
    {
        return IsSuccess ? CreateSuccessResult(content1, content2) : CreateFailedResult<T1, T2>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，直接返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <returns>最终失败的结果类对象</returns>
    public OperateResult<T1, T2> ConvertFailed<T1, T2>()
    {
        return CreateFailedResult<T1, T2>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，如果当前结果为失败，则返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <param name="content1">如果操作成功将赋予的结果内容一</param>
    /// <param name="content2">如果操作成功将赋予的结果内容二</param>
    /// <param name="content3">如果操作成功将赋予的结果内容三</param>
    /// <returns>最终的结果类对象</returns>
    public OperateResult<T1, T2, T3> Convert<T1, T2, T3>(T1 content1, T2 content2, T3 content3)
    {
        return IsSuccess ? CreateSuccessResult(content1, content2, content3) : CreateFailedResult<T1, T2, T3>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，直接返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <returns>最终失败的结果类对象</returns>
    public OperateResult<T1, T2, T3> ConvertFailed<T1, T2, T3>()
    {
        return CreateFailedResult<T1, T2, T3>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，如果当前结果为失败，则返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <typeparam name="T4">泛型参数四</typeparam>
    /// <param name="content1">如果操作成功将赋予的结果内容一</param>
    /// <param name="content2">如果操作成功将赋予的结果内容二</param>
    /// <param name="content3">如果操作成功将赋予的结果内容三</param>
    /// <param name="content4">如果操作成功将赋予的结果内容四</param>
    /// <returns>最终的结果类对象</returns>
    public OperateResult<T1, T2, T3, T4> Convert<T1, T2, T3, T4>(T1 content1, T2 content2, T3 content3, T4 content4)
    {
        return IsSuccess ? CreateSuccessResult(content1, content2, content3, content4) : CreateFailedResult<T1, T2, T3, T4>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，直接返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <typeparam name="T4">泛型参数四</typeparam>
    /// <returns>最终失败的结果类对象</returns>
    public OperateResult<T1, T2, T3, T4> ConvertFailed<T1, T2, T3, T4>()
    {
        return CreateFailedResult<T1, T2, T3, T4>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，如果当前结果为失败，则返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <typeparam name="T4">泛型参数四</typeparam>
    /// <typeparam name="T5">泛型参数五</typeparam>
    /// <param name="content1">如果操作成功将赋予的结果内容一</param>
    /// <param name="content2">如果操作成功将赋予的结果内容二</param>
    /// <param name="content3">如果操作成功将赋予的结果内容三</param>
    /// <param name="content4">如果操作成功将赋予的结果内容四</param>
    /// <param name="content5">如果操作成功将赋予的结果内容五</param>
    /// <returns>最终的结果类对象</returns>
    public OperateResult<T1, T2, T3, T4, T5> Convert<T1, T2, T3, T4, T5>(T1 content1, T2 content2, T3 content3, T4 content4, T5 content5)
    {
        return IsSuccess ? CreateSuccessResult(content1, content2, content3, content4, content5) : CreateFailedResult<T1, T2, T3, T4, T5>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，直接返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <typeparam name="T4">泛型参数四</typeparam>
    /// <typeparam name="T5">泛型参数五</typeparam>
    /// <returns>最终失败的结果类对象</returns>
    public OperateResult<T1, T2, T3, T4, T5> ConvertFailed<T1, T2, T3, T4, T5>()
    {
        return CreateFailedResult<T1, T2, T3, T4, T5>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，如果当前结果为失败，则返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <typeparam name="T4">泛型参数四</typeparam>
    /// <typeparam name="T5">泛型参数五</typeparam>
    /// <typeparam name="T6">泛型参数六</typeparam>
    /// <param name="content1">如果操作成功将赋予的结果内容一</param>
    /// <param name="content2">如果操作成功将赋予的结果内容二</param>
    /// <param name="content3">如果操作成功将赋予的结果内容三</param>
    /// <param name="content4">如果操作成功将赋予的结果内容四</param>
    /// <param name="content5">如果操作成功将赋予的结果内容五</param>
    /// <param name="content6">如果操作成功将赋予的结果内容六</param>
    /// <returns>最终的结果类对象</returns>
    public OperateResult<T1, T2, T3, T4, T5, T6> Convert<T1, T2, T3, T4, T5, T6>(T1 content1, T2 content2, T3 content3, T4 content4, T5 content5, T6 content6)
    {
        return IsSuccess ? CreateSuccessResult(content1, content2, content3, content4, content5, content6) : CreateFailedResult<T1, T2, T3, T4, T5, T6>(this);
    }

    /// <summary>
    /// 将当前的结果对象转换到指定泛型的结果类对象，直接返回指定泛型的失败结果类对象。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <typeparam name="T4">泛型参数四</typeparam>
    /// <typeparam name="T5">泛型参数五</typeparam>
    /// <typeparam name="T6">泛型参数六</typeparam>
    /// <returns>最终失败的结果类对象</returns>
    public OperateResult<T1, T2, T3, T4, T5, T6> ConvertFailed<T1, T2, T3, T4, T5, T6>()
    {
        return CreateFailedResult<T1, T2, T3, T4, T5, T6>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult Then(Func<OperateResult> func)
    {
        return IsSuccess ? func() : this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="T">泛型参数</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<T> Then<T>(Func<OperateResult<T>> func)
    {
        return IsSuccess ? func() : CreateFailedResult<T>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<T1, T2> Then<T1, T2>(Func<OperateResult<T1, T2>> func)
    {
        return IsSuccess ? func() : CreateFailedResult<T1, T2>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<T1, T2, T3> Then<T1, T2, T3>(Func<OperateResult<T1, T2, T3>> func)
    {
        return IsSuccess ? func() : CreateFailedResult<T1, T2, T3>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <typeparam name="T4">泛型参数四</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<T1, T2, T3, T4> Then<T1, T2, T3, T4>(Func<OperateResult<T1, T2, T3, T4>> func)
    {
        return IsSuccess ? func() : CreateFailedResult<T1, T2, T3, T4>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <typeparam name="T4">泛型参数四</typeparam>
    /// <typeparam name="T5">泛型参数五</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<T1, T2, T3, T4, T5> Then<T1, T2, T3, T4, T5>(Func<OperateResult<T1, T2, T3, T4, T5>> func)
    {
        return IsSuccess ? func() : CreateFailedResult<T1, T2, T3, T4, T5>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="T1">泛型参数一</typeparam>
    /// <typeparam name="T2">泛型参数二</typeparam>
    /// <typeparam name="T3">泛型参数三</typeparam>
    /// <typeparam name="T4">泛型参数四</typeparam>
    /// <typeparam name="T5">泛型参数五</typeparam>
    /// <typeparam name="T6">泛型参数六</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<T1, T2, T3, T4, T5, T6> Then<T1, T2, T3, T4, T5, T6>(Func<OperateResult<T1, T2, T3, T4, T5, T6>> func)
    {
        return IsSuccess ? func() : CreateFailedResult<T1, T2, T3, T4, T5, T6>(this);
    }

    /// <inheritdoc />
    public static OperateResult<T> CreateFailedResult<T>(OperateResult result)
    {
        return new OperateResult<T>
        {
            ErrorCode = result.ErrorCode,
            Message = result.Message
        };
    }

    /// <summary>
    /// 创建并返回一个失败的结果对象，该对象复制另一个结果对象的错误信息
    /// </summary>
    /// <typeparam name="T">目标数据类型</typeparam>
    /// <param name="msgHead">额外添加的消息头信息</param>
    /// <param name="result">之前的结果对象</param>
    /// <returns>带默认泛型对象的失败结果类</returns>
    public static OperateResult<T> CreateFailedResult<T>(string msgHead, OperateResult result)
    {
        return new OperateResult<T>
        {
            ErrorCode = result.ErrorCode,
            Message = msgHead + " : " + result.Message
        };
    }

    /// <inheritdoc />
    public static OperateResult<T1, T2> CreateFailedResult<T1, T2>(OperateResult result)
    {
        return new OperateResult<T1, T2>
        {
            ErrorCode = result.ErrorCode,
            Message = result.Message
        };
    }

    /// <summary>
    /// 创建并返回一个失败的结果对象，该对象复制另一个结果对象的错误信息
    /// </summary>
    /// <typeparam name="T1">目标数据类型一</typeparam>
    /// <typeparam name="T2">目标数据类型二</typeparam>
    /// <param name="msgHead">额外添加的消息头信息</param>
    /// <param name="result">之前的结果对象</param>
    /// <returns>带默认泛型对象的失败结果类</returns>
    public static OperateResult<T1, T2> CreateFailedResult<T1, T2>(string msgHead, OperateResult result)
    {
        return new OperateResult<T1, T2>
        {
            ErrorCode = result.ErrorCode,
            Message = msgHead + " : " + result.Message
        };
    }

    /// <inheritdoc />
    public static OperateResult<T1, T2, T3> CreateFailedResult<T1, T2, T3>(OperateResult result)
    {
        return new OperateResult<T1, T2, T3>
        {
            ErrorCode = result.ErrorCode,
            Message = result.Message
        };
    }

    /// <summary>
    /// 创建并返回一个失败的结果对象，该对象复制另一个结果对象的错误信息
    /// </summary>
    /// <typeparam name="T1">目标数据类型一</typeparam>
    /// <typeparam name="T2">目标数据类型二</typeparam>
    /// <typeparam name="T3">目标数据类型三</typeparam>
    /// <param name="msgHead">额外添加的消息头信息</param>
    /// <param name="result">之前的结果对象</param>
    /// <returns>带默认泛型对象的失败结果类</returns>
    public static OperateResult<T1, T2, T3> CreateFailedResult<T1, T2, T3>(string msgHead, OperateResult result)
    {
        return new OperateResult<T1, T2, T3>
        {
            ErrorCode = result.ErrorCode,
            Message = msgHead + " : " + result.Message
        };
    }

    /// <inheritdoc />
    public static OperateResult<T1, T2, T3, T4> CreateFailedResult<T1, T2, T3, T4>(OperateResult result)
    {
        return new OperateResult<T1, T2, T3, T4>
        {
            ErrorCode = result.ErrorCode,
            Message = result.Message
        };
    }

    /// <summary>
    /// 创建并返回一个失败的结果对象，该对象复制另一个结果对象的错误信息
    /// </summary>
    /// <typeparam name="T1">目标数据类型一</typeparam>
    /// <typeparam name="T2">目标数据类型二</typeparam>
    /// <typeparam name="T3">目标数据类型三</typeparam>
    /// <typeparam name="T4">目标数据类型四</typeparam>
    /// <param name="msgHead">额外添加的消息头信息</param>
    /// <param name="result">之前的结果对象</param>
    /// <returns>带默认泛型对象的失败结果类</returns>
    public static OperateResult<T1, T2, T3, T4> CreateFailedResult<T1, T2, T3, T4>(string msgHead, OperateResult result)
    {
        return new OperateResult<T1, T2, T3, T4>
        {
            ErrorCode = result.ErrorCode,
            Message = msgHead + " : " + result.Message
        };
    }

    /// <summary>
    /// 创建并返回一个失败的结果对象，该对象复制另一个结果对象的错误信息
    /// </summary>
    /// <typeparam name="T1">目标数据类型一</typeparam>
    /// <typeparam name="T2">目标数据类型二</typeparam>
    /// <typeparam name="T3">目标数据类型三</typeparam>
    /// <typeparam name="T4">目标数据类型四</typeparam>
    /// <typeparam name="T5">目标数据类型五</typeparam>
    /// <param name="result">之前的结果对象</param>
    /// <returns>带默认泛型对象的失败结果类</returns>
    public static OperateResult<T1, T2, T3, T4, T5> CreateFailedResult<T1, T2, T3, T4, T5>(OperateResult result)
    {
        return new OperateResult<T1, T2, T3, T4, T5>
        {
            ErrorCode = result.ErrorCode,
            Message = result.Message
        };
    }

    /// <summary>
    /// 创建并返回一个失败的结果对象，该对象复制另一个结果对象的错误信息
    /// </summary>
    /// <typeparam name="T1">目标数据类型一</typeparam>
    /// <typeparam name="T2">目标数据类型二</typeparam>
    /// <typeparam name="T3">目标数据类型三</typeparam>
    /// <typeparam name="T4">目标数据类型四</typeparam>
    /// <typeparam name="T5">目标数据类型五</typeparam>
    /// <typeparam name="T6">目标数据类型六</typeparam>
    /// <param name="result">之前的结果对象</param>
    /// <returns>带默认泛型对象的失败结果类</returns>
    public static OperateResult<T1, T2, T3, T4, T5, T6> CreateFailedResult<T1, T2, T3, T4, T5, T6>(OperateResult result)
    {
        return new OperateResult<T1, T2, T3, T4, T5, T6>
        {
            ErrorCode = result.ErrorCode,
            Message = result.Message
        };
    }

    /// <summary>
    /// 创建并返回一个成功的结果对象
    /// </summary>
    /// <returns>成功的结果对象</returns>
    public static OperateResult CreateSuccessResult()
    {
        return new OperateResult
        {
            IsSuccess = true,
            ErrorCode = 0,
            Message = StringResources.Language.SuccessText
        };
    }

    /// <summary>
    /// 创建并返回一个成功的结果对象，并带有一个参数对象
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="value">类型的值对象</param>
    /// <returns>成功的结果对象</returns>
    public static OperateResult<T> CreateSuccessResult<T>(T value)
    {
        return new OperateResult<T>
        {
            IsSuccess = true,
            ErrorCode = 0,
            Message = StringResources.Language.SuccessText,
            Content = value
        };
    }

    /// <summary>
    /// 创建并返回一个成功的结果对象，并带有两个参数对象
    /// </summary>
    /// <typeparam name="T1">第一个参数类型</typeparam>
    /// <typeparam name="T2">第二个参数类型</typeparam>
    /// <param name="value1">类型一对象</param>
    /// <param name="value2">类型二对象</param>
    /// <returns>成的结果对象</returns>
    public static OperateResult<T1, T2> CreateSuccessResult<T1, T2>(T1 value1, T2 value2)
    {
        return new OperateResult<T1, T2>
        {
            IsSuccess = true,
            ErrorCode = 0,
            Message = StringResources.Language.SuccessText,
            Content1 = value1,
            Content2 = value2
        };
    }

    /// <summary>
    /// 创建并返回一个成功的结果对象，并带有三个参数对象
    /// </summary>
    /// <typeparam name="T1">第一个参数类型</typeparam>
    /// <typeparam name="T2">第二个参数类型</typeparam>
    /// <typeparam name="T3">第三个参数类型</typeparam>
    /// <param name="value1">类型一对象</param>
    /// <param name="value2">类型二对象</param>
    /// <param name="value3">类型三对象</param>
    /// <returns>成的结果对象</returns>
    public static OperateResult<T1, T2, T3> CreateSuccessResult<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        return new OperateResult<T1, T2, T3>
        {
            IsSuccess = true,
            ErrorCode = 0,
            Message = StringResources.Language.SuccessText,
            Content1 = value1,
            Content2 = value2,
            Content3 = value3
        };
    }

    /// <summary>
    /// 创建并返回一个成功的结果对象，并带有四个参数对象
    /// </summary>
    /// <typeparam name="T1">第一个参数类型</typeparam>
    /// <typeparam name="T2">第二个参数类型</typeparam>
    /// <typeparam name="T3">第三个参数类型</typeparam>
    /// <typeparam name="T4">第四个参数类型</typeparam>
    /// <param name="value1">类型一对象</param>
    /// <param name="value2">类型二对象</param>
    /// <param name="value3">类型三对象</param>
    /// <param name="value4">类型四对象</param>
    /// <returns>成的结果对象</returns>
    public static OperateResult<T1, T2, T3, T4> CreateSuccessResult<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        return new OperateResult<T1, T2, T3, T4>
        {
            IsSuccess = true,
            ErrorCode = 0,
            Message = StringResources.Language.SuccessText,
            Content1 = value1,
            Content2 = value2,
            Content3 = value3,
            Content4 = value4
        };
    }

    /// <summary>
    /// 创建并返回一个成功的结果对象，并带有五个参数对象
    /// </summary>
    /// <typeparam name="T1">第一个参数类型</typeparam>
    /// <typeparam name="T2">第二个参数类型</typeparam>
    /// <typeparam name="T3">第三个参数类型</typeparam>
    /// <typeparam name="T4">第四个参数类型</typeparam>
    /// <typeparam name="T5">第五个参数类型</typeparam>
    /// <param name="value1">类型一对象</param>
    /// <param name="value2">类型二对象</param>
    /// <param name="value3">类型三对象</param>
    /// <param name="value4">类型四对象</param>
    /// <param name="value5">类型五对象</param>
    /// <returns>成的结果对象</returns>
    public static OperateResult<T1, T2, T3, T4, T5> CreateSuccessResult<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
        return new OperateResult<T1, T2, T3, T4, T5>
        {
            IsSuccess = true,
            ErrorCode = 0,
            Message = StringResources.Language.SuccessText,
            Content1 = value1,
            Content2 = value2,
            Content3 = value3,
            Content4 = value4,
            Content5 = value5
        };
    }

    /// <summary>
    /// 创建并返回一个成功的结果对象，并带有六个参数对象
    /// </summary>
    /// <typeparam name="T1">第一个参数类型</typeparam>
    /// <typeparam name="T2">第二个参数类型</typeparam>
    /// <typeparam name="T3">第三个参数类型</typeparam>
    /// <typeparam name="T4">第四个参数类型</typeparam>
    /// <typeparam name="T5">第五个参数类型</typeparam>
    /// <typeparam name="T6">第六个参数类型</typeparam>
    /// <param name="value1">类型一对象</param>
    /// <param name="value2">类型二对象</param>
    /// <param name="value3">类型三对象</param>
    /// <param name="value4">类型四对象</param>
    /// <param name="value5">类型五对象</param>
    /// <param name="value6">类型六对象</param>
    /// <returns>成的结果对象</returns>
    public static OperateResult<T1, T2, T3, T4, T5, T6> CreateSuccessResult<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
        return new OperateResult<T1, T2, T3, T4, T5, T6>
        {
            IsSuccess = true,
            ErrorCode = 0,
            Message = StringResources.Language.SuccessText,
            Content1 = value1,
            Content2 = value2,
            Content3 = value3,
            Content4 = value4,
            Content5 = value5,
            Content6 = value6
        };
    }
}

/// <summary>
/// 操作结果的泛型类，允许带一个用户自定义的泛型对象，推荐使用这个类
/// </summary>
/// <typeparam name="T">泛型类</typeparam>
public class OperateResult<T> : OperateResult
{
    /// <summary>
    /// 用户自定义的泛型数据
    /// </summary>
    public T? Content { get; set; }

    /// <summary>
    /// 实例化一个默认的结果对象
    /// </summary>
    public OperateResult()
    {
    }

    /// <summary>
    /// 使用指定的消息实例化一个默认的结果对象
    /// </summary>
    /// <param name="msg">错误消息</param>
    public OperateResult(string msg)
        : base(msg)
    {
    }

    /// <summary>
    /// 使用错误代码，消息文本来实例化对象
    /// </summary>
    /// <param name="err">错误代码</param>
    /// <param name="msg">错误消息</param>
    public OperateResult(int err, string msg)
        : base(err, msg)
    {
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <param name="message">检查失败的错误消息</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T> Check(Func<T, bool> check, string message = "All content data check failed")
    {
        if (!IsSuccess)
        {
            return this;
        }
        if (check(Content))
        {
            return this;
        }
        return new OperateResult<T>(message);
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T> Check(Func<T, OperateResult> check)
    {
        if (!IsSuccess)
        {
            return this;
        }
        var operateResult = check(Content);
        if (!operateResult.IsSuccess)
        {
            return CreateFailedResult<T>(operateResult);
        }
        return this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult Then(Func<T, OperateResult> func)
    {
        return IsSuccess ? func(Content) : this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult">泛型参数</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult> Then<TResult>(Func<T, OperateResult<TResult>> func)
    {
        return IsSuccess ? func(Content) : CreateFailedResult<TResult>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T, OperateResult<TResult1, TResult2>> func)
    {
        return IsSuccess ? func(Content) : CreateFailedResult<TResult1, TResult2>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T, OperateResult<TResult1, TResult2, TResult3>> func)
    {
        return IsSuccess ? func(Content) : CreateFailedResult<TResult1, TResult2, TResult3>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
    {
        return IsSuccess ? func(Content) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
    {
        return IsSuccess ? func(Content) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <typeparam name="TResult6">泛型参数六</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
    {
        return IsSuccess ? func(Content) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
    }
}

/// <summary>
/// 操作结果的泛型类，允许带两个用户自定义的泛型对象，推荐使用这个类
/// </summary>
/// <typeparam name="T1">泛型类</typeparam>
/// <typeparam name="T2">泛型类</typeparam>
public class OperateResult<T1, T2> : OperateResult
{
    /// <summary>
    /// 用户自定义的泛型数据1
    /// </summary>
    public T1 Content1 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据2
    /// </summary>
    public T2 Content2 { get; set; }

    /// <summary>
    /// 实例化一个默认的结果对象
    /// </summary>
    public OperateResult()
    {
    }

    /// <summary>
    /// 使用指定的消息实例化一个默认的结果对象
    /// </summary>
    /// <param name="msg">错误消息</param>
    public OperateResult(string msg)
        : base(msg)
    {
    }

    /// <summary>
    /// 使用错误代码，消息文本来实例化对象
    /// </summary>
    /// <param name="err">错误代码</param>
    /// <param name="msg">错误消息</param>
    public OperateResult(int err, string msg)
        : base(err, msg)
    {
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <param name="message">可以自由指定的错误信息</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2> Check(Func<T1, T2, bool> check, string message = "All content data check failed")
    {
        if (!IsSuccess)
        {
            return this;
        }
        if (check(Content1, Content2))
        {
            return this;
        }
        return new OperateResult<T1, T2>(message);
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2> Check(Func<T1, T2, OperateResult> check)
    {
        if (!IsSuccess)
        {
            return this;
        }
        var operateResult = check(Content1, Content2);
        if (!operateResult.IsSuccess)
        {
            return CreateFailedResult<T1, T2>(operateResult);
        }
        return this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult Then(Func<T1, T2, OperateResult> func)
    {
        return IsSuccess ? func(Content1, Content2) : this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult">泛型参数</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult> Then<TResult>(Func<T1, T2, OperateResult<TResult>> func)
    {
        return IsSuccess ? func(Content1, Content2) : CreateFailedResult<TResult>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, OperateResult<TResult1, TResult2>> func)
    {
        return IsSuccess ? func(Content1, Content2) : CreateFailedResult<TResult1, TResult2>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3>> func)
    {
        return IsSuccess ? func(Content1, Content2) : CreateFailedResult<TResult1, TResult2, TResult3>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
    {
        return IsSuccess ? func(Content1, Content2) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
    {
        return IsSuccess ? func(Content1, Content2) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <typeparam name="TResult6">泛型参数六</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
    {
        return IsSuccess ? func(Content1, Content2) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
    }
}

/// <summary>
/// 操作结果的泛型类，允许带三个用户自定义的泛型对象，推荐使用这个类
/// </summary>
/// <typeparam name="T1">泛型类</typeparam>
/// <typeparam name="T2">泛型类</typeparam>
/// <typeparam name="T3">泛型类</typeparam>
public class OperateResult<T1, T2, T3> : OperateResult
{
    /// <summary>
    /// 用户自定义的泛型数据1
    /// </summary>
    public T1 Content1 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据2
    /// </summary>
    public T2 Content2 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据3
    /// </summary>
    public T3 Content3 { get; set; }

    /// <summary>
    /// 实例化一个默认的结果对象
    /// </summary>
    public OperateResult()
    {
    }

    /// <summary>
    /// 使用指定的消息实例化一个默认的结果对象
    /// </summary>
    /// <param name="msg">错误消息</param>
    public OperateResult(string msg)
        : base(msg)
    {
    }

    /// <summary>
    /// 使用错误代码，消息文本来实例化对象
    /// </summary>
    /// <param name="err">错误代码</param>
    /// <param name="msg">错误消息</param>
    public OperateResult(int err, string msg)
        : base(err, msg)
    {
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <param name="message">检查失败的错误消息</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2, T3> Check(Func<T1, T2, T3, bool> check, string message = "All content data check failed")
    {
        if (!IsSuccess)
        {
            return this;
        }
        if (check(Content1, Content2, Content3))
        {
            return this;
        }
        return new OperateResult<T1, T2, T3>(message);
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2, T3> Check(Func<T1, T2, T3, OperateResult> check)
    {
        if (!IsSuccess)
        {
            return this;
        }
        var operateResult = check(Content1, Content2, Content3);
        if (!operateResult.IsSuccess)
        {
            return CreateFailedResult<T1, T2, T3>(operateResult);
        }
        return this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult Then(Func<T1, T2, T3, OperateResult> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3) : this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult">泛型参数</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, OperateResult<TResult>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3) : CreateFailedResult<TResult>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, OperateResult<TResult1, TResult2>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3) : CreateFailedResult<TResult1, TResult2>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3) : CreateFailedResult<TResult1, TResult2, TResult3>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <typeparam name="TResult6">泛型参数六</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
    }
}

/// <summary>
/// 操作结果的泛型类，允许带四个用户自定义的泛型对象，推荐使用这个类
/// </summary>
/// <typeparam name="T1">泛型类</typeparam>
/// <typeparam name="T2">泛型类</typeparam>
/// <typeparam name="T3">泛型类</typeparam>
/// <typeparam name="T4">泛型类</typeparam>
public class OperateResult<T1, T2, T3, T4> : OperateResult
{
    /// <summary>
    /// 用户自定义的泛型数据1
    /// </summary>
    public T1 Content1 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据2
    /// </summary>
    public T2 Content2 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据3
    /// </summary>
    public T3 Content3 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据4
    /// </summary>
    public T4 Content4 { get; set; }

    /// <summary>
    /// 实例化一个默认的结果对象
    /// </summary>
    public OperateResult()
    {
    }

    /// <summary>
    /// 使用指定的消息实例化一个默认的结果对象
    /// </summary>
    /// <param name="msg">错误消息</param>
    public OperateResult(string msg)
        : base(msg)
    {
    }

    /// <summary>
    /// 使用错误代码，消息文本来实例化对象
    /// </summary>
    /// <param name="err">错误代码</param>
    /// <param name="msg">错误消息</param>
    public OperateResult(int err, string msg)
        : base(err, msg)
    {
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <param name="message">检查失败的错误消息</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2, T3, T4> Check(Func<T1, T2, T3, T4, bool> check, string message = "All content data check failed")
    {
        if (!IsSuccess)
        {
            return this;
        }
        if (check(Content1, Content2, Content3, Content4))
        {
            return this;
        }
        return new OperateResult<T1, T2, T3, T4>(message);
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
    /// Returns a check result object that allows you to perform custom data checks.
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2, T3, T4> Check(Func<T1, T2, T3, T4, OperateResult> check)
    {
        if (!IsSuccess)
        {
            return this;
        }
        var operateResult = check(Content1, Content2, Content3, Content4);
        if (!operateResult.IsSuccess)
        {
            return CreateFailedResult<T1, T2, T3, T4>(operateResult);
        }
        return this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult Then(Func<T1, T2, T3, T4, OperateResult> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4) : this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult">泛型参数</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, OperateResult<TResult>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4) : CreateFailedResult<TResult>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4) : CreateFailedResult<TResult1, TResult2>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4) : CreateFailedResult<TResult1, TResult2, TResult3>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <typeparam name="TResult6">泛型参数六</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
    }
}

/// <summary>
/// 操作结果的泛型类，允许带五个用户自定义的泛型对象，推荐使用这个类
/// </summary>
/// <typeparam name="T1">泛型类</typeparam>
/// <typeparam name="T2">泛型类</typeparam>
/// <typeparam name="T3">泛型类</typeparam>
/// <typeparam name="T4">泛型类</typeparam>
/// <typeparam name="T5">泛型类</typeparam>
public class OperateResult<T1, T2, T3, T4, T5> : OperateResult
{
    /// <summary>
    /// 用户自定义的泛型数据1
    /// </summary>
    public T1 Content1 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据2
    /// </summary>
    public T2 Content2 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据3
    /// </summary>
    public T3 Content3 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据4
    /// </summary>
    public T4 Content4 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据5
    /// </summary>
    public T5 Content5 { get; set; }

    /// <summary>
    /// 实例化一个默认的结果对象
    /// </summary>
    public OperateResult()
    {
    }

    /// <summary>
    /// 使用指定的消息实例化一个默认的结果对象
    /// </summary>
    /// <param name="msg">错误消息</param>
    public OperateResult(string msg)
        : base(msg)
    {
    }

    /// <summary>
    /// 使用错误代码，消息文本来实例化对象
    /// </summary>
    /// <param name="err">错误代码</param>
    /// <param name="msg">错误消息</param>
    public OperateResult(int err, string msg)
        : base(err, msg)
    {
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <param name="message">检查失败的错误消息</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2, T3, T4, T5> Check(Func<T1, T2, T3, T4, T5, bool> check, string message = "All content data check failed")
    {
        if (!IsSuccess)
        {
            return this;
        }
        if (check(Content1, Content2, Content3, Content4, Content5))
        {
            return this;
        }
        return new OperateResult<T1, T2, T3, T4, T5>(message);
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2, T3, T4, T5> Check(Func<T1, T2, T3, T4, T5, OperateResult> check)
    {
        if (!IsSuccess)
        {
            return this;
        }
        var operateResult = check(Content1, Content2, Content3, Content4, Content5);
        if (!operateResult.IsSuccess)
        {
            return CreateFailedResult<T1, T2, T3, T4, T5>(operateResult);
        }
        return this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult Then(Func<T1, T2, T3, T4, T5, OperateResult> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult">泛型参数</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, T5, OperateResult<TResult>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : CreateFailedResult<TResult>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : CreateFailedResult<TResult1, TResult2>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : CreateFailedResult<TResult1, TResult2, TResult3>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <typeparam name="TResult6">泛型参数六</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
    }
}

/// <summary>
/// 操作结果的泛型类，允许带六个用户自定义的泛型对象，推荐使用这个类
/// </summary>
/// <typeparam name="T1">泛型类</typeparam>
/// <typeparam name="T2">泛型类</typeparam>
/// <typeparam name="T3">泛型类</typeparam>
/// <typeparam name="T4">泛型类</typeparam>
/// <typeparam name="T5">泛型类</typeparam>
/// <typeparam name="T6">泛型类</typeparam>
public class OperateResult<T1, T2, T3, T4, T5, T6> : OperateResult
{
    /// <summary>
    /// 用户自定义的泛型数据1
    /// </summary>
    public T1 Content1 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据2
    /// </summary>
    public T2 Content2 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据3
    /// </summary>
    public T3 Content3 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据4
    /// </summary>
    public T4 Content4 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据5
    /// </summary>
    public T5 Content5 { get; set; }

    /// <summary>
    /// 用户自定义的泛型数据5
    /// </summary>
    public T6 Content6 { get; set; }

    /// <summary>
    /// 实例化一个默认的结果对象
    /// </summary>
    public OperateResult()
    {
    }

    /// <summary>
    /// 使用指定的消息实例化一个默认的结果对象
    /// </summary>
    /// <param name="msg">错误消息</param>
    public OperateResult(string msg)
        : base(msg)
    {
    }

    /// <summary>
    /// 使用错误代码，消息文本来实例化对象
    /// </summary>
    /// <param name="err">错误代码</param>
    /// <param name="msg">错误消息</param>
    public OperateResult(int err, string msg)
        : base(err, msg)
    {
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <param name="message">检查失败的错误消息</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2, T3, T4, T5, T6> Check(Func<T1, T2, T3, T4, T5, T6, bool> check, string message = "All content data check failed")
    {
        if (!IsSuccess)
        {
            return this;
        }
        if (check(Content1, Content2, Content3, Content4, Content5, Content6))
        {
            return this;
        }
        return new OperateResult<T1, T2, T3, T4, T5, T6>(message);
    }

    /// <summary>
    /// 返回一个检查结果对象，可以进行自定义的数据检查。
    /// </summary>
    /// <param name="check">检查的委托方法</param>
    /// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
    public OperateResult<T1, T2, T3, T4, T5, T6> Check(Func<T1, T2, T3, T4, T5, T6, OperateResult> check)
    {
        if (!IsSuccess)
        {
            return this;
        }
        var operateResult = check(Content1, Content2, Content3, Content4, Content5, Content6);
        if (!operateResult.IsSuccess)
        {
            return CreateFailedResult<T1, T2, T3, T4, T5, T6>(operateResult);
        }
        return this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult Then(Func<T1, T2, T3, T4, T5, T6, OperateResult> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : this;
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult">泛型参数</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : CreateFailedResult<TResult>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : CreateFailedResult<TResult1, TResult2>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : CreateFailedResult<TResult1, TResult2, TResult3>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
    }

    /// <summary>
    /// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
    /// </summary>
    /// <typeparam name="TResult1">泛型参数一</typeparam>
    /// <typeparam name="TResult2">泛型参数二</typeparam>
    /// <typeparam name="TResult3">泛型参数三</typeparam>
    /// <typeparam name="TResult4">泛型参数四</typeparam>
    /// <typeparam name="TResult5">泛型参数五</typeparam>
    /// <typeparam name="TResult6">泛型参数六</typeparam>
    /// <param name="func">等待当前对象成功后执行的内容</param>
    /// <returns>返回整个方法链最终的成功失败结果</returns>
    public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
    {
        return IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
    }
}
