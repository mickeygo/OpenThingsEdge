using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core;

/// <summary>
/// 数据转换类的基础，提供了一些基础的方法实现，默认 <see cref="DataFormat.CDAB" /> 的顺序，和C#的字节顺序是一致的。
/// </summary>
public class RegularByteTransform : IByteTransform
{
    /// <inheritdoc />
    public DataFormat DataFormat { get; set; }

    /// <inheritdoc />
    public bool IsStringReverseByteWord { get; set; }

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public RegularByteTransform()
    {
        DataFormat = DataFormat.DCBA;
    }

    /// <summary>
    /// 使用指定的数据解析来实例化对象
    /// </summary>
    /// <param name="dataFormat">数据规则</param>
    public RegularByteTransform(DataFormat dataFormat)
    {
        DataFormat = dataFormat;
    }

    /// <inheritdoc />
    public virtual bool TransBool(byte[] buffer, int index)
    {
        return buffer.GetBoolByIndex(index);
    }

    /// <inheritdoc />
    public virtual bool[] TransBool(byte[] buffer, int index, int length)
    {
        var array = new bool[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = buffer.GetBoolByIndex(i + index);
        }
        return array;
    }

    /// <inheritdoc />
    public virtual byte TransByte(byte[] buffer, int index)
    {
        return buffer[index];
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(byte[] buffer, int index, int length)
    {
        var array = new byte[length];
        Array.Copy(buffer, index, array, 0, length);
        return array;
    }

    /// <inheritdoc />
    public virtual short TransInt16(byte[] buffer, int index)
    {
        return BitConverter.ToInt16(ByteTransDataFormat2(buffer, index), 0);
    }

    /// <inheritdoc />
    public virtual short[] TransInt16(byte[] buffer, int index, int length)
    {
        var array = new short[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = TransInt16(buffer, index + 2 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public short[,] TransInt16(byte[] buffer, int index, int row, int col)
    {
        return CommHelper.CreateTwoArrayFromOneArray(TransInt16(buffer, index, row * col), row, col);
    }

    /// <inheritdoc />
    public virtual ushort TransUInt16(byte[] buffer, int index)
    {
        return BitConverter.ToUInt16(ByteTransDataFormat2(buffer, index), 0);
    }

    /// <inheritdoc />
    public virtual ushort[] TransUInt16(byte[] buffer, int index, int length)
    {
        var array = new ushort[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = TransUInt16(buffer, index + 2 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public ushort[,] TransUInt16(byte[] buffer, int index, int row, int col)
    {
        return CommHelper.CreateTwoArrayFromOneArray(TransUInt16(buffer, index, row * col), row, col);
    }

    /// <inheritdoc />
    public virtual int TransInt32(byte[] buffer, int index)
    {
        return BitConverter.ToInt32(ByteTransDataFormat4(buffer, index), 0);
    }

    /// <inheritdoc />
    public virtual int[] TransInt32(byte[] buffer, int index, int length)
    {
        var array = new int[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = TransInt32(buffer, index + 4 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public int[,] TransInt32(byte[] buffer, int index, int row, int col)
    {
        return CommHelper.CreateTwoArrayFromOneArray(TransInt32(buffer, index, row * col), row, col);
    }

    /// <inheritdoc />
    public virtual uint TransUInt32(byte[] buffer, int index)
    {
        return BitConverter.ToUInt32(ByteTransDataFormat4(buffer, index), 0);
    }

    /// <inheritdoc />
    public virtual uint[] TransUInt32(byte[] buffer, int index, int length)
    {
        var array = new uint[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = TransUInt32(buffer, index + 4 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public uint[,] TransUInt32(byte[] buffer, int index, int row, int col)
    {
        return CommHelper.CreateTwoArrayFromOneArray(TransUInt32(buffer, index, row * col), row, col);
    }

    /// <inheritdoc />
    public virtual long TransInt64(byte[] buffer, int index)
    {
        return BitConverter.ToInt64(ByteTransDataFormat8(buffer, index), 0);
    }

    /// <inheritdoc />
    public virtual long[] TransInt64(byte[] buffer, int index, int length)
    {
        var array = new long[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = TransInt64(buffer, index + 8 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public long[,] TransInt64(byte[] buffer, int index, int row, int col)
    {
        return CommHelper.CreateTwoArrayFromOneArray(TransInt64(buffer, index, row * col), row, col);
    }

    /// <inheritdoc />
    public virtual ulong TransUInt64(byte[] buffer, int index)
    {
        return BitConverter.ToUInt64(ByteTransDataFormat8(buffer, index), 0);
    }

    /// <inheritdoc />
    public virtual ulong[] TransUInt64(byte[] buffer, int index, int length)
    {
        var array = new ulong[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = TransUInt64(buffer, index + 8 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public ulong[,] TransUInt64(byte[] buffer, int index, int row, int col)
    {
        return CommHelper.CreateTwoArrayFromOneArray(TransUInt64(buffer, index, row * col), row, col);
    }

    /// <inheritdoc />
    public virtual float TransSingle(byte[] buffer, int index)
    {
        return BitConverter.ToSingle(ByteTransDataFormat4(buffer, index), 0);
    }

    /// <inheritdoc />
    public virtual float[] TransSingle(byte[] buffer, int index, int length)
    {
        var array = new float[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = TransSingle(buffer, index + 4 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public float[,] TransSingle(byte[] buffer, int index, int row, int col)
    {
        return CommHelper.CreateTwoArrayFromOneArray(TransSingle(buffer, index, row * col), row, col);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransDouble(System.Byte[],System.Int32)" />
    public virtual double TransDouble(byte[] buffer, int index)
    {
        return BitConverter.ToDouble(ByteTransDataFormat8(buffer, index), 0);
    }

    /// <inheritdoc />
    public virtual double[] TransDouble(byte[] buffer, int index, int length)
    {
        var array = new double[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = TransDouble(buffer, index + 8 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public double[,] TransDouble(byte[] buffer, int index, int row, int col)
    {
        return CommHelper.CreateTwoArrayFromOneArray(TransDouble(buffer, index, row * col), row, col);
    }

    /// <inheritdoc />
    public virtual string TransString(byte[] buffer, int index, int length, Encoding encoding)
    {
        var array = TransByte(buffer, index, length);
        if (IsStringReverseByteWord)
        {
            return encoding.GetString(SoftBasic.BytesReverseByWord(array));
        }
        return encoding.GetString(array);
    }

    /// <inheritdoc />
    public virtual string TransString(byte[] buffer, Encoding encoding)
    {
        return encoding.GetString(buffer);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(bool value)
    {
        return TransByte([value]);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(bool[] values)
    {
        return values == null ? null : SoftBasic.BoolArrayToByte(values);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(byte value)
    {
        return [value];
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(short value)
    {
        return TransByte([value]);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(short[] values)
    {
        if (values == null)
        {
            return null;
        }
        var array = new byte[values.Length * 2];
        for (var i = 0; i < values.Length; i++)
        {
            ByteTransDataFormat2(BitConverter.GetBytes(values[i])).CopyTo(array, 2 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(ushort value)
    {
        return TransByte(new ushort[1] { value });
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(ushort[] values)
    {
        if (values == null)
        {
            return null;
        }
        var array = new byte[values.Length * 2];
        for (var i = 0; i < values.Length; i++)
        {
            ByteTransDataFormat2(BitConverter.GetBytes(values[i])).CopyTo(array, 2 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(int value)
    {
        return TransByte([value]);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(int[] values)
    {
        if (values == null)
        {
            return null;
        }
        var array = new byte[values.Length * 4];
        for (var i = 0; i < values.Length; i++)
        {
            ByteTransDataFormat4(BitConverter.GetBytes(values[i])).CopyTo(array, 4 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(uint value)
    {
        return TransByte([value]);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(uint[] values)
    {
        if (values == null)
        {
            return null;
        }
        var array = new byte[values.Length * 4];
        for (var i = 0; i < values.Length; i++)
        {
            ByteTransDataFormat4(BitConverter.GetBytes(values[i])).CopyTo(array, 4 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(long value)
    {
        return TransByte([value]);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(long[] values)
    {
        if (values == null)
        {
            return null;
        }
        var array = new byte[values.Length * 8];
        for (var i = 0; i < values.Length; i++)
        {
            ByteTransDataFormat8(BitConverter.GetBytes(values[i])).CopyTo(array, 8 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(ulong value)
    {
        return TransByte([value]);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(ulong[] values)
    {
        if (values == null)
        {
            return null;
        }
        var array = new byte[values.Length * 8];
        for (var i = 0; i < values.Length; i++)
        {
            ByteTransDataFormat8(BitConverter.GetBytes(values[i])).CopyTo(array, 8 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(float value)
    {
        return TransByte([value]);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(float[] values)
    {
        if (values == null)
        {
            return null;
        }
        var array = new byte[values.Length * 4];
        for (var i = 0; i < values.Length; i++)
        {
            ByteTransDataFormat4(BitConverter.GetBytes(values[i])).CopyTo(array, 4 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(double value)
    {
        return TransByte([value]);
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(double[] values)
    {
        if (values == null)
        {
            return null;
        }
        var array = new byte[values.Length * 8];
        for (var i = 0; i < values.Length; i++)
        {
            ByteTransDataFormat8(BitConverter.GetBytes(values[i])).CopyTo(array, 8 * i);
        }
        return array;
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(string value, Encoding encoding)
    {
        if (value == null)
        {
            return null;
        }
        var bytes = encoding.GetBytes(value);
        return IsStringReverseByteWord ? SoftBasic.BytesReverseByWord(bytes) : bytes;
    }

    /// <inheritdoc />
    public virtual byte[] TransByte(string value, int length, Encoding encoding)
    {
        if (value == null)
        {
            return null;
        }
        var bytes = encoding.GetBytes(value);
        return IsStringReverseByteWord
            ? SoftBasic.ArrayExpandToLength(SoftBasic.BytesReverseByWord(bytes), length)
            : SoftBasic.ArrayExpandToLength(bytes, length);
    }

    /// <summary>
    /// 反转两个字节的数据信息
    /// </summary>
    /// <param name="value">原始字节数据</param>
    /// <param name="index">起始的索引</param>
    /// <returns>反转后的字节</returns>
    protected virtual byte[] ByteTransDataFormat2(byte[] value, int index = 0)
    {
        var array = new byte[2];
        switch (DataFormat)
        {
            case DataFormat.ABCD:
            case DataFormat.CDAB:
                array[0] = value[index + 1];
                array[1] = value[index];
                break;
            case DataFormat.BADC:
            case DataFormat.DCBA:
                array[0] = value[index];
                array[1] = value[index + 1];
                break;
        }
        return array;
    }

    /// <summary>
    /// 反转多字节的数据信息
    /// </summary>
    /// <param name="value">数据字节</param>
    /// <param name="index">起始索引，默认值为0</param>
    /// <returns>实际字节信息</returns>
    protected virtual byte[] ByteTransDataFormat4(byte[] value, int index = 0)
    {
        var array = new byte[4];
        switch (DataFormat)
        {
            case DataFormat.ABCD:
                array[0] = value[index + 3];
                array[1] = value[index + 2];
                array[2] = value[index + 1];
                array[3] = value[index];
                break;
            case DataFormat.BADC:
                array[0] = value[index + 2];
                array[1] = value[index + 3];
                array[2] = value[index];
                array[3] = value[index + 1];
                break;
            case DataFormat.CDAB:
                array[0] = value[index + 1];
                array[1] = value[index];
                array[2] = value[index + 3];
                array[3] = value[index + 2];
                break;
            case DataFormat.DCBA:
                array[0] = value[index];
                array[1] = value[index + 1];
                array[2] = value[index + 2];
                array[3] = value[index + 3];
                break;
        }
        return array;
    }

    /// <summary>
    /// 反转多字节的数据信息
    /// </summary>
    /// <param name="value">数据字节</param>
    /// <param name="index">起始索引，默认值为0</param>
    /// <returns>实际字节信息</returns>
    protected virtual byte[] ByteTransDataFormat8(byte[] value, int index = 0)
    {
        var array = new byte[8];
        switch (DataFormat)
        {
            case DataFormat.ABCD:
                array[0] = value[index + 7];
                array[1] = value[index + 6];
                array[2] = value[index + 5];
                array[3] = value[index + 4];
                array[4] = value[index + 3];
                array[5] = value[index + 2];
                array[6] = value[index + 1];
                array[7] = value[index];
                break;
            case DataFormat.BADC:
                array[0] = value[index + 6];
                array[1] = value[index + 7];
                array[2] = value[index + 4];
                array[3] = value[index + 5];
                array[4] = value[index + 2];
                array[5] = value[index + 3];
                array[6] = value[index];
                array[7] = value[index + 1];
                break;
            case DataFormat.CDAB:
                array[0] = value[index + 1];
                array[1] = value[index];
                array[2] = value[index + 3];
                array[3] = value[index + 2];
                array[4] = value[index + 5];
                array[5] = value[index + 4];
                array[6] = value[index + 7];
                array[7] = value[index + 6];
                break;
            case DataFormat.DCBA:
                array[0] = value[index];
                array[1] = value[index + 1];
                array[2] = value[index + 2];
                array[3] = value[index + 3];
                array[4] = value[index + 4];
                array[5] = value[index + 5];
                array[6] = value[index + 6];
                array[7] = value[index + 7];
                break;
        }
        return array;
    }

    /// <inheritdoc />
    public virtual IByteTransform CreateByDateFormat(DataFormat dataFormat)
    {
        return new RegularByteTransform(dataFormat)
        {
            IsStringReverseByteWord = IsStringReverseByteWord
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"RegularByteTransform[{DataFormat}]";
    }
}
