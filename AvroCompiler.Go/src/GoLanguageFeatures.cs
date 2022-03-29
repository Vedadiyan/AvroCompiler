using System.Diagnostics;
using System.Reflection;
using System.Text;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;
using AvroCompiler.Core.Specifications;

namespace AvroCompiler.Go;

public class GoLanguageFeatures : ILanguageFeature
{
    private HashSet<string> types;
    private HashSet<string> codecs;
    public GoLanguageFeatures()
    {
        types = new HashSet<string>();
        codecs = new HashSet<string>();
    }
    public async Task<string> Format(string input)
    {
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, input);
        ProcessStartInfo processStartInfo = new ProcessStartInfo("gofmt", $"-w {tempFile}");
        Process? process = Process.Start(processStartInfo);
        if (process != null)
        {
            process.Start();
            await process.WaitForExitAsync();
            string output = File.ReadAllText(tempFile);
            File.Delete(tempFile);
            return output;
        }
        throw new Exception("Could not format the code");
    }

    public string GetArray(int dimensions, AvroTypes elementType, string? elementGenericType, string name, object? options)
    {
        StringBuilder dimensionBuilder = new StringBuilder();
        for (int i = 0; i < dimensions; i++)
        {
            dimensionBuilder.Append("[]");
        }
        if (options != null)
        {
            PropertyInfo[] properties = options.GetType().GetProperties();
            PropertyInfo? jsonPropertyNameProperty = properties.FirstOrDefault(x => x.Name == "JsonPropertyName");
            if (jsonPropertyNameProperty != null)
            {
                string jsonPropertyName = (string)jsonPropertyNameProperty.GetValue(options)!;
                if (elementType != AvroTypes.MAP)
                {
                    return @$"{name.ToPascalCase()} {dimensionBuilder.ToString()}{GetType(elementType)} `avro:""{jsonPropertyName}""`";
                }
                else
                {
                    return @$"{name.ToPascalCase()} {dimensionBuilder.ToString()}map[{elementGenericType}]any `avro:""{jsonPropertyName}""`";
                }
            }
        }
        if (elementType != AvroTypes.MAP)
        {
            return $"{name.ToPascalCase()} {dimensionBuilder.ToString()}{GetType(elementType)}";
        }
        else
        {
            return $"{name.ToPascalCase()} {dimensionBuilder.ToString()}map[{elementGenericType}]any";
        }
    }
    public string GetArray(int dimensions, string elementType, string name, object? options)
    {
        StringBuilder dimensionBuilder = new StringBuilder();
        for (int i = 0; i < dimensions; i++)
        {
            dimensionBuilder.Append("[]");
        }
        if (options != null)
        {
            PropertyInfo[] properties = options.GetType().GetProperties();
            PropertyInfo? jsonPropertyNameProperty = properties.FirstOrDefault(x => x.Name == "JsonPropertyName");
            if (jsonPropertyNameProperty != null)
            {
                string jsonPropertyName = (string)jsonPropertyNameProperty.GetValue(options)!;
                return @$"{name.ToPascalCase()} {dimensionBuilder.ToString()}{elementType} `avro:""{jsonPropertyName}""`";
            }
        }
        return $"{name.ToPascalCase()} {dimensionBuilder.ToString()}{elementType}";
    }

    public string GetCodec(string name, string schema, object? options)
    {
        if (codecs.Add(name))
        {
            string base46Schema = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(schema));
            return @$"
func ({name.ToCamelCase()} {name.ToPascalCase()}) Codec() (*goavro.Codec, error) {{
    avroSchemaInBase64 := ""{base46Schema}""
    if value, err := base64.StdEncoding.DecodeString(avroSchemaInBase64); err == nil {{
        if codec, err := goavro.NewCodec(string(value)); err == nil {{
            return codec, nil
        }} else {{
            return nil, err
        }}
    }} else {{
        return nil, err
    }}
}}
        ";
        }
        return string.Empty;
    }

    public string GetEnum(string name, string[] symbols, object? options)
    {
        if (types.Add(name))
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"type {name.ToPascalCase()} int");
            stringBuilder.AppendLine("const (");
            for (int i = 0; i < symbols.Length; i++)
            {
                stringBuilder.AppendLine($"    {symbols[i]} {name.ToPascalCase()} = {i}");
            }
            stringBuilder.AppendLine(")");
            return stringBuilder.ToString();
        }
        return string.Empty;
    }

    public string GetField(AvroTypes type, string name, object? options)
    {
        if (options != null)
        {
            PropertyInfo[] properties = options.GetType().GetProperties();
            PropertyInfo? jsonPropertyNameProperty = properties.FirstOrDefault(x => x.Name == "JsonPropertyName");
            if (jsonPropertyNameProperty != null)
            {
                string jsonPropertyName = (string)jsonPropertyNameProperty.GetValue(options)!;
                return @$"{name.ToPascalCase()} {GetType(type)} `avro:""{jsonPropertyName}""`";
            }
        }
        return $"{name.ToPascalCase()} {GetType(type)}";
    }

    public string GetField(string type, AvroTypes actualType, string name, object? options)
    {
        if (options != null)
        {
            PropertyInfo[] properties = options.GetType().GetProperties();
            PropertyInfo? jsonPropertyNameProperty = properties.FirstOrDefault(x => x.Name == "JsonPropertyName");
            if (jsonPropertyNameProperty != null)
            {
                string jsonPropertyName = (string)jsonPropertyNameProperty.GetValue(options)!;
                return @$"{name.ToPascalCase()} {nullable(actualType)}{type} `avro:""{jsonPropertyName}""`";
            }
        }
        return $"{name.ToPascalCase()} {nullable(actualType)}{type}";
    }

    public string GetFixed(string name, object? options)
    {
        if (types.Add(name))
        {
            return $"type {name.ToPascalCase()} string";
        }
        return string.Empty;
    }

    public string GetImports()
    {
        return @"
import (
    ""encoding/base64""

	""github.com/linkedin/goavro""
    ""github.com/nats-io/nats.go""
)
        ";
    }

    public string GetMap(AvroTypes elementType, string name, object? options)
    {
        if (options != null)
        {
            PropertyInfo[] properties = options.GetType().GetProperties();
            PropertyInfo? jsonPropertyNameProperty = properties.FirstOrDefault(x => x.Name == "JsonPropertyName");
            if (jsonPropertyNameProperty != null)
            {
                string jsonPropertyName = (string)jsonPropertyNameProperty.GetValue(options)!;
                return @$"{name.ToPascalCase()} map[{GetType(elementType)}]any `avro:""{jsonPropertyName}""`";
            }
        }
        return $"{name.ToPascalCase()} map[{GetType(elementType)}]any";
    }
    public string GetMap(string elementType, string name, object? options)
    {
        if (options != null)
        {
            PropertyInfo[] properties = options.GetType().GetProperties();
            PropertyInfo? jsonPropertyNameProperty = properties.FirstOrDefault(x => x.Name == "JsonPropertyName");
            if (jsonPropertyNameProperty != null)
            {
                string jsonPropertyName = (string)jsonPropertyNameProperty.GetValue(options)!;
                return @$"{name.ToPascalCase()} map[{elementType}]any `avro:""{jsonPropertyName}""`";
            }
        }
        return $"{name.ToPascalCase()} map[{elementType}]any";
    }
    public string GetMessage(string name, IReadOnlyDictionary<string, string> request, string response, object? options)
    {
        if (request.Count == 1)
        {
            PropertyInfo[]? _options = options?.GetType().GetProperties();
            StringBuilder output = new StringBuilder();
            if (response != "null" && Enum.TryParse<AvroTypes>(response.ToUpper(), out AvroTypes responseType))
            {
                return "";
            }
            string type = request[request.Keys.FirstOrDefault()!];
            if (!Enum.TryParse<AvroTypes>(type.ToUpper(), out AvroTypes avroType))
            {
                object? error = _options?.FirstOrDefault(x => x.Name == "Error")?.GetValue(options);
                NatsClientCreator natsClientCreator = new NatsClientCreator(name, "", type, response, error != null ? (string)error : null);
                output.AppendLine();
                output.AppendLine(natsClientCreator.GetFunctionType());
                output.Append(natsClientCreator.GetFunction());
            }
            return output.ToString();
        }
        else if (request.Count == 0)
        {
            PropertyInfo[]? _options = options?.GetType().GetProperties();
            StringBuilder output = new StringBuilder();
            if (response != "null" && Enum.TryParse<AvroTypes>(response.ToUpper(), out AvroTypes responseType))
            {
                return "";
            }
            object? error = _options?.FirstOrDefault(x => x.Name == "Error")?.GetValue(options);
            NatsClientCreator natsClientCreator = new NatsClientCreator(name, "", null, response, error != null ? (string)error : null);
            output.AppendLine();
            output.AppendLine(natsClientCreator.GetFunctionType());
            output.AppendLine(natsClientCreator.GetFunction());
            return output.ToString();
        }
        return "";
    }
    public string GetNamespace(string @namespace)
    {
        return $"package {@namespace}";
    }

    public string GetRecord(string name, AvroElement[] fields, object? options)
    {
        if (types.Add(name))
        {
            StringBuilder stringBuilder = new StringBuilder();
            //StringBuilder constructor = new StringBuilder();
            string namePascalCase = name.ToPascalCase();
            string nameCamelCase = name.ToCamelCase();
            stringBuilder.AppendLine($"type {namePascalCase} struct {{");
            foreach (var i in fields)
            {
                stringBuilder.AppendLine($"    {i.Template()}");
                // if (i is AvroField avroField)
                // {
                //     if (avroField.AvroType == AvroTypes.REFERENCE)
                //     {
                //         constructor.AppendLine(@$"
                //             if value, ok := args[""{i.Name.ToCamelCase()}""].(map[string]any); ok {{
                //                 {nameCamelCase}.{i.Name.ToPascalCase} = new{avroField.TypeNames![1]}(value)
                //             }} 
                //         ");
                //     }
                //     else if (avroField.AvroType == AvroTypes.MAP)
                //     {
                //         if (Enum.TryParse<AvroTypes>(avroField.TypeNames![1].ToUpper(), out AvroTypes type))
                //         {
                //             constructor.AppendLine(@$"
                //                 if value, ok := args[""{i.Name.ToCamelCase()}""].(map[{GetType(type)}]any); ok {{
                //                     {nameCamelCase}.{i.Name.ToPascalCase} = value
                //                 }} 
                //             ");
                //         }
                //         else
                //         {
                //             throw new Exception("Invalid Map Type");
                //         }
                //     }
                //     else
                //     {
                //         constructor.AppendLine(@$"
                //             if value, ok := args[""{i.Name.ToCamelCase()}""].({GetType(avroField.AvroType)}); ok {{
                //                 {nameCamelCase}.{i.Name.ToPascalCase()} = value
                //             }} 
                //         ");
                //     }
                // }
                // else if (i is AvroArray avroArray)
                // {
                //     for (int dimension = 0; dimension < avroArray.Dimensions; dimension++)
                //     {
                //         StringBuilder arrayBuilder = new StringBuilder();
                //         for (int x = dimension; x < avroArray.Dimensions; x++)
                //         {
                //             arrayBuilder.Append("[]");
                //         }
                  
                //     }
                // }
            }
            //var test = constructor.ToString();
            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }
        return string.Empty;
    }

    public string GetType(AvroTypes type)
    {
        switch (type)
        {
            case var t when (t & AvroTypes.UNION) == AvroTypes.UNION:
            case AvroTypes.NULL:
                {
                    return "any";
                }
            case AvroTypes.BOOLEAN:
                {
                    return "bool";
                }
            case AvroTypes.BOOLEAN | AvroTypes.NULL:
                {
                    return "*bool";
                }
            case AvroTypes.BYTES:
                {
                    return "[]byte";
                }
            case AvroTypes.DOUBLE:
                {
                    return "float64";
                }
            case AvroTypes.DOUBLE | AvroTypes.NULL:
                {
                    return "*float64";
                }
            case AvroTypes.FLOAT:
                {
                    return "float32";
                }
            case AvroTypes.FLOAT | AvroTypes.NULL:
                {
                    return "*float32";
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
                    return "*int64";
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
}