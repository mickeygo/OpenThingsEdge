using System.Reflection;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Reflection;

/// <summary>
/// 反射的辅助类
/// </summary>
public class CommReflectionHelper
{
    /// <summary>
    /// 从属性中获取对应的设备类型的地址特性信息
    /// </summary>
    /// <param name="deviceType">设备类型信息</param>
    /// <param name="property">属性信息</param>
    /// <returns>设备类型信息</returns>
    public static CommDeviceAddressAttribute GetHslDeviceAddressAttribute(Type deviceType, PropertyInfo property)
    {
        var customAttributes = property.GetCustomAttributes(typeof(CommDeviceAddressAttribute), inherit: false);
        if (customAttributes == null)
        {
            return null;
        }
        CommDeviceAddressAttribute hslDeviceAddressAttribute = null;
        for (var i = 0; i < customAttributes.Length; i++)
        {
            var hslDeviceAddressAttribute2 = (CommDeviceAddressAttribute)customAttributes[i];
            if (hslDeviceAddressAttribute2.DeviceType != null && hslDeviceAddressAttribute2.DeviceType == deviceType)
            {
                hslDeviceAddressAttribute = hslDeviceAddressAttribute2;
                break;
            }
        }
        if (hslDeviceAddressAttribute == null)
        {
            for (var j = 0; j < customAttributes.Length; j++)
            {
                var hslDeviceAddressAttribute3 = (CommDeviceAddressAttribute)customAttributes[j];
                if (hslDeviceAddressAttribute3.DeviceType == null)
                {
                    hslDeviceAddressAttribute = hslDeviceAddressAttribute3;
                    break;
                }
            }
        }
        return hslDeviceAddressAttribute;
    }

    /// <inheritdoc cref="M:HslCommunication.Reflection.HslReflectionHelper.GetHslDeviceAddressAttribute(System.Type,System.Reflection.PropertyInfo)" />
    public static CommDeviceAddressAttribute[] GetHslDeviceAddressAttributeArray(Type deviceType, PropertyInfo property)
    {
        var customAttributes = property.GetCustomAttributes(typeof(CommDeviceAddressAttribute), inherit: false);
        if (customAttributes == null)
        {
            return null;
        }
        var list = new List<CommDeviceAddressAttribute>();
        for (var i = 0; i < customAttributes.Length; i++)
        {
            var hslDeviceAddressAttribute = (CommDeviceAddressAttribute)customAttributes[i];
            if (hslDeviceAddressAttribute.DeviceType != null && hslDeviceAddressAttribute.DeviceType == deviceType)
            {
                list.Add(hslDeviceAddressAttribute);
            }
        }
        if (list.Count == 0)
        {
            for (var j = 0; j < customAttributes.Length; j++)
            {
                var hslDeviceAddressAttribute2 = (CommDeviceAddressAttribute)customAttributes[j];
                if (hslDeviceAddressAttribute2.DeviceType == null)
                {
                    list.Add(hslDeviceAddressAttribute2);
                }
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// 根据类型信息，直接从原始字节解析出类型对象，然后赋值给对应的对象，该对象的属性需要支持特性 <see cref="T:HslCommunication.Reflection.HslStructAttribute" /> 才支持设置
    /// </summary>
    /// <typeparam name="T">类型信息</typeparam>
    /// <param name="buffer">缓存信息</param>
    /// <param name="startIndex">起始偏移地址</param>
    /// <param name="byteTransform">数据变换规则对象</param>
    /// <returns>新的实例化的类型对象</returns>
    public static T PraseStructContent<T>(byte[] buffer, int startIndex, IByteTransform byteTransform) where T : class, new()
    {
        var typeFromHandle = typeof(T);
        var obj = typeFromHandle.Assembly.CreateInstance(typeFromHandle.FullName);
        PraseStructContent(obj, buffer, startIndex, byteTransform);
        return (T)obj;
    }

    /// <summary>
    /// 根据结构体的定义，将原始字节的数据解析出来，然后赋值给对应的对象，该对象的属性需要支持特性 <see cref="T:HslCommunication.Reflection.HslStructAttribute" /> 才支持设置
    /// </summary>
    /// <param name="obj">类型对象信息</param>
    /// <param name="buffer">读取的缓存数据信息</param>
    /// <param name="startIndex">起始的偏移地址</param>
    /// <param name="byteTransform">数据变换规则对象</param>
    public static void PraseStructContent(object obj, byte[] buffer, int startIndex, IByteTransform byteTransform)
    {
        var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var array = properties;
        foreach (var propertyInfo in array)
        {
            var customAttributes = propertyInfo.GetCustomAttributes(typeof(CommStructAttribute), inherit: false);
            if (customAttributes == null)
            {
                continue;
            }
            var hslStructAttribute = customAttributes.Length != 0 ? (CommStructAttribute)customAttributes[0] : null;
            if (hslStructAttribute == null)
            {
                continue;
            }
            var propertyType = propertyInfo.PropertyType;
            if (propertyType == typeof(byte))
            {
                propertyInfo.SetValue(obj, buffer[startIndex + hslStructAttribute.Index], null);
            }
            else if (propertyType == typeof(byte[]))
            {
                propertyInfo.SetValue(obj, buffer.SelectMiddle(startIndex + hslStructAttribute.Index, hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(short))
            {
                propertyInfo.SetValue(obj, byteTransform.TransInt16(buffer, startIndex + hslStructAttribute.Index), null);
            }
            else if (propertyType == typeof(short[]))
            {
                propertyInfo.SetValue(obj, byteTransform.TransInt16(buffer, startIndex + hslStructAttribute.Index, hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(ushort))
            {
                propertyInfo.SetValue(obj, byteTransform.TransUInt16(buffer, startIndex + hslStructAttribute.Index), null);
            }
            else if (propertyType == typeof(ushort[]))
            {
                propertyInfo.SetValue(obj, byteTransform.TransUInt16(buffer, startIndex + hslStructAttribute.Index, hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(int))
            {
                propertyInfo.SetValue(obj, byteTransform.TransInt32(buffer, startIndex + hslStructAttribute.Index), null);
            }
            else if (propertyType == typeof(int[]))
            {
                propertyInfo.SetValue(obj, byteTransform.TransInt32(buffer, startIndex + hslStructAttribute.Index, hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(uint))
            {
                propertyInfo.SetValue(obj, byteTransform.TransUInt32(buffer, startIndex + hslStructAttribute.Index), null);
            }
            else if (propertyType == typeof(uint[]))
            {
                propertyInfo.SetValue(obj, byteTransform.TransUInt32(buffer, startIndex + hslStructAttribute.Index, hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(long))
            {
                propertyInfo.SetValue(obj, byteTransform.TransInt64(buffer, startIndex + hslStructAttribute.Index), null);
            }
            else if (propertyType == typeof(long[]))
            {
                propertyInfo.SetValue(obj, byteTransform.TransInt64(buffer, startIndex + hslStructAttribute.Index, hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(ulong))
            {
                propertyInfo.SetValue(obj, byteTransform.TransUInt64(buffer, startIndex + hslStructAttribute.Index), null);
            }
            else if (propertyType == typeof(ulong[]))
            {
                propertyInfo.SetValue(obj, byteTransform.TransUInt64(buffer, startIndex + hslStructAttribute.Index, hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(float))
            {
                propertyInfo.SetValue(obj, byteTransform.TransSingle(buffer, startIndex + hslStructAttribute.Index), null);
            }
            else if (propertyType == typeof(float[]))
            {
                propertyInfo.SetValue(obj, byteTransform.TransSingle(buffer, startIndex + hslStructAttribute.Index, hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(double))
            {
                propertyInfo.SetValue(obj, byteTransform.TransDouble(buffer, startIndex + hslStructAttribute.Index), null);
            }
            else if (propertyType == typeof(double[]))
            {
                propertyInfo.SetValue(obj, byteTransform.TransDouble(buffer, startIndex + hslStructAttribute.Index, hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(string))
            {
                var uTF = Encoding.UTF8;
                propertyInfo.SetValue(obj, byteTransform.TransString(encoding: hslStructAttribute.Encoding.Equals("ASCII", StringComparison.OrdinalIgnoreCase) ? Encoding.ASCII : hslStructAttribute.Encoding.Equals("UNICODE", StringComparison.OrdinalIgnoreCase) ? Encoding.Unicode : hslStructAttribute.Encoding.Equals("ANSI", StringComparison.OrdinalIgnoreCase) ? Encoding.Default : hslStructAttribute.Encoding.Equals("UTF8", StringComparison.OrdinalIgnoreCase) ? Encoding.UTF8 : hslStructAttribute.Encoding.Equals("BIG-UNICODE", StringComparison.OrdinalIgnoreCase) ? Encoding.BigEndianUnicode : !hslStructAttribute.Encoding.Equals("GB2312", StringComparison.OrdinalIgnoreCase) ? Encoding.GetEncoding(hslStructAttribute.Encoding) : Encoding.GetEncoding("GB2312"), buffer: buffer, index: startIndex + hslStructAttribute.Index, length: hslStructAttribute.Length), null);
            }
            else if (propertyType == typeof(bool))
            {
                propertyInfo.SetValue(obj, buffer.GetBoolByIndex(startIndex * 8 + hslStructAttribute.Index), null);
            }
            else if (propertyType == typeof(bool[]))
            {
                var array2 = new bool[hslStructAttribute.Length];
                for (var j = 0; j < array2.Length; j++)
                {
                    array2[j] = buffer.GetBoolByIndex(startIndex * 8 + hslStructAttribute.Index + j);
                }
                propertyInfo.SetValue(obj, array2, null);
            }
        }
    }

    /// <summary>
    /// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="T:HslCommunication.Reflection.HslDeviceAddressAttribute" />，详细参考论坛的操作说明。
    /// </summary>
    /// <typeparam name="T">自定义的数据类型对象</typeparam>
    /// <param name="readWrite">读写接口的实现</param>
    /// <returns>包含是否成功的结果对象</returns>
    public static async Task<OperateResult<T>> ReadAsync<T>(IReadWriteNet readWrite) where T : class, new()
    {
        var type = typeof(T);
        var obj = type.Assembly.CreateInstance(type.FullName);
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var array = properties;
        foreach (var property in array)
        {
            var propertyType = property.PropertyType;
            if (propertyType == typeof(string[]))
            {
                var hslAttributes = GetHslDeviceAddressAttributeArray(readWrite.GetType(), property);
                if (hslAttributes == null || hslAttributes.Length == 0)
                {
                    continue;
                }
                var strings = new string[hslAttributes.Length];
                for (var i = 0; i < hslAttributes.Length; i++)
                {
                    var valueResult = await readWrite.ReadStringAsync(hslAttributes[i].Address, (ushort)(hslAttributes[i].Length < 0 ? 1u : (uint)hslAttributes[i].Length), hslAttributes[i].GetEncoding());
                    if (!valueResult.IsSuccess)
                    {
                        return OperateResult.CreateFailedResult<T>(valueResult);
                    }
                    strings[i] = valueResult.Content;
                }
                property.SetValue(obj, strings, null);
                continue;
            }
            var attribute = property.GetCustomAttributes(typeof(CommDeviceAddressAttribute), inherit: false);
            if (attribute == null)
            {
                continue;
            }
            var hslAttribute = GetHslDeviceAddressAttribute(readWrite.GetType(), property);
            if (hslAttribute == null)
            {
                continue;
            }
            if (propertyType == typeof(byte))
            {
                var readByteMethod = readWrite.GetType().GetMethod("ReadByteAsync", new Type[1] { typeof(string) });
                if (readByteMethod == null)
                {
                    return new OperateResult<T>(readWrite.GetType().Name + " not support read byte value. ");
                }
                if (!(readByteMethod.Invoke(readWrite, new object[1] { hslAttribute.Address }) is Task readByteTask))
                {
                    return new OperateResult<T>(readWrite.GetType().Name + " not task type result. ");
                }
                await readByteTask;
                var valueResult = readByteTask.GetType().GetProperty("Result").GetValue(readByteTask, null) as OperateResult<byte>;
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(short))
            {
                var valueResult = await readWrite.ReadInt16Async(hslAttribute.Address);
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(short[]))
            {
                var valueResult = await readWrite.ReadInt16Async(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(ushort))
            {
                var valueResult = await readWrite.ReadUInt16Async(hslAttribute.Address);
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(ushort[]))
            {
                var valueResult = await readWrite.ReadUInt16Async(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(int))
            {
                var valueResult = await readWrite.ReadInt32Async(hslAttribute.Address);
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(int[]))
            {
                var valueResult = await readWrite.ReadInt32Async(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(uint))
            {
                var valueResult = await readWrite.ReadUInt32Async(hslAttribute.Address);
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(uint[]))
            {
                var valueResult = await readWrite.ReadUInt32Async(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(long))
            {
                var valueResult = await readWrite.ReadInt64Async(hslAttribute.Address);
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(long[]))
            {
                var valueResult = await readWrite.ReadInt64Async(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(ulong))
            {
                var valueResult = await readWrite.ReadUInt64Async(hslAttribute.Address);
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(ulong[]))
            {
                var valueResult = await readWrite.ReadUInt64Async(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(float))
            {
                var valueResult = await readWrite.ReadFloatAsync(hslAttribute.Address);
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(float[]))
            {
                var valueResult = await readWrite.ReadFloatAsync(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(double))
            {
                var valueResult = await readWrite.ReadDoubleAsync(hslAttribute.Address);
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(double[]))
            {
                var valueResult = await readWrite.ReadDoubleAsync(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(string))
            {
                var valueResult = await readWrite.ReadStringAsync(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length), hslAttribute.GetEncoding());
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(byte[]))
            {
                var valueResult = await readWrite.ReadAsync(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(bool))
            {
                var valueResult = await readWrite.ReadBoolAsync(hslAttribute.Address);
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
            else if (propertyType == typeof(bool[]))
            {
                var valueResult = await readWrite.ReadBoolAsync(hslAttribute.Address, (ushort)(hslAttribute.Length < 0 ? 1u : (uint)hslAttribute.Length));
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
                property.SetValue(obj, valueResult.Content, null);
            }
        }
        return OperateResult.CreateSuccessResult((T)obj);
    }

    /// <summary>
    /// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="T:HslCommunication.Reflection.HslDeviceAddressAttribute" />，详细参考论坛的操作说明。
    /// </summary>
    /// <typeparam name="T">自定义的数据类型对象</typeparam>
    /// <param name="data">自定义的数据对象</param>
    /// <param name="readWrite">数据读写对象</param>
    /// <returns>包含是否成功的结果对象</returns>
    /// <exception cref="T:System.ArgumentNullException"></exception>
    public static async Task<OperateResult> WriteAsync<T>(T data, IReadWriteNet readWrite) where T : class, new()
    {
        if (data == null)
        {
            throw new ArgumentNullException("data");
        }
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var array = properties;
        foreach (var property in array)
        {
            var propertyType = property.PropertyType;
            if (propertyType == typeof(string[]))
            {
                var hslAttributes = GetHslDeviceAddressAttributeArray(readWrite.GetType(), property);
                if (hslAttributes == null || hslAttributes.Length == 0)
                {
                    continue;
                }
                var strings = (string[])property.GetValue(data, null);
                for (var i = 0; i < hslAttributes.Length; i++)
                {
                    var writeResult = await readWrite.WriteAsync(hslAttributes[i].Address, strings[i], hslAttributes[i].GetEncoding());
                    if (!writeResult.IsSuccess)
                    {
                        return writeResult;
                    }
                }
                continue;
            }
            var attribute = property.GetCustomAttributes(typeof(CommDeviceAddressAttribute), inherit: false);
            if (attribute == null)
            {
                continue;
            }
            var hslAttribute = GetHslDeviceAddressAttribute(readWrite.GetType(), property);
            if (hslAttribute == null)
            {
                continue;
            }
            if (propertyType == typeof(byte))
            {
                var method = readWrite.GetType().GetMethod("WriteAsync", new Type[2]
                {
                    typeof(string),
                    typeof(byte)
                });
                if (method == null)
                {
                    return new OperateResult<T>(readWrite.GetType().Name + " not support write byte value. ");
                }
                var value = (byte)property.GetValue(data, null);
                if (!(method.Invoke(readWrite, new object[2] { hslAttribute.Address, value }) is Task writeTask))
                {
                    return new OperateResult(readWrite.GetType().Name + " not task type result. ");
                }
                await writeTask;
                var valueResult = writeTask.GetType().GetProperty("Result").GetValue(writeTask, null) as OperateResult;
                if (!valueResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<T>(valueResult);
                }
            }
            else if (propertyType == typeof(short))
            {
                var writeResult = await readWrite.WriteAsync(value: (short)property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(short[]))
            {
                var writeResult = await readWrite.WriteAsync(values: (short[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(ushort))
            {
                var writeResult = await readWrite.WriteAsync(value: (ushort)property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(ushort[]))
            {
                var writeResult = await readWrite.WriteAsync(values: (ushort[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(int))
            {
                var writeResult = await readWrite.WriteAsync(value: (int)property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(int[]))
            {
                var writeResult = await readWrite.WriteAsync(values: (int[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(uint))
            {
                var writeResult = await readWrite.WriteAsync(value: (uint)property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(uint[]))
            {
                var writeResult = await readWrite.WriteAsync(values: (uint[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(long))
            {
                var writeResult = await readWrite.WriteAsync(value: (long)property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(long[]))
            {
                var writeResult = await readWrite.WriteAsync(values: (long[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(ulong))
            {
                var writeResult = await readWrite.WriteAsync(value: (ulong)property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(ulong[]))
            {
                var writeResult = await readWrite.WriteAsync(values: (ulong[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(float))
            {
                var writeResult = await readWrite.WriteAsync(value: (float)property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(float[]))
            {
                var writeResult = await readWrite.WriteAsync(values: (float[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(double))
            {
                var writeResult = await readWrite.WriteAsync(value: (double)property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(double[]))
            {
                var writeResult = await readWrite.WriteAsync(values: (double[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(string))
            {
                var writeResult = await readWrite.WriteAsync(value: (string)property.GetValue(data, null), address: hslAttribute.Address, encoding: hslAttribute.GetEncoding());
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(byte[]))
            {
                var writeResult = await readWrite.WriteAsync(value: (byte[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(bool))
            {
                var writeResult = await readWrite.WriteAsync(value: (bool)property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
            else if (propertyType == typeof(bool[]))
            {
                var writeResult = await readWrite.WriteAsync(value: (bool[])property.GetValue(data, null), address: hslAttribute.Address);
                if (!writeResult.IsSuccess)
                {
                    return writeResult;
                }
            }
        }
        return OperateResult.CreateSuccessResult(data);
    }

    /// <summary>
    /// 根据提供的类型对象，解析出符合 <see cref="T:HslCommunication.Reflection.HslDeviceAddressAttribute" /> 特性的地址列表
    /// </summary>
    /// <param name="valueType">数据类型</param>
    /// <param name="deviceType">设备类型</param>
    /// <param name="obj">类型的对象信息</param>
    /// <param name="byteTransform">数据变换对象</param>
    /// <returns>地址列表信息</returns>
    public static List<CommAddressProperty> GetHslPropertyInfos(Type valueType, Type deviceType, object obj, IByteTransform byteTransform)
    {
        var list = new List<CommAddressProperty>();
        var properties = valueType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var num = 0;
        var array = properties;
        foreach (var propertyInfo in array)
        {
            var hslDeviceAddressAttribute = GetHslDeviceAddressAttribute(deviceType, propertyInfo);
            if (hslDeviceAddressAttribute == null)
            {
                continue;
            }
            var hslAddressProperty = new CommAddressProperty();
            hslAddressProperty.PropertyInfo = propertyInfo;
            hslAddressProperty.DeviceAddressAttribute = hslDeviceAddressAttribute;
            hslAddressProperty.ByteOffset = num;
            var propertyType = propertyInfo.PropertyType;
            if (propertyType == typeof(byte))
            {
                num++;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = new byte[1] { (byte)propertyInfo.GetValue(obj, null) };
                }
            }
            else if (propertyType == typeof(short))
            {
                num += 2;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((short)propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(short[]))
            {
                num += 2 * (hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length);
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((short[])propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(ushort))
            {
                num += 2;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((ushort)propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(ushort[]))
            {
                num += 2 * (hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length);
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((ushort[])propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(int))
            {
                num += 4;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((int)propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(int[]))
            {
                num += 4 * (hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length);
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((int[])propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(uint))
            {
                num += 4;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((uint)propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(uint[]))
            {
                num += 4 * (hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length);
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((uint[])propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(long))
            {
                num += 8;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((long)propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(long[]))
            {
                num += 8 * (hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length);
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((long[])propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(ulong))
            {
                num += 8;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((ulong)propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(ulong[]))
            {
                num += 8 * (hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length);
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((ulong[])propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(float))
            {
                num += 4;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((float)propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(float[]))
            {
                num += 4 * (hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length);
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((float[])propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(double))
            {
                num += 8;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((double)propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(double[]))
            {
                num += 8 * (hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length);
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((double[])propertyInfo.GetValue(obj, null));
                }
            }
            else if (propertyType == typeof(string))
            {
                num += hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = byteTransform.TransByte((string)propertyInfo.GetValue(obj, null), Encoding.ASCII);
                }
            }
            else if (propertyType == typeof(byte[]))
            {
                num += hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = (byte[])propertyInfo.GetValue(obj, null);
                }
            }
            else if (propertyType == typeof(bool))
            {
                num++;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = !(bool)propertyInfo.GetValue(obj, null) ? new byte[1] : new byte[1] { 1 };
                }
            }
            else if (propertyType == typeof(bool[]))
            {
                num += hslDeviceAddressAttribute.Length <= 0 ? 1 : hslDeviceAddressAttribute.Length;
                if (obj != null)
                {
                    hslAddressProperty.Buffer = ((bool[])propertyInfo.GetValue(obj, null)).Select((m) => (byte)(m ? 1 : 0)).ToArray();
                }
            }
            hslAddressProperty.ByteLength = num - hslAddressProperty.ByteOffset;
            list.Add(hslAddressProperty);
        }
        return list;
    }

    /// <summary>
    /// 根据地址列表信息，数据缓存，自动解析基础类型的数据，赋值到自定义的对象上去
    /// </summary>
    /// <param name="byteTransform">数据解析对象</param>
    /// <param name="obj">数据对象信息</param>
    /// <param name="properties">地址属性列表</param>
    /// <param name="buffer">缓存数据信息</param>
    public static void SetPropertyValueFrom(IByteTransform byteTransform, object obj, List<CommAddressProperty> properties, byte[] buffer)
    {
        foreach (var property in properties)
        {
            var propertyType = property.PropertyInfo.PropertyType;
            object obj2 = null;
            if (propertyType == typeof(byte))
            {
                obj2 = buffer[property.ByteOffset];
            }
            else if (propertyType == typeof(short))
            {
                obj2 = byteTransform.TransInt16(buffer, property.ByteOffset);
            }
            else if (propertyType == typeof(short[]))
            {
                obj2 = byteTransform.TransInt16(buffer, property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(ushort))
            {
                obj2 = byteTransform.TransUInt16(buffer, property.ByteOffset);
            }
            else if (propertyType == typeof(ushort[]))
            {
                obj2 = byteTransform.TransUInt16(buffer, property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(int))
            {
                obj2 = byteTransform.TransInt32(buffer, property.ByteOffset);
            }
            else if (propertyType == typeof(int[]))
            {
                obj2 = byteTransform.TransInt32(buffer, property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(uint))
            {
                obj2 = byteTransform.TransUInt32(buffer, property.ByteOffset);
            }
            else if (propertyType == typeof(uint[]))
            {
                obj2 = byteTransform.TransUInt32(buffer, property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(long))
            {
                obj2 = byteTransform.TransInt64(buffer, property.ByteOffset);
            }
            else if (propertyType == typeof(long[]))
            {
                obj2 = byteTransform.TransInt64(buffer, property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(ulong))
            {
                obj2 = byteTransform.TransUInt64(buffer, property.ByteOffset);
            }
            else if (propertyType == typeof(ulong[]))
            {
                obj2 = byteTransform.TransUInt64(buffer, property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(float))
            {
                obj2 = byteTransform.TransSingle(buffer, property.ByteOffset);
            }
            else if (propertyType == typeof(float[]))
            {
                obj2 = byteTransform.TransSingle(buffer, property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(double))
            {
                obj2 = byteTransform.TransDouble(buffer, property.ByteOffset);
            }
            else if (propertyType == typeof(double[]))
            {
                obj2 = byteTransform.TransDouble(buffer, property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(string))
            {
                obj2 = Encoding.ASCII.GetString(buffer, property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(byte[]))
            {
                obj2 = buffer.SelectMiddle(property.ByteOffset, property.DeviceAddressAttribute.GetDataLength());
            }
            else if (propertyType == typeof(bool))
            {
                obj2 = buffer[property.ByteOffset] != 0;
            }
            else if (propertyType == typeof(bool[]))
            {
                obj2 = (from m in buffer.SelectMiddle(property.ByteOffset, property.DeviceAddressAttribute.GetDataLength())
                        select m != 0).ToArray();
            }
            if (obj2 != null)
            {
                property.PropertyInfo.SetValue(obj, obj2, null);
            }
        }
    }
}
