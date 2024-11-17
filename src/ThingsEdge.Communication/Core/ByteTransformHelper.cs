using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core;

/// <summary>
/// 所有数据转换类的静态辅助方法。
/// </summary>
public static class ByteTransformHelper
{
    /// <summary>
    /// 结果转换操作的基础方法，需要支持类型，及转换的委托，并捕获转换时的异常方法
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="result">源</param>
    /// <param name="translator">实际转换的委托</param>
    /// <returns>转换结果</returns>
    public static OperateResult<TResult> GetResultFromBytes<TResult>(OperateResult<byte[]> result, Func<byte[], TResult> translator)
    {
        try
        {
            if (result.IsSuccess)
            {
                return OperateResult.CreateSuccessResult(translator(result.Content!));
            }
            return OperateResult.CreateFailedResult<TResult>(result);
        }
        catch (Exception ex)
        {
            var operateResult = new OperateResult<TResult>
            {
                Message = $"{StringResources.Language.DataTransformError} {SoftBasic.ByteToHexString(result.Content ?? [])} : Length({result.Content?.Length}) {ex.Message}"
            };
            return operateResult;
        }
    }

    /// <summary>
    /// 结果转换操作的基础方法，需要支持类型，及转换的委托
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="result">源结果</param>
    /// <returns>转换结果</returns>
    public static OperateResult<TResult> GetResultFromArray<TResult>(OperateResult<TResult[]> result)
    {
        return GetSuccessResultFromOther(result, (m) => m[0]);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容，所转换的规则
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans">转换方法，从类型TIn转换拿到TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetSuccessResultFromOther<TResult, TIn>(OperateResult<TIn> result, Func<TIn, TResult> trans)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }

        try
        {
            var value = trans(result.Content!);
            return OperateResult.CreateSuccessResult(value);
        }
        catch (Exception ex)
        {
            return new OperateResult<TResult>(StringResources.Language.DataTransformError + " " + ex.Message);
        }
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans">转换方法，从类型TIn转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult GetResultFromOther<TIn>(OperateResult<TIn> result, Func<TIn, OperateResult> trans)
    {
        if (!result.IsSuccess)
        {
            return result;
        }

        return trans(result.Content!);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans">转换方法，从类型TIn转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetResultFromOther<TResult, TIn>(OperateResult<TIn> result, Func<TIn, OperateResult<TResult>> trans)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }

        return trans(result.Content!);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn1">输入类型1</typeparam>
    /// <typeparam name="TIn2">输入类型2</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans1">转换方法1，从类型TIn1转换拿到OperateResult的TIn2的泛型委托</param>
    /// <param name="trans2">转换方法2，从类型TIn2转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetResultFromOther<TResult, TIn1, TIn2>(OperateResult<TIn1> result, Func<TIn1, OperateResult<TIn2>> trans1, Func<TIn2, OperateResult<TResult>> trans2)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }

        var operateResult = trans1(result.Content!);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult);
        }
        return trans2(operateResult.Content!);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn1">输入类型1</typeparam>
    /// <typeparam name="TIn2">输入类型2</typeparam>
    /// <typeparam name="TIn3">输入类型3</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans1">转换方法1，从类型TIn1转换拿到OperateResult的TIn2的泛型委托</param>
    /// <param name="trans2">转换方法2，从类型TIn2转换拿到OperateResult的TIn3的泛型委托</param>
    /// <param name="trans3">转换方法3，从类型TIn3转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetResultFromOther<TResult, TIn1, TIn2, TIn3>(OperateResult<TIn1> result, Func<TIn1, OperateResult<TIn2>> trans1, Func<TIn2, OperateResult<TIn3>> trans2, Func<TIn3, OperateResult<TResult>> trans3)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }

        var operateResult = trans1(result.Content!);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult);
        }

        var operateResult2 = trans2(operateResult.Content!);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult2);
        }
        return trans3(operateResult2.Content!);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn1">输入类型1</typeparam>
    /// <typeparam name="TIn2">输入类型2</typeparam>
    /// <typeparam name="TIn3">输入类型3</typeparam>
    /// <typeparam name="TIn4">输入类型4</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans1">转换方法1，从类型TIn1转换拿到OperateResult的TIn2的泛型委托</param>
    /// <param name="trans2">转换方法2，从类型TIn2转换拿到OperateResult的TIn3的泛型委托</param>
    /// <param name="trans3">转换方法3，从类型TIn3转换拿到OperateResult的TIn4的泛型委托</param>
    /// <param name="trans4">转换方法4，从类型TIn4转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetResultFromOther<TResult, TIn1, TIn2, TIn3, TIn4>(OperateResult<TIn1> result, Func<TIn1, OperateResult<TIn2>> trans1, Func<TIn2, OperateResult<TIn3>> trans2, Func<TIn3, OperateResult<TIn4>> trans3, Func<TIn4, OperateResult<TResult>> trans4)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }
        var operateResult = trans1(result.Content!);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult);
        }
        var operateResult2 = trans2(operateResult.Content!);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult2);
        }
        var operateResult3 = trans3(operateResult2.Content!);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult3);
        }
        return trans4(operateResult3.Content!);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn1">输入类型1</typeparam>
    /// <typeparam name="TIn2">输入类型2</typeparam>
    /// <typeparam name="TIn3">输入类型3</typeparam>
    /// <typeparam name="TIn4">输入类型4</typeparam>
    /// <typeparam name="TIn5">输入类型5</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans1">转换方法1，从类型TIn1转换拿到OperateResult的TIn2的泛型委托</param>
    /// <param name="trans2">转换方法2，从类型TIn2转换拿到OperateResult的TIn3的泛型委托</param>
    /// <param name="trans3">转换方法3，从类型TIn3转换拿到OperateResult的TIn4的泛型委托</param>
    /// <param name="trans4">转换方法4，从类型TIn4转换拿到OperateResult的TIn5的泛型委托</param>
    /// <param name="trans5">转换方法5，从类型TIn5转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetResultFromOther<TResult, TIn1, TIn2, TIn3, TIn4, TIn5>(OperateResult<TIn1> result, Func<TIn1, OperateResult<TIn2>> trans1, Func<TIn2, OperateResult<TIn3>> trans2, Func<TIn3, OperateResult<TIn4>> trans3, Func<TIn4, OperateResult<TIn5>> trans4, Func<TIn5, OperateResult<TResult>> trans5)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }

        var operateResult = trans1(result.Content!);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult);
        }

        var operateResult2 = trans2(operateResult.Content!);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult2);
        }

        var operateResult3 = trans3(operateResult2.Content!);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult3);
        }

        var operateResult4 = trans4(operateResult3.Content!);
        if (!operateResult4.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult4);
        }
        return trans5(operateResult4.Content!);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn1">输入类型1</typeparam>
    /// <typeparam name="TIn2">输入类型2</typeparam>
    /// <typeparam name="TIn3">输入类型3</typeparam>
    /// <typeparam name="TIn4">输入类型4</typeparam>
    /// <typeparam name="TIn5">输入类型5</typeparam>
    /// <typeparam name="TIn6">输入类型6</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans1">转换方法1，从类型TIn1转换拿到OperateResult的TIn2的泛型委托</param>
    /// <param name="trans2">转换方法2，从类型TIn2转换拿到OperateResult的TIn3的泛型委托</param>
    /// <param name="trans3">转换方法3，从类型TIn3转换拿到OperateResult的TIn4的泛型委托</param>
    /// <param name="trans4">转换方法4，从类型TIn4转换拿到OperateResult的TIn5的泛型委托</param>
    /// <param name="trans5">转换方法5，从类型TIn5转换拿到OperateResult的TIn6的泛型委托</param>
    /// <param name="trans6">转换方法6，从类型TIn6转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetResultFromOther<TResult, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(OperateResult<TIn1> result, Func<TIn1, OperateResult<TIn2>> trans1, Func<TIn2, OperateResult<TIn3>> trans2, Func<TIn3, OperateResult<TIn4>> trans3, Func<TIn4, OperateResult<TIn5>> trans4, Func<TIn5, OperateResult<TIn6>> trans5, Func<TIn6, OperateResult<TResult>> trans6)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }

        var operateResult = trans1(result.Content!);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult);
        }

        var operateResult2 = trans2(operateResult.Content!);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult2);
        }

        var operateResult3 = trans3(operateResult2.Content!);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult3);
        }

        var operateResult4 = trans4(operateResult3.Content!);
        if (!operateResult4.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult4);
        }

        var operateResult5 = trans5(operateResult4.Content!);
        if (!operateResult5.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult5);
        }
        return trans6(operateResult5.Content!);
    }
}
