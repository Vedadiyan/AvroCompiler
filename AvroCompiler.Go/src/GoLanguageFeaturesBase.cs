using System.Diagnostics;
using System.Reflection;
using System.Text;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;
using AvroCompiler.Core.Specifications;

namespace AvroCompiler.Go;

public abstract class GoLanguageFeaturesBase : ILanguageFeature
{
    private HashSet<string> types;
    private HashSet<string> codecList;
    private HashSet<string> codecs;
    private HashSet<string> protocols;
    public GoLanguageFeaturesBase()
    {
        types = new HashSet<string>();
        codecList = new HashSet<string>();
        codecs = new HashSet<string>();
        protocols = new HashSet<string>();
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
                    return @$"{name.ToPascalCase()} {dimensionBuilder.ToString()}{GetType(elementType)} `avro:""{jsonPropertyName}"" json:""{jsonPropertyName}""` {parseValidations(properties, options)}";
                }
                else
                {
                    return @$"{name.ToPascalCase()} {dimensionBuilder.ToString()}map[{elementGenericType}]any `avro:""{jsonPropertyName}"" json:""{jsonPropertyName}""` {parseValidations(properties, options)}";
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
                return @$"{name.ToPascalCase()} {dimensionBuilder.ToString()}{elementType} `avro:""{jsonPropertyName}"" json:""{jsonPropertyName}""` {parseValidations(properties, options)}";
            }
        }
        return $"{name.ToPascalCase()} {dimensionBuilder.ToString()}{elementType}";
    }

    public void RegisterCodec(string name, string schema, object? options)
    {
        if (codecList.Add(name))
        {
            string base46Schema = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(schema));
            string codec = @$"
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
            codecs.Add(codec);
        }
    }

    public string GetComments()
    {
        return @$"
/*                                        
    The following code has been automatically generated by the 
    AvroCompiler Utility 
    ----------------------------------------------------------
    WARNING:    Please DO NOT modify the content of this file.
    ----------------------------------------------------------
    Timestamp:  {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
    ----------------------------------------------------------
    License:    MIT License
    Author:     Pouya Vedadiyan
*/
        ";
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
                return @$"{name.ToPascalCase()} {GetType(type)} `avro:""{jsonPropertyName}"" json:""{jsonPropertyName}""` {parseValidations(properties, options)}";
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
                return @$"{name.ToPascalCase()} {nullable(actualType)}{type.ToPascalCase()} `avro:""{jsonPropertyName}"" json:""{jsonPropertyName}""` {parseValidations(properties, options)}";
            }
        }
        return $"{name.ToPascalCase()} {nullable(actualType)}{type.ToPascalCase()}";
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
                ""errors""
                ""time""

                goavro ""github.com/linkedin/goavro""
                nats ""github.com/nats-io/nats.go""
                log ""github.com/sirupsen/logrus""
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
                return @$"{name.ToPascalCase()} map[{GetType(elementType)}]any `avro:""{jsonPropertyName}"" json:""{jsonPropertyName}""` {parseValidations(properties, options)}";
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
                return @$"{name.ToPascalCase()} map[{elementType}]any `avro:""{jsonPropertyName}"" json:""{jsonPropertyName}""` {parseValidations(properties, options)}";
            }
        }
        return $"{name.ToPascalCase()} map[{elementType}]any";
    }
    public abstract string GetMessage(string name, IReadOnlyDictionary<string, string> request, string response, object? options);
    public void RegisterProtocol(string protocol)
    {
        protocols.Add($"package {protocol}");
    }

    public string GetRecord(string name, AvroElement[] fields, object? options)
    {
        if (types.Add(name))
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder constructor = new StringBuilder();
            StringBuilder toMap = new StringBuilder();
            string namePascalCase = name.ToPascalCase();
            string nameCamelCase = name.ToCamelCase();
            stringBuilder.AppendLine($"type {namePascalCase} struct {{");
            string constructorFunction = @$"func new{namePascalCase}(value map[string]any) {namePascalCase} {{
                {nameCamelCase} := {namePascalCase} {{}}
                $next
                return {nameCamelCase}
            }}";
            string toMapFunction = @$"func ({nameCamelCase} {namePascalCase}) ToMap() map[string]any {{
                _map := make(map[string]any)
                $next
                return _map
            }}";
            foreach (var i in fields)
            {
                stringBuilder.AppendLine($"    {i.Template()}");
                if (i is AvroField avroField)
                {
                    constructor.AppendLine(new AutoMappers(avroField.AvroType, name, avroField.Name, avroField.SelectedType!, GetType).GetBackwardMapper());
                    toMap.AppendLine(new AutoMappers(avroField.AvroType, name, avroField.Name, avroField.SelectedType!, GetType).GetForwardMapper());
                }
                else if (i is AvroArray avroArray)
                {
                    constructor.AppendLine(new AutoMappers(name, avroArray.Name, avroArray.TypeNames![0], avroArray.ElementGenericType!, avroArray.Dimensions, GetType).GetBackwardMapper());
                    toMap.AppendLine(new AutoMappers(name, avroArray.Name, avroArray.TypeNames![0], avroArray.ElementGenericType!, avroArray.Dimensions, GetType).GetForwardMapper());
                }
                else if (i is AvroMap avroMap)
                {
                    constructor.AppendLine(new AutoMappers(AvroTypes.MAP, name, avroMap.Name, null!, GetType).GetBackwardMapper());
                    toMap.AppendLine(new AutoMappers(AvroTypes.MAP, name, avroMap.Name, null!, GetType).GetForwardMapper());
                }
            }
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine(constructorFunction.Replace("$next", constructor.ToString()));
            stringBuilder.AppendLine(toMapFunction.Replace("$next", toMap.ToString()));
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
                    return "time.Time";
                }
            case AvroTypes.TIMESTAMP | AvroTypes.NULL:
                {
                    return "*time.Time";
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

    public string GetNamespace()
    {
        return string.Join("\r\n", protocols);
    }

    public string GetCodec()
    {
        return string.Join("\r\n", codecs);
    }
    private string parseValidations(PropertyInfo[] properties, object options)
    {
        PropertyInfo? validations = properties.FirstOrDefault(x => x.Name == "Validations");
        object? value = validations?.GetValue(options);
        if(value != null) {
            List<string> values = ((List<string>)value);
            if(values.Count > 0) {
                return @$"validate:""{string.Join(",", values)}""";
            }
        }
        return string.Empty;
    }
}