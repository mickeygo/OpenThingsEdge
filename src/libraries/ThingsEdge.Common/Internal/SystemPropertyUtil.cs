namespace ThingsEdge.Common.Internal;

public static class SystemPropertyUtil
{
    /// <summary>
    ///     Returns <c>true</c> if and only if the system property with the specified <c>key</c>
    ///     exists.
    /// </summary>
    public static bool Contains(string key) => Get(key) != null;

    /// <summary>
    ///     Returns the value of the system property with the specified
    ///     <c>key</c>, while falling back to <c>null</c> if the property access fails.
    /// </summary>
    /// <returns>the property value or <c>null</c></returns>
    public static string? Get(string key) => Get(key, null);

    /// <summary>
    ///     Returns the value of the system property with the specified
    ///     <c>key</c>, while falling back to the specified default value if
    ///     the property access fails.
    /// </summary>
    /// <returns>
    ///     the property value.
    ///     <c>def</c> if there's no such property or if an access to the
    ///     specified property is not allowed.
    /// </returns>
    public static string? Get(string key, string? def)
    {
        Contract.Requires(!string.IsNullOrEmpty(key));

        try
        {
            return Environment.GetEnvironmentVariable(key) ?? def;
        }
        catch
        {
            return def;
        }
    }

    /// <summary>
    ///     Returns the value of the system property with the specified
    ///     <c>key</c>, while falling back to the specified default value if
    ///     the property access fails.
    /// </summary>
    /// <returns>
    ///     the property value or <c>def</c> if there's no such property or
    ///     if an access to the specified property is not allowed.
    /// </returns>
    public static bool GetBoolean(string key, bool def)
    {
        string? value = Get(key);
        if (value == null)
        {
            return def;
        }

        value = value.Trim();
        if (value.Length == 0)
        {
            return true;
        }

        if ("true".Equals(value, StringComparison.OrdinalIgnoreCase)
            || "yes".Equals(value, StringComparison.OrdinalIgnoreCase)
            || "1".Equals(value, StringComparison.Ordinal))
        {
            return true;
        }

        if ("false".Equals(value, StringComparison.OrdinalIgnoreCase)
            || "no".Equals(value, StringComparison.OrdinalIgnoreCase)
            || "0".Equals(value, StringComparison.Ordinal))
        {
            return false;
        }

        return def;
    }

    /// <summary>
    ///     Returns the value of the system property with the specified
    ///     <c>key</c>, while falling back to the specified default value if
    ///     the property access fails.
    /// </summary>
    /// <returns>
    ///     the property value.
    ///     <c>def</c> if there's no such property or if an access to the
    ///     specified property is not allowed.
    /// </returns>
    public static int GetInt(string key, int def)
    {
        string? value = Get(key);
        if (value == null)
        {
            return def;
        }

        value = value.Trim();
        if (!int.TryParse(value, out int result))
        {
            result = def;
        }
        return result;
    }

    /// <summary>
    ///     Returns the value of the system property with the specified
    ///     <c>key</c>, while falling back to the specified default value if
    ///     the property access fails.
    /// </summary>
    /// <returns>
    ///     the property value.
    ///     <c>def</c> if there's no such property or if an access to the
    ///     specified property is not allowed.
    /// </returns>
    public static long GetLong(string key, long def)
    {
        string? value = Get(key);
        if (value == null)
        {
            return def;
        }

        if (!long.TryParse(value, out long result))
        {
            result = def;
        }
        return result;
    }

}
