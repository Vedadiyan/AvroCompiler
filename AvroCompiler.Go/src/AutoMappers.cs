using System.Text;
using System.Text.RegularExpressions;
using AvroCompiler.Core.Contants;
using AvroCompiler.Core.Storage;

using static AvroCompiler.Core.Lexicon;

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
    private readonly string? selectedType;
    private readonly GetType getType;
    public AutoMappers(AvroTypes avroType, string recordName, string fieldName, string selectedType, GetType getType)
    {
        this.avroType = avroType;
        this.recordName = recordName;
        this.fieldName = fieldName;
        this.selectedType = selectedType;
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
        if (selectedType != null && Types.Current.Value.TryGetType(selectedType!, out HighOrderType highOrderType))
        {
            if (highOrderType == HighOrderType.RECORD)
            {
                return getForwardedReferenceType();
            }
            else
            {
                return getForwardedPrimitiveType();
            }
        }
        else
        {
            return getForwardedPrimitiveType();
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
        string recordName = ShouldOr(this.recordName, new ArgumentNullException());
        string fieldName = ShouldOr(this.fieldName, new ArgumentNullException());
        return @$"_map[""{fieldName}""] = {recordName.ToCamelCase()}.{fieldName.ToPascalCase()}";
    }
    private string getForwardedReferenceType()
    {
        string recordName = ShouldOr(this.recordName, new ArgumentNullException());
        string fieldName = ShouldOr(this.fieldName, new ArgumentNullException());
        return @$"_map[""{fieldName}""] = {recordName.ToCamelCase()}.{fieldName.ToPascalCase()}.ToMap()";
    }
    private string getBackwardedPrimitiveNonArrayType()
    {
        string getSetter(string recordName, string fieldName, string type, Func<string> setter)
        {
            return @$"
                    if value, ok := value[""{fieldName}""].({type}); ok {{
                        {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = {setter()}
                    }} else {{
                        log.WithFields(log.Fields{{
                            ""Field Name"": ""{fieldName.ToPascalCase()}"",
                        }}).Warning(""Type Mismatch"")
                    }}
                ";
        }
        string recordName = ShouldOr(this.recordName, new ArgumentNullException());
        string fieldName = ShouldOr(this.fieldName, new ArgumentNullException());
        //string selectedType = ShouldOr(this.selectedType, new ArgumentNullException());
        if ((avroType & AvroTypes.REFERENCE) != AvroTypes.REFERENCE)
        {
            if (avroType != AvroTypes.MAP)
            {
                return getSetter(recordName, fieldName, getType(avroType), () => "value");
            }
            else
            {
                return getSetter(recordName, fieldName, "map[string]any", () => "value");
            }
        }
        else
        {
            if (Types.Current.Value.TryGetType(selectedType!, out HighOrderType highOrderType))
            {
                if (highOrderType == HighOrderType.RECORD)
                {
                    return getSetter(recordName, fieldName, "map[string]any", () => $"new{fieldName.ToPascalCase()}(value)");
                }
                else
                {
                    return getSetter(recordName, fieldName, selectedType!.ToPascalCase(), () => "value");
                }
            }
            else
            {
                if ((avroType & AvroTypes.NULL) == AvroTypes.NULL)
                {
                    return getSetter(recordName, fieldName, $"*{selectedType!.ToPascalCase()}", () => "value");
                }
                else
                {
                    return getSetter(recordName, fieldName, selectedType!.ToPascalCase(), () => "value");
                }
            }
        }
    }
    public string getBackwardedPimitiveArrayType()
    {
        string recordName = ShouldOr(this.recordName, new ArgumentNullException());
        string fieldName = ShouldOr(this.fieldName, new ArgumentNullException());
        string fieldItem = ShouldOr(this.fieldItem, new ArgumentNullException());
        string type = "";
        bool isPrimitiveType;
        if (Enum.TryParse<AvroTypes>(fieldItem.ToUpper(), out AvroTypes avroType))
        {
            if (avroType == AvroTypes.MAP)
            {
                type = "map";
                isPrimitiveType = true;
            }
            else
            {
                type = getType(avroType);
                isPrimitiveType = true;
            }
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
        if (dimensions == 1)
        {
            if (type != "map")
            {
                string tmp = @$"
                    if value, ok := value[""{fieldName}""].([]any); ok {{
                        tmp{0} := make({getArrayDimensions(0)}{type}, len(value))
                        for index, value := range value {{
                            if value, ok := value.({type}); ok {{
                                {getAssignment(0)}
                            }} else {{
                                log.WithFields(log.Fields{{
                                    ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                }}).Warning(""Type Mismatch"")
                            }}
                        }}
                        {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = tmp{0}
                    }} else {{
                        log.WithFields(log.Fields{{
                            ""Field Name"": ""{fieldName.ToPascalCase()}"",
                        }}).Warning(""Type Mismatch"")
                    }}
                ";
                output = output.Replace("$next", tmp);
            }
            else
            {
                string tmp = @$"
                    if value, ok := value[""{fieldName}""].([]any); ok {{
                        tmp{0} := make({getArrayDimensions(0)}map[{fieldGenericParameter}]any, len(value))
                        for index, value := range value {{
                            if value, ok := value.(map[{fieldGenericParameter}]any); ok {{
                                {getAssignment(0)}
                            }} else {{
                                log.WithFields(log.Fields{{
                                    ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                }}).Warning(""Type Mismatch"")
                            }}
                        }}
                        {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = tmp{0}
                    }} else {{
                        log.WithFields(log.Fields{{
                            ""Field Name"": ""{fieldName.ToPascalCase()}"",
                        }}).Warning(""Type Mismatch"")
                    }}
                ";
                output = output.Replace("$next", tmp);
            }
        }
        else
        {
            for (int i = 0; i < dimensions; i++)
            {
                if (i == 0)
                {
                    if (type != "map")
                    {
                        string tmp = @$"
                            if value, ok := value[""{fieldName}""].([]any); ok {{
                                tmp{i} := make({getArrayDimensions(i)}{type}, len(value))
                                for index, value := range value {{
                                    $next    
                                }}
                                {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = tmp{i}
                            }} else {{
                                log.WithFields(log.Fields{{
                                    ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                }}).Warning(""Type Mismatch"")
                            }}
                        ";
                        output = output.Replace("$next", tmp);
                    }
                    else
                    {
                        string tmp = @$"
                            if value, ok := value[""{fieldName}""].([]any); ok {{
                                tmp{i} := make({getArrayDimensions(i)}map[{fieldGenericParameter}]any, len(value))
                                for index, value := range value {{
                                    $next    
                                }}
                                {recordName.ToCamelCase()}.{fieldName.ToPascalCase()} = tmp{i}
                            }} else {{
                                log.WithFields(log.Fields{{
                                    ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                }}).Warning(""Type Mismatch"")
                            }}
                        ";
                        output = output.Replace("$next", tmp);
                    }

                }
                else if (i < dimensions - 1)
                {
                    if (type != "map")
                    {
                        string tmp = @$"
                            if value, ok := value.([]any); ok {{
                                tmp{i} := make({getArrayDimensions(i)}{type}, len(value))
                                for index, value := range value {{
                                    $next    
                                }}
                                {getAssignment(i - 1)}
                            }} else {{
                                log.WithFields(log.Fields{{
                                    ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                }}).Warning(""Type Mismatch"")
                            }}
                        ";
                        output = output.Replace("$next", tmp);
                    }
                    else
                    {
                        string tmp = @$"
                            if value, ok := value.([]any); ok {{
                                tmp{i} := make({getArrayDimensions(i)}map[{fieldGenericParameter}]any, len(value))
                                for index, value := range value {{
                                    $next    
                                }}
                                {getAssignment(i - 1)}
                            }} else {{
                                log.WithFields(log.Fields{{
                                    ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                }}).Warning(""Type Mismatch"")
                            }}
                        ";
                        output = output.Replace("$next", tmp);
                    }
                }
                else
                {
                    if (type != "map")
                    {
                        string tmp = @$"
                            if value, ok := value.([]any); ok {{
                                tmp{i} := make({getArrayDimensions(i)}{type}, len(value))
                                for index, value := range value {{
                                    if value, ok := value.({type}); ok {{
                                        {getAssignment(i)}
                                    }} else {{
                                        log.WithFields(log.Fields{{
                                            ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                        }}).Warning(""Type Mismatch"")
                                    }}
                                }}
                                {getAssignment(i - 1)}
                            }} else {{
                                log.WithFields(log.Fields{{
                                    ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                }}).Warning(""Type Mismatch"")
                            }}
                        ";
                        output = output.Replace("$next", tmp);
                    }
                    else
                    {
                        string tmp = @$"
                            if value, ok := value.([]any); ok {{
                                tmp{i} := make({getArrayDimensions(i)}map[{fieldGenericParameter}]any, len(value))
                                for index, value := range value {{
                                    if value, ok := value.({type}); ok {{
                                        {getAssignment(i)}
                                    }} else {{
                                        log.WithFields(log.Fields{{
                                            ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                        }}).Warning(""Type Mismatch"")
                                    }}
                                }}
                                {getAssignment(i - 1)}
                            }} else {{
                                log.WithFields(log.Fields{{
                                    ""Field Name"": ""{fieldName.ToPascalCase()}"",
                                }}).Warning(""Type Mismatch"")
                            }}
                        ";
                        output = output.Replace("$next", tmp);
                    }
                }
            }
        }
        Regex whitespace = new Regex("(  +)");
        Regex newLine = new Regex(@"(^\n\n+)");
        return newLine.Replace(whitespace.Replace(output, " "), "\r\n");
    }
}