using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.BasicFramework;

/// <summary>
/// 一个线程安全的缓存数据块，支持批量动态修改，添加，并获取快照<br />
/// A thread-safe cache data block that supports batch dynamic modification, addition, and snapshot acquisition
/// </summary>
/// <remarks>
/// 这个类可以实现什么功能呢，就是你有一个大的数组，作为你的应用程序的中间数据池，允许你往byte[]数组里存放指定长度的子byte[]数组，也允许从里面拿数据，
/// 这些操作都是线程安全的，当然，本类扩展了一些额外的方法支持，也可以直接赋值或获取基本的数据类型对象。
/// </remarks>
/// <example>
/// 此处举例一些数据的读写说明，可以此处的数据示例。
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\BasicFramework\SoftBufferExample.cs" region="SoftBufferExample1" title="SoftBuffer示例" />
/// </example>
public class SoftBuffer : IDisposable
{
    private int capacity = 10;

    private byte[] buffer;

    private SimpleHybirdLock hybirdLock;

    private IByteTransform byteTransform;

    private bool isBoolReverseByWord = false;

    private bool disposedValue = false;

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.ByteTransform" />
    public IByteTransform ByteTransform
    {
        get
        {
            return byteTransform;
        }
        set
        {
            byteTransform = value;
        }
    }

    /// <summary>
    /// 获取或设置当前的bool操作是否按照字节反转<br />
    /// Gets or sets whether the current bool operation is reversed by bytes
    /// </summary>
    public bool IsBoolReverseByWord
    {
        get
        {
            return isBoolReverseByWord;
        }
        set
        {
            isBoolReverseByWord = value;
        }
    }

    /// <summary>
    /// 使用默认的大小(10个字节)初始化缓存空间<br />
    /// Initialize the cache space with the default size (10 bytes).
    /// </summary>
    public SoftBuffer()
    {
        buffer = new byte[capacity];
        hybirdLock = new SimpleHybirdLock();
        byteTransform = new RegularByteTransform();
    }

    /// <summary>
    /// 使用指定的容量初始化缓存数据块，长度以字节为单位<br />
    /// Initializes cache chunks with the specified capacity, with a length in bytes
    /// </summary>
    /// <param name="capacity">初始化的容量</param>
    public SoftBuffer(int capacity)
    {
        buffer = new byte[capacity];
        this.capacity = capacity;
        hybirdLock = new SimpleHybirdLock();
        byteTransform = new RegularByteTransform();
    }

    /// <summary>
    /// 设置指定的位置bool值，如果超出，则丢弃数据，该位置是指按照位为单位排序的<br />
    /// Set the bool value at the specified position, if it is exceeded, 
    /// the data is discarded, the position refers to sorting in units of bits
    /// </summary>
    /// <param name="value">bool值</param>
    /// <param name="destIndex">目标存储的索引</param>
    /// <exception cref="T:System.IndexOutOfRangeException"></exception>
    public void SetBool(bool value, int destIndex)
    {
        SetBool(new bool[1] { value }, destIndex);
    }

    /// <summary>
    /// 设置指定的位置的bool数组，如果超出，则丢弃数据，该位置是指按照位为单位排序的<br />
    /// Set the bool array at the specified position, if it is exceeded, 
    /// the data is discarded, the position refers to sorting in units of bits
    /// </summary>
    /// <param name="value">bool数组值</param>
    /// <param name="destIndex">目标存储的索引</param>
    /// <exception cref="T:System.IndexOutOfRangeException"></exception>
    public void SetBool(bool[] value, int destIndex)
    {
        if (value == null)
        {
            return;
        }
        try
        {
            hybirdLock.Enter();
            for (var i = 0; i < value.Length; i++)
            {
                var num = (destIndex + i) / 8;
                var offset = (destIndex + i) % 8;
                if (isBoolReverseByWord)
                {
                    num = num % 2 != 0 ? num - 1 : num + 1;
                }
                if (value[i])
                {
                    buffer[num] |= getOrByte(offset);
                }
                else
                {
                    buffer[num] &= getAndByte(offset);
                }
            }
            hybirdLock.Leave();
        }
        catch
        {
            hybirdLock.Leave();
            throw;
        }
    }

    /// <summary>
    /// 获取指定的位置的bool值，如果超出，则引发异常<br />
    /// Get the bool value at the specified position, if it exceeds, an exception is thrown
    /// </summary>
    /// <param name="destIndex">目标存储的索引</param>
    /// <returns>获取索引位置的bool数据值</returns>
    /// <exception cref="T:System.IndexOutOfRangeException"></exception>
    public bool GetBool(int destIndex)
    {
        return GetBool(destIndex, 1)[0];
    }

    /// <summary>
    /// 获取指定位置的bool数组值，如果超过，则引发异常<br />
    /// Get the bool array value at the specified position, if it exceeds, an exception is thrown
    /// </summary>
    /// <param name="destIndex">目标存储的索引</param>
    /// <param name="length">读取的数组长度</param>
    /// <exception cref="T:System.IndexOutOfRangeException"></exception>
    /// <returns>bool数组值</returns>
    public bool[] GetBool(int destIndex, int length)
    {
        var array = new bool[length];
        try
        {
            hybirdLock.Enter();
            for (var i = 0; i < length; i++)
            {
                var num = (destIndex + i) / 8;
                var offset = (destIndex + i) % 8;
                if (isBoolReverseByWord)
                {
                    num = num % 2 != 0 ? num - 1 : num + 1;
                }
                array[i] = (buffer[num] & getOrByte(offset)) == getOrByte(offset);
            }
            hybirdLock.Leave();
        }
        catch
        {
            hybirdLock.Leave();
            throw;
        }
        return array;
    }

    private byte getAndByte(int offset)
    {
        return offset switch
        {
            0 => 254,
            1 => 253,
            2 => 251,
            3 => 247,
            4 => 239,
            5 => 223,
            6 => 191,
            7 => 127,
            _ => byte.MaxValue,
        };
    }

    private byte getOrByte(int offset)
    {
        return offset switch
        {
            0 => 1,
            1 => 2,
            2 => 4,
            3 => 8,
            4 => 16,
            5 => 32,
            6 => 64,
            7 => 128,
            _ => 0,
        };
    }

    /// <summary>
    /// 设置指定的位置的数据块，如果超出，则丢弃数据<br />
    /// Set the data block at the specified position, if it is exceeded, the data is discarded
    /// </summary>
    /// <param name="data">数据块信息</param>
    /// <param name="destIndex">目标存储的索引</param>
    public void SetBytes(byte[] data, int destIndex)
    {
        if (destIndex < capacity && destIndex >= 0 && data != null)
        {
            hybirdLock.Enter();
            if (data.Length + destIndex > buffer.Length)
            {
                Array.Copy(data, 0, buffer, destIndex, buffer.Length - destIndex);
            }
            else
            {
                data.CopyTo(buffer, destIndex);
            }
            hybirdLock.Leave();
        }
    }

    /// <summary>
    /// 设置指定的位置的数据块，如果超出，则丢弃数据
    /// Set the data block at the specified position, if it is exceeded, the data is discarded
    /// </summary>
    /// <param name="data">数据块信息</param>
    /// <param name="destIndex">目标存储的索引</param>
    /// <param name="length">准备拷贝的数据长度</param>
    public void SetBytes(byte[] data, int destIndex, int length)
    {
        if (destIndex < capacity && destIndex >= 0 && data != null)
        {
            if (length > data.Length)
            {
                length = data.Length;
            }
            hybirdLock.Enter();
            if (length + destIndex > buffer.Length)
            {
                Array.Copy(data, 0, buffer, destIndex, buffer.Length - destIndex);
            }
            else
            {
                Array.Copy(data, 0, buffer, destIndex, length);
            }
            hybirdLock.Leave();
        }
    }

    /// <summary>
    /// 设置指定的位置的数据块，如果超出，则丢弃数据<br />
    /// Set the data block at the specified position, if it is exceeded, the data is discarded
    /// </summary>
    /// <param name="data">数据块信息</param>
    /// <param name="sourceIndex">Data中的起始位置</param>
    /// <param name="destIndex">目标存储的索引</param>
    /// <param name="length">准备拷贝的数据长度</param>
    /// <exception cref="T:System.IndexOutOfRangeException"></exception>
    public void SetBytes(byte[] data, int sourceIndex, int destIndex, int length)
    {
        if (destIndex < capacity && destIndex >= 0 && data != null)
        {
            if (length > data.Length)
            {
                length = data.Length;
            }
            hybirdLock.Enter();
            Array.Copy(data, sourceIndex, buffer, destIndex, length);
            hybirdLock.Leave();
        }
    }

    /// <summary>
    /// 获取内存指定长度的数据信息<br />
    /// Get data information of specified length in memory
    /// </summary>
    /// <param name="index">起始位置</param>
    /// <param name="length">数组长度</param>
    /// <returns>返回实际的数据信息</returns>
    public byte[] GetBytes(int index, int length)
    {
        var array = new byte[length];
        if (length > 0)
        {
            hybirdLock.Enter();
            if (index >= 0 && index + length <= buffer.Length)
            {
                Array.Copy(buffer, index, array, 0, length);
            }
            hybirdLock.Leave();
        }
        return array;
    }

    /// <summary>
    /// 获取内存所有的数据信息<br />
    /// Get all data information in memory
    /// </summary>
    /// <returns>实际的数据信息</returns>
    public byte[] GetBytes()
    {
        return GetBytes(0, capacity);
    }

    /// <summary>
    /// 设置byte类型的数据到缓存区<br />
    /// Set byte type data to the cache area
    /// </summary>
    /// <param name="value">byte数值</param>
    /// <param name="index">索引位置</param>
    public void SetValue(byte value, int index)
    {
        SetBytes(new byte[1] { value }, index);
    }

    /// <summary>
    /// 设置short数组的数据到缓存区<br />
    /// Set short array data to the cache area
    /// </summary>
    /// <param name="values">short数组</param>
    /// <param name="index">索引位置</param>
    public void SetValue(short[] values, int index)
    {
        SetBytes(byteTransform.TransByte(values), index);
    }

    /// <summary>
    /// 设置short类型的数据到缓存区<br />
    /// Set short type data to the cache area
    /// </summary>
    /// <param name="value">short数值</param>
    /// <param name="index">索引位置</param>
    public void SetValue(short value, int index)
    {
        SetValue(new short[1] { value }, index);
    }

    /// <summary>
    /// 设置ushort数组的数据到缓存区<br />
    /// Set ushort array data to the cache area
    /// </summary>
    /// <param name="values">ushort数组</param>
    /// <param name="index">索引位置</param>
    public void SetValue(ushort[] values, int index)
    {
        SetBytes(byteTransform.TransByte(values), index);
    }

    /// <summary>
    /// 设置ushort类型的数据到缓存区<br />
    /// Set ushort type data to the cache area
    /// </summary>
    /// <param name="value">ushort数值</param>
    /// <param name="index">索引位置</param>
    public void SetValue(ushort value, int index)
    {
        SetValue(new ushort[1] { value }, index);
    }

    /// <summary>
    /// 设置int数组的数据到缓存区<br />
    /// Set int array data to the cache area
    /// </summary>
    /// <param name="values">int数组</param>
    /// <param name="index">索引位置</param>
    public void SetValue(int[] values, int index)
    {
        SetBytes(byteTransform.TransByte(values), index);
    }

    /// <summary>
    /// 设置int类型的数据到缓存区<br />
    /// Set int type data to the cache area
    /// </summary>
    /// <param name="value">int数值</param>
    /// <param name="index">索引位置</param>
    public void SetValue(int value, int index)
    {
        SetValue(new int[1] { value }, index);
    }

    /// <summary>
    /// 设置uint数组的数据到缓存区<br />
    /// Set uint array data to the cache area
    /// </summary>
    /// <param name="values">uint数组</param>
    /// <param name="index">索引位置</param>
    public void SetValue(uint[] values, int index)
    {
        SetBytes(byteTransform.TransByte(values), index);
    }

    /// <summary>
    /// 设置uint类型的数据到缓存区<br />
    /// Set uint byte data to the cache area
    /// </summary>
    /// <param name="value">uint数值</param>
    /// <param name="index">索引位置</param>
    public void SetValue(uint value, int index)
    {
        SetValue(new uint[1] { value }, index);
    }

    /// <summary>
    /// 设置float数组的数据到缓存区<br />
    /// Set float array data to the cache area
    /// </summary>
    /// <param name="values">float数组</param>
    /// <param name="index">索引位置</param>
    public void SetValue(float[] values, int index)
    {
        SetBytes(byteTransform.TransByte(values), index);
    }

    /// <summary>
    /// 设置float类型的数据到缓存区<br />
    /// Set float type data to the cache area
    /// </summary>
    /// <param name="value">float数值</param>
    /// <param name="index">索引位置</param>
    public void SetValue(float value, int index)
    {
        SetValue(new float[1] { value }, index);
    }

    /// <summary>
    /// 设置long数组的数据到缓存区<br />
    /// Set long array data to the cache area
    /// </summary>
    /// <param name="values">long数组</param>
    /// <param name="index">索引位置</param>
    public void SetValue(long[] values, int index)
    {
        SetBytes(byteTransform.TransByte(values), index);
    }

    /// <summary>
    /// 设置long类型的数据到缓存区<br />
    /// Set long type data to the cache area
    /// </summary>
    /// <param name="value">long数值</param>
    /// <param name="index">索引位置</param>
    public void SetValue(long value, int index)
    {
        SetValue(new long[1] { value }, index);
    }

    /// <summary>
    /// 设置ulong数组的数据到缓存区<br />
    /// Set long array data to the cache area
    /// </summary>
    /// <param name="values">ulong数组</param>
    /// <param name="index">索引位置</param>
    public void SetValue(ulong[] values, int index)
    {
        SetBytes(byteTransform.TransByte(values), index);
    }

    /// <summary>
    /// 设置ulong类型的数据到缓存区<br />
    /// Set ulong byte data to the cache area
    /// </summary>
    /// <param name="value">ulong数值</param>
    /// <param name="index">索引位置</param>
    public void SetValue(ulong value, int index)
    {
        SetValue(new ulong[1] { value }, index);
    }

    /// <summary>
    /// 设置double数组的数据到缓存区<br />
    /// Set double array data to the cache area
    /// </summary>
    /// <param name="values">double数组</param>
    /// <param name="index">索引位置</param>
    public void SetValue(double[] values, int index)
    {
        SetBytes(byteTransform.TransByte(values), index);
    }

    /// <summary>
    /// 设置double类型的数据到缓存区<br />
    /// Set double type data to the cache area
    /// </summary>
    /// <param name="value">double数值</param>
    /// <param name="index">索引位置</param>
    public void SetValue(double value, int index)
    {
        SetValue(new double[1] { value }, index);
    }

    /// <summary>
    /// 获取byte类型的数据<br />
    /// Get byte data
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <returns>byte数值</returns>
    public byte GetByte(int index)
    {
        return GetBytes(index, 1)[0];
    }

    /// <summary>
    /// 获取short类型的数组到缓存区<br />
    /// Get short type array to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <param name="length">数组长度</param>
    /// <returns>short数组</returns>
    public short[] GetInt16(int index, int length)
    {
        return byteTransform.TransInt16(GetBytes(index, length * 2), 0, length);
    }

    /// <summary>
    /// 获取short类型的数据到缓存区<br />
    /// Get short data to the cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <returns>short数据</returns>
    public short GetInt16(int index)
    {
        return GetInt16(index, 1)[0];
    }

    /// <summary>
    /// 获取ushort类型的数组到缓存区<br />
    /// Get ushort type array to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <param name="length">数组长度</param>
    /// <returns>ushort数组</returns>
    public ushort[] GetUInt16(int index, int length)
    {
        return byteTransform.TransUInt16(GetBytes(index, length * 2), 0, length);
    }

    /// <summary>
    /// 获取ushort类型的数据到缓存区<br />
    /// Get ushort type data to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <returns>ushort数据</returns>
    public ushort GetUInt16(int index)
    {
        return GetUInt16(index, 1)[0];
    }

    /// <summary>
    /// 获取int类型的数组到缓存区<br />
    /// Get int type array to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <param name="length">数组长度</param>
    /// <returns>int数组</returns>
    public int[] GetInt32(int index, int length)
    {
        return byteTransform.TransInt32(GetBytes(index, length * 4), 0, length);
    }

    /// <summary>
    /// 获取int类型的数据到缓存区<br />
    /// Get int type data to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <returns>int数据</returns>
    public int GetInt32(int index)
    {
        return GetInt32(index, 1)[0];
    }

    /// <summary>
    /// 获取uint类型的数组到缓存区<br />
    /// Get uint type array to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <param name="length">数组长度</param>
    /// <returns>uint数组</returns>
    public uint[] GetUInt32(int index, int length)
    {
        return byteTransform.TransUInt32(GetBytes(index, length * 4), 0, length);
    }

    /// <summary>
    /// 获取uint类型的数据到缓存区<br />
    /// Get uint type data to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <returns>uint数据</returns>
    public uint GetUInt32(int index)
    {
        return GetUInt32(index, 1)[0];
    }

    /// <summary>
    /// 获取float类型的数组到缓存区<br />
    /// Get float type array to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <param name="length">数组长度</param>
    /// <returns>float数组</returns>
    public float[] GetSingle(int index, int length)
    {
        return byteTransform.TransSingle(GetBytes(index, length * 4), 0, length);
    }

    /// <summary>
    /// 获取float类型的数据到缓存区<br />
    /// Get float type data to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <returns>float数据</returns>
    public float GetSingle(int index)
    {
        return GetSingle(index, 1)[0];
    }

    /// <summary>
    /// 获取long类型的数组到缓存区<br />
    /// Get long type array to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <param name="length">数组长度</param>
    /// <returns>long数组</returns>
    public long[] GetInt64(int index, int length)
    {
        return byteTransform.TransInt64(GetBytes(index, length * 8), 0, length);
    }

    /// <summary>
    /// 获取long类型的数据到缓存区<br />
    /// Get long type data to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <returns>long数据</returns>
    public long GetInt64(int index)
    {
        return GetInt64(index, 1)[0];
    }

    /// <summary>
    /// 获取ulong类型的数组到缓存区<br />
    /// Get ulong type array to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <param name="length">数组长度</param>
    /// <returns>ulong数组</returns>
    public ulong[] GetUInt64(int index, int length)
    {
        return byteTransform.TransUInt64(GetBytes(index, length * 8), 0, length);
    }

    /// <summary>
    /// 获取ulong类型的数据到缓存区<br />
    /// Get ulong type data to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <returns>ulong数据</returns>
    public ulong GetUInt64(int index)
    {
        return GetUInt64(index, 1)[0];
    }

    /// <summary>
    /// 获取double类型的数组到缓存区<br />
    /// Get double type array to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <param name="length">数组长度</param>
    /// <returns>double数组</returns>
    public double[] GetDouble(int index, int length)
    {
        return byteTransform.TransDouble(GetBytes(index, length * 8), 0, length);
    }

    /// <summary>
    /// 获取double类型的数据到缓存区<br />
    /// Get double type data to cache
    /// </summary>
    /// <param name="index">索引位置</param>
    /// <returns>double数据</returns>
    public double GetDouble(int index)
    {
        return GetDouble(index, 1)[0];
    }

    /// <summary>
    /// 读取自定义类型的数据，需要规定解析规则<br />
    /// Read custom types of data, need to specify the parsing rules
    /// </summary>
    /// <typeparam name="T">类型名称</typeparam>
    /// <param name="index">起始索引</param>
    /// <returns>自定义的数据类型</returns>
    public T GetCustomer<T>(int index) where T : IDataTransfer, new()
    {
        var result = new T();
        var bytes = GetBytes(index, result.ReadCount);
        result.ParseSource(bytes);
        return result;
    }

    /// <summary>
    /// 写入自定义类型的数据到缓存中去，需要规定生成字节的方法<br />
    /// Write custom type data to the cache, need to specify the method of generating bytes
    /// </summary>
    /// <typeparam name="T">自定义类型</typeparam>
    /// <param name="data">实例对象</param>
    /// <param name="index">起始地址</param>
    public void SetCustomer<T>(T data, int index) where T : IDataTransfer, new()
    {
        SetBytes(data.ToSource(), index);
    }

    /// <summary>
    /// 释放当前的对象
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                hybirdLock?.Dispose();
                buffer = null;
            }
            disposedValue = true;
        }
    }

    /// <inheritdoc cref="M:System.IDisposable.Dispose" />
    public void Dispose()
    {
        Dispose(disposing: true);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"SoftBuffer[{capacity}][{ByteTransform}]";
    }
}
