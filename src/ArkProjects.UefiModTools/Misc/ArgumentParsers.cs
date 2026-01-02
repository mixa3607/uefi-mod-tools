using System.CommandLine.Parsing;

namespace ArkProjects.UefiModTools.Misc;

public class ArgumentParsers
{
    public static T NumberParser<T>(ArgumentResult result) where T : struct
    {
        if (result.Tokens.Count == 0)
        {
            return default;
        }
        else if (result.Tokens.Count > 1)
        {
            result.AddError($"Argument --{result.Argument.Name} expects one argument but got {result.Tokens.Count}");
            return default;
        }

        var numBase = 10;
        var numStr = result.Tokens[0].Value.ToLowerInvariant();
        if (numStr.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            numBase = 16;
        }
        else if (numStr.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
        {
            numBase = 2;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (targetType == typeof(byte))
            return (T)(object)Convert.ToByte(numStr, numBase);
        else if (targetType == typeof(sbyte))
            return (T)(object)Convert.ToSByte(numStr, numBase);

        if (targetType == typeof(short))
            return (T)(object)Convert.ToInt16(numStr, numBase);
        else if (targetType == typeof(ushort))
            return (T)(object)Convert.ToUInt16(numStr, numBase);

        if (targetType == typeof(int))
            return (T)(object)Convert.ToInt32(numStr, numBase);
        else if (targetType == typeof(uint))
            return (T)(object)Convert.ToUInt32(numStr, numBase);

        if (targetType == typeof(long))
            return (T)(object)Convert.ToInt64(numStr, numBase);
        else if (targetType == typeof(ulong))
            return (T)(object)Convert.ToUInt64(numStr, numBase);

        result.AddError($"Argument --{result.Argument.Name} can not be parsed. " +
                        "One of formats expected: 0xDEADBEEF, 3735928559");
        return default;
    }
}
