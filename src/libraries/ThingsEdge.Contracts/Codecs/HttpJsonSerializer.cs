namespace ThingsEdge.Contracts.Codecs;

public sealed class HttpJsonSerializer : IHttpJsonSerializer
{
    public T To<T>(object obj)
    {
        if (obj is not JsonElement jsonElement)
        {
            throw new FormatException("参数值不能转换为JsonElement类型。");
        }

        return JsonObjectTo<T>(jsonElement);
    }

    public T[] ToArray<T>(object obj)
    {
        if (obj is not JsonElement jsonElement)
        {
            throw new FormatException("参数值不能转换为JsonElement类型。");
        }

        if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            var len = jsonElement.GetArrayLength();
            var arr = new List<T>(len);
            foreach (var item in jsonElement.EnumerateArray())
            {
                T obj2 = JsonObjectTo<T>(item);
                arr.Add(obj2);
            }

            return arr.ToArray();
        }

        return Array.Empty<T>();
    }

    /// <summary>
    /// 将 JSON 反序列化的对象转换为指定类型的基元对象。
    /// </summary>
    /// <typeparam name="T">要转换的基元类型</typeparam>
    /// <param name="jsonElement">Json 元素</param>
    /// <returns></returns>
    private static T JsonObjectTo<T>(JsonElement jsonElement)
    {
        object? obj = null;
        if (jsonElement.ValueKind == JsonValueKind.True)
        {
            obj = true;
        }
        else if (jsonElement.ValueKind == JsonValueKind.False)
        {
            obj = false;
        }
        else if (jsonElement.ValueKind == JsonValueKind.Number)
        {
            if (typeof(T) == typeof(byte))
            {
                obj = jsonElement.GetByte();
            }
            else if (typeof(T) == typeof(sbyte))
            {
                obj = jsonElement.GetSByte();
            }
            else if (typeof(T) == typeof(ushort))
            {
                obj = jsonElement.GetUInt16();
            }
            else if (typeof(T) == typeof(short))
            {
                obj = jsonElement.GetInt16();
            }
            else if (typeof(T) == typeof(uint))
            {
                obj = jsonElement.GetUInt32();
            }
            else if (typeof(T) == typeof(int))
            {
                obj = jsonElement.GetInt32();
            }
            else if (typeof(T) == typeof(float))
            {
                obj = jsonElement.GetSingle();
            }
            else if (typeof(T) == typeof(double))
            {
                obj = jsonElement.GetDouble();
            }
        }
        else if (jsonElement.ValueKind == JsonValueKind.String)
        {
            if (typeof(T) == typeof(string))
            {
                obj = jsonElement.GetString();
            }
        }

        if (obj is null)
        {
            throw new FormatException($"JsonElement值不能转换为{typeof(T).Name}类型。");
        }

        return (T)obj;
    }
}
