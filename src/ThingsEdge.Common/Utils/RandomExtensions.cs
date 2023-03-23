namespace ThingsEdge.Common.Utils;

public static class RandomExtensions
{
    public static long NextLong(this Random random) => random.Next() << 32 & unchecked((uint)random.Next());
}
