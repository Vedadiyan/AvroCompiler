namespace AvroCompiler.Go;

public static class StringExtensions
{
    public static string ToPascalCase(this string str)
    {
        if (str.Length > 0)
        {
            return char.ToUpper(str[0]) + str.Substring(1, str.Length - 1);
        }
        return str;
    }
    public static string ToCamelCase(this string str)
    {
        if (str.Length > 0)
        {
            return char.ToLower(str[0]) + str.Substring(1, str.Length - 1);
        }
        return str;
    }
}