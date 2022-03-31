using System.Text;
using System.Text.RegularExpressions;
using AvroCompiler.Core.Contants;
using AvroCompiler.Core.Storage;

namespace AvroCompiler.Go;

public delegate string GetType(AvroTypes avroType);
public class AutoMappers
{
    private readonly AvroTypes avroType;
    private readonly string? recordName;
    private readonly string? fieldName;
    private readonly string? fieldItem;
    private readonly string? fieldGenericParameter;
    private readonly int? dimensions;
    private readonly GetType getType;
    public AutoMappers(AvroTypes avroType, string recordName, string fieldName, GetType getType)
    {
        this.avroType = avroType;
        this.recordName = recordName;
        this.fieldName = fieldName;
        this.getType = getType;
    }
    public AutoMappers(string recordName, string fieldName, string fieldItem, string fieldGenericParameter, int dimension, GetType getType)
    {
        this.recordName = recordName;
        this.fieldName = fieldName;
        this.fieldItem = fieldItem;
        this.fieldGenericParameter = fieldGenericParameter;
        this.dimensions = dimension;
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
    public string GetBackwardMapper()
    {
        if (avroType != AvroTypes.UNDEFINED)
        {
            return getBackwardedPrimitiveNonArrayType();
        }
        else
        {
            return getBackwardedPimitiveArrayType();
        }
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
        if ((avroType | AvroTypes.REFERENCE) != AvroTypes.REFERENCE)
        {
            return @$"
            if value, ok := value[""{fieldName}""].({getType(avroType)}); ok {{
                {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = value
            }} else {{
                // fmt.println(""WARNING: Type mismatch for"", ""{fieldName}"")
            }}
        ";
        }
        else
        {
            if (Types.Current.Value.TryGetType(fieldName, out HighOrderType highOrderType))
            {
                if (highOrderType == HighOrderType.RECORD)
                {
                    return @$"
                        if value, ok := value[""{fieldName}""].(map[string]any); ok {{
                            {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = new{fieldName.ToPascalCase()}(value)
                        }} else {{
                            // fmt.println(""WARNING: Type mismatch for"", ""{fieldName}"")
                        }}
                    ";
                }
                else {
                      return @$"
                        if value, ok := value[""{fieldName}""].({fieldName.ToPascalCase()}); ok {{
                            {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = value
                        }} else {{
                            // fmt.println(""WARNING: Type mismatch for"", ""{fieldName}"")
                        }}
                    ";                  
                }
            }
            else {
                throw new ArgumentException();
            }
        }
    }
    public string getBackwardedPimitiveArrayType()
    {
        string type = "";
        bool isPrimitiveType;
        if (Enum.TryParse<AvroTypes>(fieldItem.ToUpper(), out AvroTypes avroType))
        {
            type = getType(avroType);
            isPrimitiveType = true;
        }
        else
        {
            type = fieldItem;
            isPrimitiveType = false;
        }
        string getArrayDimensions(int d)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = d; i < dimensions; i++)
            {
                stringBuilder.Append("[]");
            }
            return stringBuilder.ToString();
        }
        string getAssignment(int d)
        {
            if (d < dimensions - 1)
            {
                return @$"
                    tmp{d}[index] = tmp{d + 1}
                ";
            }
            else
            {
                if (isPrimitiveType)
                {
                    return @$"
                        tmp{d}[index] = value
                    ";
                }
                else
                {
                    return @$"
                        tmp{d}[index] = new{recordName.ToPascalCase()}(value)
                    ";
                }
            }
        }
        string output = "$next";
        for (int i = 0; i < dimensions; i++)
        {
            if (i == 0)
            {
                string tmp = @$"
                    if value, ok := value[""{fieldName}""].([]any); ok {{
                        tmp{i} := make({getArrayDimensions(i)}{type}, len(value))
                        for index, value := range value {{
                            $next    
                        }}
                        {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = tmp{i}
                    }}
                ";
                output = output.Replace("$next", tmp);

            }
            else if (i < dimensions - 1)
            {
                string tmp = @$"
                    if value, ok := value.([]any); ok {{
                        tmp{i} := make({getArrayDimensions(i)}{type}, len(value))
                        for index, value := range value {{
                            $next    
                        }}
                        {getAssignment(i - 1)}
                    }}
                ";
                output = output.Replace("$next", tmp);
            }
            else
            {
                string tmp = @$"
                    if value, ok := value.([]any); ok {{
                        tmp{i} := make({getArrayDimensions(i)}{type}, len(value))
                        for index, value := range value {{
                            if value, ok := value.({type}); ok {{
                                {getAssignment(i)}
                            }}
                        }}
                        {getAssignment(i - 1)}
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