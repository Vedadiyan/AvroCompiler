using System.Text;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;

namespace AvroCompiler;

public class GoLanguageFeatures : ILanguageFeature
{
    public string GetArray(AvroTypes elementType, string name, object? options)
    {
        return $"{toPascalCase(name)} []{GetType(elementType)}";
    }
    public string GetArray(string elementType, string name, object? options)
    {
        return $"{toPascalCase(name)} []{elementType}";
    }

    public string GetCodec(string name, string schema, object? options)
    {
        string base46Schema = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(schema));
        return @$"
func ({toCamelCase(name)} {toPascalCase(name)}) Codec() (*goavro.Codec, error){{
    avroSchemaInBase64 := ""{base46Schema}""
    if value, err := base64.StdEncoding.DecodeString(avroSchemaInBase64); err == nil {{
        if codec, err := goavro.NewCodec(value); err == nil {{
            return codec, nil
        }} else {{
            return err, nil
        }}
    }} else {{
        return err, nil
    }}
}}
        ";
    }

    public string GetEnum(string name, string[] symbols, object? options)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"type {toPascalCase(name)} int");
        stringBuilder.AppendLine("const (");
        for (int i = 0; i < symbols.Length; i++)
        {
            stringBuilder.AppendLine($"    {symbols[i]} {toPascalCase(name)} = {i}");
        }
        stringBuilder.AppendLine(")");
        return stringBuilder.ToString();
    }

    public string GetField(AvroTypes type, string name, object? options)
    {
        return $"{toPascalCase(name)} {GetType(type)}";
    }

    public string GetField(string type, AvroTypes actualType, string name, object? options)
    {
        return $"{toPascalCase(name)} {nullable(actualType)}{type}";
    }

    public string GetFixed(string name, object? options)
    {
        return $"type {toPascalCase(name)} string";
    }

    public string GetImports()
    {
        return @"
import (
	""github.com/linkedin/goavro""
)
        ";
    }

    public string GetMessage(string name, IReadOnlyDictionary<string, string> request, string response, object? options)
    {

        return @$"
func ${toPascalCase(name)}() {{
    
}}
        ";
    }

    public string GetNamespace(string @namespace)
    {
        return $"package {@namespace}";
    }

    public string GetRecord(string name, string[] fields, object? options)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"type {toPascalCase(name)} struct {{");
        foreach (var i in fields)
        {
            stringBuilder.AppendLine($"    {i}");
        }
        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }

    public string GetType(AvroTypes type)
    {
        switch (type)
        {
            case AvroTypes.UNION:
            case AvroTypes.UNION | AvroTypes.NULL:
                {
                    return "any";
                }
            case AvroTypes.BOOLEAN:
                {
                    return "boolean";
                }
            case AvroTypes.BOOLEAN | AvroTypes.NULL:
                {
                    return "*boolean";
                }
            case AvroTypes.BYTES:
                {
                    return "[]byte";
                }
            case AvroTypes.DOUBLE:
                {
                    return "double";
                }
            case AvroTypes.DOUBLE | AvroTypes.NULL:
                {
                    return "*double";
                }
            case AvroTypes.FLOAT:
                {
                    return "float";
                }
            case AvroTypes.FLOAT | AvroTypes.NULL:
                {
                    return "*float";
                }
            case AvroTypes.INT:
                {
                    return "int";
                }
            case AvroTypes.INT | AvroTypes.NULL:
                {
                    return "*int";
                }
            case AvroTypes.STRING:
                {
                    return "string";
                }
            case AvroTypes.STRING | AvroTypes.NULL:
                {
                    return "*string";
                }
            case AvroTypes.LONG:
                {
                    return "int64";
                }
            case AvroTypes.LONG | AvroTypes.NULL:
                {
                    return "*long";
                }
            case AvroTypes.TIMESTAMP:
                {
                    return "time";
                }
        }
        throw new ArgumentException();
    }

    private string nullable(AvroTypes type)
    {
        if ((type & AvroTypes.NULL) == AvroTypes.NULL)
        {
            return "*";
        }
        else
        {
            return "";
        }
    }
    private string toPascalCase(string name)
    {
        return char.ToUpper(name[0]) + name.Substring(1, name.Length - 1);
    }
    private string toCamelCase(string name)
    {
        return char.ToLower(name[0]) + name.Substring(1, name.Length - 1);
    }
}