namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// AB PLC的数据标签实体类。
/// </summary>
public class AbTagItem
{
    /// <summary>
    /// 实例ID。
    /// </summary>
    public uint InstanceID { get; set; }

    /// <summary>
    /// 当前标签的名字。
    /// </summary>
    [NotNull]
    public string? Name { get; set; }

    /// <summary>
    /// 当前标签的类型代号，例如 0x0C1 表示bool类型，如果当前的标签的<see cref="IsStruct" />为 <c>True</c>，那么本属性表示结构体的实例ID。
    /// </summary>
    public ushort SymbolType { get; set; }

    /// <summary>
    /// 数据的维度信息，默认是0，标量数据，1表示一维数组，2表示二维数组。
    /// </summary>
    public int ArrayDimension { get; set; }

    /// <summary>
    /// 当前的标签是否结构体数据。
    /// </summary>
    public bool IsStruct { get; set; }

    /// <summary>
    /// 当前如果是数组，表示数组的长度，仅在读取结构体的变量信息时有效，为-1则是无效。
    /// </summary>
    public int[] ArrayLength { get; set; } = [-1, -1, -1];

    /// <summary>
    /// 获取或设置本属性实际数据在结构体中的偏移位置信息。
    /// </summary>
    public int ByteOffset { get; set; }

    /// <summary>
    /// 获取类型的文本描述信息
    /// </summary>
    /// <returns>文本信息</returns>
    public string GetTypeText()
    {
        var text = ArrayDimension switch
        {
            1 => ArrayLength[0] >= 0 ? $"[{ArrayLength[0]}]" : "[]",
            2 => $"[{ArrayLength[0]},{ArrayLength[1]}]",
            3 => $"[{ArrayLength[0]},{ArrayLength[1]},{ArrayLength[2]}]",
            _ => string.Empty
        };

        if (IsStruct)
        {
            return "struct" + text;
        }

        if (SymbolType == 8)
        {
            return "date" + text;
        }
        if (SymbolType == 9)
        {
            return "time" + text;
        }
        if (SymbolType == 10)
        {
            return "timeAndDate" + text;
        }
        if (SymbolType == 11)
        {
            return "timeOfDate" + text;
        }
        if (SymbolType == 193)
        {
            return "bool" + text;
        }
        if (SymbolType == 194)
        {
            return "sbyte" + text;
        }
        if (SymbolType == 195)
        {
            return "short" + text;
        }
        if (SymbolType == 196)
        {
            return "int" + text;
        }
        if (SymbolType == 197)
        {
            return "long" + text;
        }
        if (SymbolType == 198)
        {
            return "byte" + text;
        }
        if (SymbolType == 199)
        {
            return "ushort" + text;
        }
        if (SymbolType == 200)
        {
            return "uint" + text;
        }
        if (SymbolType == 201)
        {
            return "ulong" + text;
        }
        if (SymbolType == 202)
        {
            return "float" + text;
        }
        if (SymbolType == 203)
        {
            return "double" + text;
        }
        if (SymbolType == 204)
        {
            return "struct";
        }
        if (SymbolType == 208)
        {
            return "string";
        }
        if (SymbolType == 209)
        {
            return "byte-str";
        }
        if (SymbolType == 210)
        {
            return "word-str";
        }
        if (SymbolType == 211)
        {
            if (ArrayDimension == 0)
            {
                return "bool[32]";
            }
            if (ArrayDimension == 1)
            {
                return "bool" + $"[{ArrayLength[0] * 32}]";
            }
            return "bool-str" + text;
        }
        if ((SymbolType | 0xF00) == 4033)
        {
            return "bool";
        }
        return "";
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    private void SetSymbolType(ushort value)
    {
        ArrayDimension = (value & 0x4000) == 16384 ? 2 : (value & 0x2000) == 8192 ? 1 : 0;
        IsStruct = (value & 0x8000) == 32768;
        SymbolType = (ushort)(value & 0xFFFu);
    }

    /// <summary>
    /// 从指定的原始字节的数据中，解析出实际的节点信息
    /// </summary>
    /// <param name="source">原始字节数据</param>
    /// <param name="index">起始的索引</param>
    /// <returns>标签信息</returns>
    public static AbTagItem PraseAbTagItem(byte[] source, ref int index)
    {
        var abTagItem = new AbTagItem
        {
            InstanceID = BitConverter.ToUInt32(source, index)
        };
        index += 4;
        var num = BitConverter.ToUInt16(source, index);
        index += 2;
        abTagItem.Name = Encoding.ASCII.GetString(source, index, num);
        index += num;
        abTagItem.SetSymbolType(BitConverter.ToUInt16(source, index));
        index += 2;
        abTagItem.ArrayLength[0] = BitConverter.ToInt32(source, index);
        index += 4;
        abTagItem.ArrayLength[1] = BitConverter.ToInt32(source, index);
        index += 4;
        abTagItem.ArrayLength[2] = BitConverter.ToInt32(source, index);
        index += 4;
        return abTagItem;
    }

    /// <summary>
    /// 从指定的原始字节的数据中，解析出实际的标签数组，如果是系统保留的数组，或是__开头的，则自动忽略。
    /// </summary>
    /// <param name="source">原始字节数据</param>
    /// <param name="index">起始的索引</param>
    /// <param name="isGlobalVariable">是否局部变量</param>
    /// <param name="instance">输出最后一个标签的实例ID</param>
    /// <returns>标签信息</returns>
    public static List<AbTagItem> PraseAbTagItems(byte[] source, int index, bool isGlobalVariable, out uint instance)
    {
        var list = new List<AbTagItem>();
        instance = 0u;
        while (index < source.Length)
        {
            var abTagItem = PraseAbTagItem(source, ref index);
            instance = abTagItem.InstanceID;
            if ((abTagItem.SymbolType & 0x1000) != 4096 && !abTagItem.Name.StartsWith("__") && !abTagItem.Name.Contains(':'))
            {
                if (!isGlobalVariable)
                {
                    abTagItem.Name = "Program:MainProgram." + abTagItem.Name;
                }
                list.Add(abTagItem);
            }
        }
        return list;
    }

    /// <summary>
    /// 计算到达指定的字节的长度信息，可以用来计算固定分割符得字节长度
    /// </summary>
    /// <param name="source">原始字节数据</param>
    /// <param name="index">索引位置</param>
    /// <param name="value">等待判断的字节</param>
    /// <returns>字符串长度，如果不存在，返回-1</returns>
    private static int CalculatesSpecifiedCharacterLength(byte[] source, int index, byte value)
    {
        for (var i = index; i < source.Length; i++)
        {
            if (source[i] == value)
            {
                return i - index;
            }
        }
        return -1;
    }

    private static string CalculatesString(byte[] source, ref int index, byte value)
    {
        if (index >= source.Length)
        {
            return string.Empty;
        }
        var num = CalculatesSpecifiedCharacterLength(source, index, value);
        if (num < 0)
        {
            index = source.Length;
            return string.Empty;
        }
        var @string = Encoding.ASCII.GetString(source, index, num);
        index += num + 1;
        return @string;
    }

    /// <summary>
    /// 从结构体的数据中解析出实际的子标签信息
    /// </summary>
    /// <param name="source">原始字节</param>
    /// <param name="index">偏移索引</param>
    /// <param name="structHandle">结构体句柄</param>
    /// <returns>结果内容</returns>
    public static List<AbTagItem> PraseAbTagItemsFromStruct(byte[] source, int index, AbStructHandle structHandle)
    {
        var list = new List<AbTagItem>();
        var index2 = structHandle.MemberCount * 8 + index;
        _ = CalculatesString(source, ref index2, 0);
        for (var i = 0; i < structHandle.MemberCount; i++)
        {
            var abTagItem = new AbTagItem();
            abTagItem.ArrayLength[0] = BitConverter.ToUInt16(source, 8 * i + index);
            abTagItem.SetSymbolType(BitConverter.ToUInt16(source, 8 * i + index + 2));
            abTagItem.ByteOffset = BitConverter.ToInt32(source, 8 * i + index + 4) + 2;
            abTagItem.Name = CalculatesString(source, ref index2, 0);
            list.Add(abTagItem);
        }
        return list;
    }
}
