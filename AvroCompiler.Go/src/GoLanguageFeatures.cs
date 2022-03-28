using System.Diagnostics;
using System.Reflection;
using System.Text;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;

namespace AvroCompiler.Go;

public class GoLanguageFeatures : ILanguageFeature
{
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

    public string GetArray(AvroTypes elementType, string name, object? options)
    {
        return $"{name.ToPascalCase()} []{GetType(elementType)}";
    }
    public string GetArray(string elementType, string name, object? options)
    {
        return $"{name.ToPascalCase()} []{elementType}";
    }

    public string GetCodec(string name, string schema, object? options)
    {
        string base46Schema = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(schema));
        return @$"
func ({name.ToCamelCase()} {name.ToPascalCase()}) Codec() (*goavro.Codec, error) {{
    avroSchemaInBase64 := ""{schema}""
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

    public string GetEnum(string name, string[] symbols, object? options)
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

    public string GetField(AvroTypes type, string name, object? options)
    {
        return $"{name.ToPascalCase()} {GetType(type)}";
    }

    public string GetField(string type, AvroTypes actualType, string name, object? options)
    {
        return $"{name.ToPascalCase()} {nullable(actualType)}{type}";
    }

    public string GetFixed(string name, object? options)
    {
        return $"type {name.ToPascalCase()} string";
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

    public string GetRecord(string name, string[] fields, object? options)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"type {name.ToPascalCase()} struct {{");
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
}