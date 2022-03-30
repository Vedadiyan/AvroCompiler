using System.Text;
using System.Text.RegularExpressions;
using AvroCompiler.Core.Contants;

namespace AvroCompiler.Go;

public delegate string GetType(AvroTypes avroType);
public class AutoMappers
{
    private readonly AvroTypes avroType;
    private readonly string recordName;
    private readonly string fieldName;
    private readonly string? fieldItem;
    private readonly string? fieldGenericParameter;
    private readonly GetType getType;
    public AutoMappers(AvroTypes avroType, string recordName, string fieldName, GetType getType)
    {
        this.avroType = avroType;
        this.recordName = recordName;
        this.fieldName = fieldName;
        this.getType = getType;
    }
    public string GetForwardMapper()
    {
        if ((avroType & AvroTypes.REFERENCE) != AvroTypes.REFERENCE)
        {
            return getForwardedPrimitiveType();
        }
        else
        {
            return getForwardedReferenceType();
        }
    }
    public void GetBackwardMapper()
    {

    }
    private string getForwardedPrimitiveType()
    {
        return @$"_map[""{fieldName}""] = {recordName.ToCamelCase()}.{fieldName.ToPascalCase()}";
    }
    private string getForwardedReferenceType()
    {
        return @$"_map[""{fieldName}""] = {recordName.ToCamelCase()}.{fieldName.ToPascalCase()}.ToMap()";
    }
    private string getBackwardedPrimitiveNonArrayType()
    {
        return @$"
            if value, ok := _map[""{fieldName}""].({getType(avroType)}); ok {{
                {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = value
            }} else {{
                // fmt.println(""WARNING: Type mismatch for"", ""{fieldName}"")
            }}
        ";
    }
    public string getBackwardedPimitiveArrayType(int dimension, AvroTypes avroType)
    {
        string getArrayDimensions(int d)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = d; i < dimension; i++)
            {
                stringBuilder.Append("[]");
            }
            return stringBuilder.ToString();
        }
        string getAssignment(int d)
        {
            if (d < dimension - 1)
            {
                return @$"
                    tmp{d}[index] = tmp{d + 1}
                ";
            }
            return @$"
                tmp{d}[index] = value
            ";
        }
        string output = "$next";
        for (int i = 0; i < dimension; i++)
        {
            if (i == 0)
            {
                string tmp = @$"
                    if value, ok := value.([]any); ok {{
                        tmp{i} := make({getArrayDimensions(i)}{getType(avroType)}, len(value))
                        for index, value := range value {{
                            $next    
                        }}
                    }}
                ";
                output = output.Replace("$next", tmp);

            }
            else if (i < dimension - 1)
            {
                string tmp = @$"
                    if value, ok := value.([]any); ok {{
                        tmp{i} := make({getArrayDimensions(i)}{getType(avroType)}, len(value))
                        for index, value := range value {{
                            $next    
                        }}
                        {getAssignment(i-1)}
                    }}
                ";
                output = output.Replace("$next", tmp);
            }
            else
            {
                string tmp = @$"
                    if value, ok := value.([]any); ok {{
                        tmp{i} := make({getArrayDimensions(i)}{getType(avroType)}, len(value))
                        for index, value := range value {{
                            if value, ok := value.({getType(avroType)}); ok {{
                                {getAssignment(i)}
                            }}
                        }}
                        {getAssignment(i-1)}
                    }}
                ";
                output = output.Replace("$next", tmp);
            }
        }
        Regex whitespace = new Regex("(  +)");
        Regex newLine = new Regex(@"(^\n\n+)");
        return newLine.Replace(whitespace.Replace(output, " "), "\r\n");
    }
}