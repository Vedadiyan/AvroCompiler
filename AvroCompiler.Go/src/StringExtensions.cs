using System.Text;

namespace AvroCompiler.Go;

public static class StringExtensions
{
    public static string ToPascalCase(this string str)
    {
        if (str.Length > 0)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var i in str.Split('_')) {
                stringBuilder.Append(char.ToUpper(i[0])).Append(i.Substring(1, i.Length -1));
            }
            return stringBuilder.ToString();
        }
        return str;
    }
    public static string ToCamelCase(this string str)
    {
        if (str.Length > 0)
        {
            string tmp = str.ToPascalCase();
            return char.ToLower(tmp[0]) + tmp.Substring(1, str.Length - 1);
        }
        return str;
    }
}