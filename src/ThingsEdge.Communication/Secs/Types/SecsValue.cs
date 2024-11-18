using System.Xml.Linq;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Secs.Helper;

namespace ThingsEdge.Communication.Secs.Types;

/// <summary>
/// SECS数据的对象信息，可以用来表示层级及嵌套的数据内容，如果需要显示，只需要<see cref="M:HslCommunication.Secs.Types.SecsValue.ToString" /> 方法即可，
/// 如果需要发送SECS设备，只需要 <see cref="M:HslCommunication.Secs.Types.SecsValue.ToSourceBytes" />，并支持反序列化操作 <see cref="M:HslCommunication.Secs.Types.SecsValue.ParseFromSource(System.Byte[],System.Text.Encoding)" />，无论是XML元素还是byte[]类型。<br />
/// SECS data object information, can be used to represent the hierarchy and nested data content, if you need to display, just need to <see cref="M:HslCommunication.Secs.Types.SecsValue.ToString" /> method can be.
/// If you need to send SECS equipment, only need <see cref="M:HslCommunication.Secs.Types.SecsValue.ToSourceBytes" />, and support the deserialization operation <see cref="M:HslCommunication.Secs.Types.SecsValue.ParseFromSource(System.Byte[],System.Text.Encoding)" />. Whether it's an XML element or byte[] type.
/// </summary>
/// <remarks>
/// XML序列化，反序列化例子：<br />
/// SecsValue value = new SecsValue( new object[]{ 1.23f, "ABC" } );<br />
/// XElement xml = value.ToXElement( ); <br />
/// SecsValue value2 = new SecsValue(xml);
/// </remarks>
/// <example>
/// 关于<see cref="T:HslCommunication.Secs.Types.SecsValue" />类型，可以非常灵活的实例化，参考下面的示例代码
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Secs\SecsGemSample.cs" region="Sample2" title="SecsValue说明" />
/// </example>
public class SecsValue
{
    private object obj = null;

    private int length = 1;

    /// <summary>
    /// 类型信息
    /// </summary>
    public SecsItemType ItemType { get; set; }

    /// <summary>
    /// 字节长度信息，如果是 <see cref="F:HslCommunication.Secs.Types.SecsItemType.List" /> 类型的话，就是数组长度，如果如 <see cref="F:HslCommunication.Secs.Types.SecsItemType.ASCII" /> 类型，就是字符串的字节长度，其他类型都是表示数据个数<br />
    /// Byte length information, if it is of type <see cref="F:HslCommunication.Secs.Types.SecsItemType.List" />, it is the length of the array, if it is of type <see cref="F:HslCommunication.Secs.Types.SecsItemType.ASCII" />,
    /// it is the byte length of the string, other types are the number of data
    /// </summary>
    public int Length => length;

    /// <summary>
    /// 数据值信息，也可以是 <see cref="T:HslCommunication.Secs.Types.SecsValue" /> 的列表信息，在设置列表之前，必须先设置类型
    /// </summary>
    public object Value
    {
        get
        {
            return obj;
        }
        set
        {
            if (ItemType == SecsItemType.None && value != null)
            {
                throw new ArgumentException("Must set ItemType before set value.", "value");
            }
            obj = value;
            length = GetValueLength(this);
        }
    }

    /// <summary>
    /// 实例化一个空的SECS对象
    /// </summary>
    public SecsValue()
    {
        ItemType = SecsItemType.None;
    }

    /// <summary>
    /// 从一个字符串对象初始化数据信息
    /// </summary>
    /// <param name="value">字符串信息</param>
    public SecsValue(string value)
        : this(SecsItemType.ASCII, value)
    {
    }

    /// <summary>
    /// 从一个字符串数组对象初始化数据信息
    /// </summary>
    /// <param name="value">字符串数组</param>
    public SecsValue(string[] value)
    {
        ItemType = SecsItemType.List;
        var list = new List<SecsValue>();
        if (value == null)
        {
            value = new string[0];
        }
        var array = value;
        foreach (var value2 in array)
        {
            list.Add(new SecsValue(value2));
        }
        Value = list.ToArray();
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.SByte" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(sbyte value)
        : this(SecsItemType.SByte, value)
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsValue.#ctor(System.SByte)" />
    public SecsValue(sbyte[] value)
        : this(SecsItemType.SByte, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Byte" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(byte value)
        : this(SecsItemType.Byte, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Int16" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(short value)
        : this(SecsItemType.Int16, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Int32" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(short[] value)
        : this(SecsItemType.Int16, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.UInt16" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(ushort value)
        : this(SecsItemType.UInt16, value)
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsValue.#ctor(System.UInt16)" />
    public SecsValue(ushort[] value)
        : this(SecsItemType.UInt16, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Int32" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(int value)
        : this(SecsItemType.Int32, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Int32" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(int[] value)
        : this(SecsItemType.Int32, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.UInt32" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(uint value)
        : this(SecsItemType.UInt32, value)
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsValue.#ctor(System.UInt32)" />
    public SecsValue(uint[] value)
        : this(SecsItemType.UInt32, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Int64" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(long value)
        : this(SecsItemType.Int64, value)
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsValue.#ctor(System.Int64)" />
    public SecsValue(long[] value)
        : this(SecsItemType.Int64, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.UInt64" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(ulong value)
        : this(SecsItemType.UInt64, value)
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsValue.#ctor(System.UInt64)" />
    public SecsValue(ulong[] value)
        : this(SecsItemType.UInt64, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Single" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(float value)
        : this(SecsItemType.Single, value)
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsValue.#ctor(System.Single)" />
    public SecsValue(float[] value)
        : this(SecsItemType.Single, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Double" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(double value)
        : this(SecsItemType.Double, value)
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsValue.#ctor(System.Double)" />
    public SecsValue(double[] value)
        : this(SecsItemType.Double, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Byte" /> 数组的对象初始化数据，需要指定 <see cref="T:HslCommunication.Secs.Types.SecsItemType" /> 来表示二进制还是byte数组类型
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(byte[] value)
    {
        ItemType = SecsItemType.Binary;
        Value = value;
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Boolean" /> 的对象初始化数据
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(bool value)
        : this(SecsItemType.Bool, value)
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsValue.#ctor(System.Boolean)" />
    public SecsValue(bool[] value)
        : this(SecsItemType.Bool, value)
    {
    }

    /// <summary>
    /// 从一个类型为 <see cref="T:System.Object" /> 数组的对象初始化数据，初始化后，本对象为 <see cref="F:HslCommunication.Secs.Types.SecsItemType.List" /> 类型
    /// </summary>
    /// <param name="value">数据值信息</param>
    public SecsValue(IEnumerable<object> value)
    {
        ItemType = SecsItemType.List;
        var list = new List<SecsValue>();
        if (value == null)
        {
            value = new object[0];
        }
        foreach (var item in value)
        {
            var type = item.GetType();
            if (type == typeof(SecsValue))
            {
                list.Add((SecsValue)item);
            }
            if (type == typeof(bool))
            {
                list.Add(new SecsValue((bool)item));
            }
            if (type == typeof(bool[]))
            {
                list.Add(new SecsValue((bool[])item));
            }
            if (type == typeof(sbyte))
            {
                list.Add(new SecsValue((sbyte)item));
            }
            if (type == typeof(sbyte[]))
            {
                list.Add(new SecsValue((sbyte[])item));
            }
            if (type == typeof(byte))
            {
                list.Add(new SecsValue((byte)item));
            }
            if (type == typeof(short))
            {
                list.Add(new SecsValue((short)item));
            }
            if (type == typeof(short[]))
            {
                list.Add(new SecsValue((short[])item));
            }
            if (type == typeof(ushort))
            {
                list.Add(new SecsValue((ushort)item));
            }
            if (type == typeof(ushort[]))
            {
                list.Add(new SecsValue((ushort[])item));
            }
            if (type == typeof(int))
            {
                list.Add(new SecsValue((int)item));
            }
            if (type == typeof(int[]))
            {
                list.Add(new SecsValue((int[])item));
            }
            if (type == typeof(uint))
            {
                list.Add(new SecsValue((uint)item));
            }
            if (type == typeof(uint[]))
            {
                list.Add(new SecsValue((uint[])item));
            }
            if (type == typeof(long))
            {
                list.Add(new SecsValue((long)item));
            }
            if (type == typeof(long[]))
            {
                list.Add(new SecsValue((long[])item));
            }
            if (type == typeof(ulong))
            {
                list.Add(new SecsValue((ulong)item));
            }
            if (type == typeof(ulong[]))
            {
                list.Add(new SecsValue((ulong[])item));
            }
            if (type == typeof(float))
            {
                list.Add(new SecsValue((float)item));
            }
            if (type == typeof(float[]))
            {
                list.Add(new SecsValue((float[])item));
            }
            if (type == typeof(double))
            {
                list.Add(new SecsValue((double)item));
            }
            if (type == typeof(double[]))
            {
                list.Add(new SecsValue((double[])item));
            }
            if (type == typeof(string))
            {
                list.Add(new SecsValue((string)item));
            }
            if (type == typeof(string[]))
            {
                list.Add(new SecsValue((string[])item));
            }
            if (type == typeof(byte[]))
            {
                list.Add(new SecsValue((byte[])item));
            }
            if (type == typeof(object[]))
            {
                list.Add(new SecsValue((object[])item));
            }
            if (type == typeof(List<object>))
            {
                list.Add(new SecsValue((List<object>)item));
            }
        }
        Value = list.ToArray();
    }

    /// <inheritdoc cref="M:HslCommunication.Secs.Types.SecsValue.#ctor(HslCommunication.Secs.Types.SecsItemType,System.Object,System.Int32)" />
    public SecsValue(SecsItemType type, object value)
    {
        ItemType = type;
        Value = value;
    }

    /// <summary>
    /// 通过指定的参数类型及值信息，来实例化一个对象<br />
    /// Instantiate an object by specifying the parameter type and value information
    /// </summary>
    /// <param name="type">数据的类型信息</param>
    /// <param name="value">数据值信息，当是<see cref="F:HslCommunication.Secs.Types.SecsItemType.List" />类型时，本值为空 </param>
    /// <param name="length">长度参数信息</param>
    public SecsValue(SecsItemType type, object value, int length)
    {
        ItemType = type;
        Value = value;
        this.length = length;
    }

    /// <summary>
    /// 从完整的XML元素进行实例化一个对象
    /// </summary>
    /// <param name="element">符合SECS的XML数据表示元素</param>
    /// <exception cref="T:System.ArgumentException">解析失败的异常</exception>
    public SecsValue(XElement element)
    {
        if (element.Name == "List")
        {
            ItemType = SecsItemType.List;
            var list = new List<SecsValue>();
            foreach (var item in element.Elements())
            {
                var secsValue = new SecsValue(item);
                if (secsValue != null)
                {
                    list.Add(secsValue);
                }
            }
            Value = list.ToArray();
        }
        else if (element.Name == "SByte")
        {
            ItemType = SecsItemType.SByte;
            Value = GetObjectValue(element, sbyte.Parse);
        }
        else if (element.Name == "Byte")
        {
            ItemType = SecsItemType.Byte;
            Value = GetObjectValue(element, byte.Parse);
        }
        else if (element.Name == "Int16")
        {
            ItemType = SecsItemType.Int16;
            Value = GetObjectValue(element, short.Parse);
        }
        else if (element.Name == "UInt16")
        {
            ItemType = SecsItemType.UInt16;
            Value = GetObjectValue(element, ushort.Parse);
        }
        else if (element.Name == "Int32")
        {
            ItemType = SecsItemType.Int32;
            Value = GetObjectValue(element, int.Parse);
        }
        else if (element.Name == "UInt32")
        {
            ItemType = SecsItemType.UInt32;
            Value = GetObjectValue(element, uint.Parse);
        }
        else if (element.Name == "Int64")
        {
            ItemType = SecsItemType.Int64;
            Value = GetObjectValue(element, long.Parse);
        }
        else if (element.Name == "UInt64")
        {
            ItemType = SecsItemType.UInt64;
            Value = GetObjectValue(element, ulong.Parse);
        }
        else if (element.Name == "Single")
        {
            ItemType = SecsItemType.Single;
            Value = GetObjectValue(element, float.Parse);
        }
        else if (element.Name == "Double")
        {
            ItemType = SecsItemType.Double;
            Value = GetObjectValue(element, double.Parse);
        }
        else if (element.Name == "Bool")
        {
            ItemType = SecsItemType.Bool;
            Value = GetObjectValue(element, bool.Parse);
        }
        else if (element.Name == "ASCII")
        {
            ItemType = SecsItemType.ASCII;
            Value = GetAttribute(element, "Value", null, (m) => m);
        }
        else if (element.Name == "Binary")
        {
            ItemType = SecsItemType.Binary;
            Value = GetAttribute(element, "Value", new byte[0], (m) => m.ToHexBytes());
        }
        else if (element.Name == "JIS8")
        {
            ItemType = SecsItemType.JIS8;
            Value = GetAttribute(element, "Value", new byte[0], (m) => m.ToHexBytes());
        }
        else
        {
            if (!(element.Name == "None"))
            {
                throw new ArgumentException("element");
            }
            ItemType = SecsItemType.None;
            Value = GetAttribute(element, "Value", null, (m) => m);
        }
    }

    /// <summary>
    /// 获取当前数值的XML表示形式
    /// </summary>
    /// <returns>XML元素信息</returns>
    public XElement ToXElement()
    {
        if (ItemType == SecsItemType.List)
        {
            var xElement = new XElement("List");
            if (Value is IEnumerable<SecsValue> enumerable)
            {
                xElement.SetAttributeValue("Length", enumerable.Count());
                foreach (var item in enumerable)
                {
                    xElement.Add(item.ToXElement());
                }
            }
            else
            {
                xElement.SetAttributeValue("Length", 0);
            }
            return xElement;
        }
        var xElement2 = new XElement(ItemType.ToString());
        xElement2.SetAttributeValue("Length", Length);
        if (ItemType == SecsItemType.Binary || ItemType == SecsItemType.JIS8)
        {
            xElement2.SetAttributeValue("Value", (Value as byte[]).ToHexString());
        }
        else if (ItemType == SecsItemType.ASCII)
        {
            xElement2.SetAttributeValue("Value", Value);
        }
        else if (ItemType != SecsItemType.None)
        {
            if (Value is Array array)
            {
                var stringBuilder = new StringBuilder("[");
                for (var i = 0; i < array.Length; i++)
                {
                    stringBuilder.Append(array.GetValue(i).ToString());
                    if (i != array.Length - 1)
                    {
                        stringBuilder.Append(",");
                    }
                }
                stringBuilder.Append("]");
                xElement2.SetAttributeValue("Value", stringBuilder.ToString());
            }
            else
            {
                xElement2.SetAttributeValue("Value", Value);
            }
        }
        return xElement2;
    }

    /// <summary>
    /// 当前的对象信息转换回实际的原始字节信息，方便写入操作
    /// </summary>
    /// <returns>原始字节数据</returns>
    public byte[] ToSourceBytes()
    {
        return ToSourceBytes(Encoding.Default);
    }

    /// <summary>
    /// 使用指定的编码将当前的对象信息转换回实际的原始字节信息，方便写入操作
    /// </summary>
    /// <param name="encoding">编码信息</param>
    /// <returns>原始字节数据</returns>
    public byte[] ToSourceBytes(Encoding encoding)
    {
        if (ItemType == SecsItemType.None)
        {
            return new byte[0];
        }
        var list = new List<byte>();
        if (ItemType == SecsItemType.List)
        {
            Secs2.AddCodeAndValueSource(list, this, encoding);
            if (Value is SecsValue[] array)
            {
                var array2 = array;
                foreach (var secsValue in array2)
                {
                    list.AddRange(secsValue.ToSourceBytes(encoding));
                }
            }
        }
        else
        {
            Secs2.AddCodeAndValueSource(list, this, encoding);
        }
        return list.ToArray();
    }

    private static string getSourceCode(SecsValue secsValue)
    {
        if (secsValue.ItemType == SecsItemType.List)
        {
            var stringBuilder = new StringBuilder("new object[]{ ");
            if (secsValue.Value is IEnumerable<SecsValue> enumerable)
            {
                foreach (var item in enumerable)
                {
                    stringBuilder.Append(getSourceCode(item));
                    stringBuilder.Append(",");
                }
            }
            if (stringBuilder[stringBuilder.Length - 1] == ',')
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append(" }");
            return stringBuilder.ToString();
        }
        if (secsValue.ItemType == SecsItemType.Binary || secsValue.ItemType == SecsItemType.JIS8)
        {
            return "\"" + (secsValue.Value as byte[]).ToHexString() + "\".ToHexBytes( )";
        }
        if (secsValue.ItemType == SecsItemType.ASCII)
        {
            return $"\"{secsValue.Value}\"";
        }
        if (secsValue.ItemType == SecsItemType.Int16)
        {
            if (secsValue.Value is Array array)
            {
                return "new short[]{ " + getArrayString(array) + " }";
            }
            return $"(short){secsValue.Value}";
        }
        if (secsValue.ItemType == SecsItemType.Bool)
        {
            if (secsValue.Value is Array array2)
            {
                return "new bool[]{ " + getArrayString(array2) + " }";
            }
            return secsValue.Value.ToString().ToLower() ?? "";
        }
        if (secsValue.ItemType == SecsItemType.UInt16)
        {
            if (secsValue.Value is Array array3)
            {
                return "new ushort[]{ " + getArrayString(array3) + " }";
            }
            return $"(ushort){secsValue.Value}";
        }
        if (secsValue.ItemType == SecsItemType.Int32)
        {
            if (secsValue.Value is Array array4)
            {
                return "new int[]{ " + getArrayString(array4) + " }";
            }
            return $"{secsValue.Value}";
        }
        if (secsValue.ItemType == SecsItemType.UInt32)
        {
            if (secsValue.Value is Array array5)
            {
                return "new uint[]{ " + getArrayString(array5) + " }";
            }
            return $"(uint){secsValue.Value}";
        }
        if (secsValue.ItemType == SecsItemType.Int64)
        {
            if (secsValue.Value is Array array6)
            {
                return "new long[]{ " + getArrayString(array6) + " }";
            }
            return $"{secsValue.Value}L";
        }
        if (secsValue.ItemType == SecsItemType.UInt64)
        {
            if (secsValue.Value is Array array7)
            {
                return "new ulong[]{ " + getArrayString(array7) + " }";
            }
            return $"{secsValue.Value}UL";
        }
        if (secsValue.ItemType == SecsItemType.Single)
        {
            if (secsValue.Value is Array array8)
            {
                return "new float[]{ " + getArrayString(array8, "f") + " }";
            }
            return $"{secsValue.Value}f";
        }
        if (secsValue.ItemType == SecsItemType.Double)
        {
            if (secsValue.Value is Array array9)
            {
                return "new double[]{ " + getArrayString(array9, "d") + " }";
            }
            return $"{secsValue.Value}d";
        }
        if (secsValue.ItemType == SecsItemType.Byte)
        {
            if (secsValue.Value is Array array10)
            {
                return "new byte[]{ " + getArrayString(array10) + " }";
            }
            return $"(byte){secsValue.Value}";
        }
        if (secsValue.ItemType == SecsItemType.SByte)
        {
            if (secsValue.Value is Array array11)
            {
                return "new sbyte[]{ " + getArrayString(array11) + " }";
            }
            return $"(sbyte){secsValue.Value}";
        }
        if (secsValue.Value is Array)
        {
            return "Unkonw data";
        }
        return secsValue.Value.ToString();
    }

    private static string getArrayString(Array array, string tail = "")
    {
        var stringBuilder = new StringBuilder("");
        for (var i = 0; i < array.Length; i++)
        {
            stringBuilder.Append(array.GetValue(i).ToString());
            if (!string.IsNullOrEmpty(tail))
            {
                stringBuilder.Append(tail);
            }
            if (i != array.Length - 1)
            {
                stringBuilder.Append(",");
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 获取当前 <see cref="T:HslCommunication.Secs.Types.SecsValue" /> 对象的源代码表示方式，可以直接复制生成同等对象<br />
    /// Obtain the source code representation of the current <see cref="T:HslCommunication.Secs.Types.SecsValue" /> object, and you can directly copy and generate an equivalent object
    /// </summary>
    /// <returns>对象的源代码表示方式</returns>
    public string ToSourceCode()
    {
        if (ItemType == SecsItemType.None)
        {
            return "SecsValue.EmptySecsValue( )";
        }
        var stringBuilder = new StringBuilder("new SecsValue( ");
        stringBuilder.Append(getSourceCode(this));
        stringBuilder.Append(")");
        return stringBuilder.ToString();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ToXElement().ToString();
    }

    /// <summary>
    /// 将当前的对象转为 <see cref="T:HslCommunication.Secs.Types.VariableName" /> 数组对象信息，也就是标签名列表
    /// </summary>
    /// <returns><see cref="T:HslCommunication.Secs.Types.VariableName" />数组对象</returns>
    /// <exception cref="T:System.InvalidCastException"></exception>
    public VariableName[] ToVaruableNames()
    {
        TypeHelper.TypeListCheck(this);
        var list = new List<VariableName>();
        if (Value is SecsValue[] array)
        {
            var array2 = array;
            foreach (var secsValue in array2)
            {
                TypeHelper.TypeListCheck(secsValue);
                list.Add(secsValue);
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// 从一个对象数组里创建一个secsvalue对象
    /// </summary>
    /// <param name="objs">实际的数据数组</param>
    /// <returns>secs对象信息</returns>
    public static SecsValue CreateListSecsValue(params object[] objs)
    {
        return new SecsValue(objs);
    }

    /// <summary>
    /// 从原始的字节数据中解析出实际的 <see cref="T:HslCommunication.Secs.Types.SecsValue" /> 对象内容。
    /// </summary>
    /// <param name="source">原始字节数据</param>
    /// <param name="encoding">编码信息</param>
    /// <returns>SecsItemValue对象</returns>
    public static SecsValue ParseFromSource(byte[] source, Encoding encoding)
    {
        return Secs2.ExtraToSecsItemValue(source, encoding);
    }

    /// <summary>
    /// 获取空的列表信息
    /// </summary>
    /// <returns>secs数据对象</returns>
    public static SecsValue EmptyListValue()
    {
        return new SecsValue(SecsItemType.List, null);
    }

    /// <summary>
    /// 获取空的对象信息
    /// </summary>
    /// <returns>secs数据对象</returns>
    public static SecsValue EmptySecsValue()
    {
        return new SecsValue(SecsItemType.None, null);
    }

    /// <summary>
    /// 获取当前的 <see cref="T:HslCommunication.Secs.Types.SecsValue" /> 的数据长度信息
    /// </summary>
    /// <param name="secsValue">secs值</param>
    /// <returns>数据长度信息</returns>
    public static int GetValueLength(SecsValue secsValue)
    {
        if (secsValue.ItemType == SecsItemType.None)
        {
            return 0;
        }
        if (secsValue.ItemType == SecsItemType.List)
        {
            return secsValue.Value is IEnumerable<SecsValue> source ? source.Count() : 0;
        }
        if (secsValue.Value == null)
        {
            return 0;
        }
        if (secsValue.ItemType == SecsItemType.SByte)
        {
            return secsValue.Value.GetType() == typeof(sbyte) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.Byte)
        {
            return secsValue.Value.GetType() == typeof(byte) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.Int16)
        {
            return secsValue.Value.GetType() == typeof(short) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.UInt16)
        {
            return secsValue.Value.GetType() == typeof(ushort) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.Int32)
        {
            return secsValue.Value.GetType() == typeof(int) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.UInt32)
        {
            return secsValue.Value.GetType() == typeof(uint) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.Int64)
        {
            return secsValue.Value.GetType() == typeof(long) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.UInt64)
        {
            return secsValue.Value.GetType() == typeof(ulong) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.Single)
        {
            return secsValue.Value.GetType() == typeof(float) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.Double)
        {
            return secsValue.Value.GetType() == typeof(double) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.Bool)
        {
            return secsValue.Value.GetType() == typeof(bool) ? 1 : (secsValue.Value as Array).Length;
        }
        if (secsValue.ItemType == SecsItemType.Binary)
        {
            return (secsValue.Value as byte[]).Length;
        }
        if (secsValue.ItemType == SecsItemType.JIS8)
        {
            return (secsValue.Value as byte[]).Length;
        }
        if (secsValue.ItemType == SecsItemType.ASCII)
        {
            return secsValue.Value.ToString().Length;
        }
        return 0;
    }

    private static object GetObjectValue<T>(XElement element, Func<string, T> trans)
    {
        var attribute = GetAttribute(element, "Value", "", (m) => m);
        if (!attribute.Contains(","))
        {
            return trans(attribute);
        }
        return attribute.ToStringArray(trans);
    }

    private static T GetAttribute<T>(XElement element, string name, T defaultValue, Func<string, T> trans)
    {
        var xAttribute = element.Attribute(name);
        if (xAttribute == null)
        {
            return defaultValue;
        }
        return trans(xAttribute.Value);
    }
}
