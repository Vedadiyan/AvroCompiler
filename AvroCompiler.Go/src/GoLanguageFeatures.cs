using System.Reflection;
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
func ({toCamelCase(name)} {toPascalCase(name)}) Codec() (*goavro.Codec, error) {{
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
            string natsMessage = "";
            string type = request[request.Keys.FirstOrDefault()!];
            if (!Enum.TryParse<AvroTypes>(type.ToUpper(), out AvroTypes avroType))
            {
                object? error = _options?.FirstOrDefault(x => x.Name == "Error")?.GetValue(options);

                if (response != "null")
                {
                    if (error == null)
                    {
                        output.Append(@$"type {toPascalCase(name)} func({toCamelCase(type)} {toPascalCase(type)}) {response}");

                    }
                    else
                    {
                        output.Append(@$"type {toPascalCase(name)} func({toCamelCase(type)} {toPascalCase(type)}) ({response}, *{error})");
                    }
                }
                else
                {
                    output.Append(@$"type {toPascalCase(name)} func({toCamelCase(type)} {toPascalCase(type)}) *{error}");
                }
                natsMessage = getNatsListener(name, type, response, "", error != null ? (string)error : null);
                output.AppendLine();
                output.AppendLine(natsMessage);
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
            string natsMessage = "";
            object? error = _options?.FirstOrDefault(x => x.Name == "Error")?.GetValue(options);

            if (response != "null")
            {
                if (error == null)
                {
                    output.Append(@$"type {toPascalCase(name)} func() {response}");

                }
                else
                {
                    output.Append(@$"type {toPascalCase(name)} func() ({response}, *{error})");
                }
            }
            else
            {
                if (error == null)
                {
                    output.Append(@$"type {toPascalCase(name)} func()");
                }
                else
                {
                    output.Append(@$"type {toPascalCase(name)} func() *{error}");
                }
            }
            natsMessage = getNatsListener(name, null, response, "", error != null ? (string)error : null);
            output.AppendLine();
            output.AppendLine(natsMessage);
            return output.ToString();
        }
        return "";
    }
    private string getNatsListener(string name, string? request, string response, string @namespace, string? error)
    {
        string namePascalCase = toPascalCase(name);
        string nameCamelCase = toCamelCase(name);
        string mainTemplate = @$"
    $requestCodec
    $responseCodec
    $errorCodec
    $subscription
            ";
        string subscriptionTemplate =
@$"conn.Subscribe(""{@namespace}"", func(msg *nats.Msg) {{ 
        $handler
    }})
            ";
        mainTemplate = mainTemplate.Replace("$subscription", subscriptionTemplate);
        string? requestCodecTemplate = null;
        string? responseCodecTemplate = null;
        string? errorCodecTemplate = null;
        string? errorPascalCase = null;
        string? requestPascalCase = null;
        string? responsePascalCase = null;
        string? requestCamelCase = null;
        string handlerTemplate;
        if (request != null)
        {
            requestPascalCase = toPascalCase(request);
            requestCamelCase = toCamelCase(request);
            requestCodecTemplate =
@$"requestCodec, requestCodecError := {requestPascalCase}.Codec({requestPascalCase} {{}})
    if requestCodecError != nil {{
        panic(requestCodecError)
    }}";
        }
        if (error != null)
        {
            errorPascalCase = toPascalCase(error);
            errorCodecTemplate =
$@"errorCodec, errorCodecError := {errorPascalCase}.Codec({errorPascalCase} {{}})
    if errorCodecError != nil {{
        panic(errorCodecError)
    }}";
        }
        if (response != "null")
        {
            responsePascalCase = toPascalCase(response);
            responseCodecTemplate =
@$"responseCodec, responseCodedError := {responsePascalCase}.Codec({responsePascalCase} {{}})
    if responseCodedError != nil {{
        panic(responseCodedError)
    }}";
        }
        if (request != null)
        {
            mainTemplate = mainTemplate.Replace("$requestCodec", requestCodecTemplate);
            handlerTemplate =
@$"message, _, messageError := requestCodec.NativeFromBinary(msg.Data);
        if messageError != nil {{
            msg.Term()
        }} 
        if messageValue, ok := message.({requestPascalCase}); ok {{
            $handle
        }} else {{
            msg.Term()
        }}";
            mainTemplate =
@$"func ListenTo{namePascalCase}(conn *nats.Conn, {nameCamelCase} {namePascalCase}) {{
    {mainTemplate}
}}";
            if (response != "null")
            {
                mainTemplate = mainTemplate.Replace("$handler", handlerTemplate);
                mainTemplate = mainTemplate.Replace("$responseCodec", responseCodecTemplate);
                string responseTemplate;
                if (error != null)
                {
                    mainTemplate = mainTemplate.Replace("$errorCodec", errorCodecTemplate);
                    responseTemplate =
             @$"if response, responseError := {nameCamelCase}(messageValue); responseError == nil {{
                if responseEncoded, err := responseCodec.BinaryFromNative(nil, response); err == nil {{
                    msg.Respond(responseEncoded)
                }} else {{
                    msg.Term()
                }}
            }} else {{
                if errorEncoded, err := errorCodec.BinaryFromNative(nil, responseError); err == nil {{
                    msg.Respond(errorEncoded)
                }} else {{
                    msg.Term()
                }}
            }}";

                }
                else
                {
                    mainTemplate = mainTemplate.Replace("$errorCodec", "");
                    responseTemplate =
         @$"response := {nameCamelCase}(messageValue);
            if responseEncoded, err := responseCodec.BinaryFromNative(nil, response); err == nil {{
                msg.Respond(responseEncoded)
            }} else {{
                msg.Term()
            }}";
                }
                mainTemplate = mainTemplate.Replace("$handle", responseTemplate);
            }
            else
            {
                mainTemplate = mainTemplate.Replace("$responseCodec", "");
                handlerTemplate =
         @$"message, _, messageError := requestCodec.NativeFromBinary(msg.Data);
            if messageError != nil {{
                msg.Term();
            }} 
            if messageValue, ok := message.({requestPascalCase}); ok {{
                $handle
            }} else {{
                msg.Term()
            }}";
                mainTemplate = mainTemplate.Replace("$handler", handlerTemplate);
                string handleTemplate;
                if (error != null)
                {
                    mainTemplate = mainTemplate.Replace("$errorCodec", "");
                    handleTemplate =
          @$"if error := {nameCamelCase}(messageValue); error == nil {{
                msg.Ack()
            }} else {{
                msg.Term()
            }}";
                }
                else
                {
                    mainTemplate = mainTemplate.Replace("$errorCodec", "");
                    handleTemplate =
         @$"{namePascalCase}(messageValue)
            msg.Ack()";
                }
                mainTemplate = mainTemplate.Replace("$handle", handleTemplate);
            }
        }
        else
        {
            if (response != "null")
            {
                mainTemplate =
                @$"func ListenTo{namePascalCase}(conn *nats.Conn, {nameCamelCase} {namePascalCase}) {{
    {mainTemplate}
}}";
                mainTemplate = mainTemplate.Replace("$requestCodec", "");
                mainTemplate = mainTemplate.Replace("$responseCodec", responseCodecTemplate);
                if (error != null)
                {
                    mainTemplate = mainTemplate.Replace("$errorCodec", errorCodecTemplate);
                    handlerTemplate =
         @$"if response, responseError := {nameCamelCase}(); responseError == nil {{
            if responseEncoded, err := responseCodec.BinaryFromNative(nil, response); err == nil {{
                msg.Respond(responseEncoded)
            }} else {{
                msg.Term()
            }}
        }} else {{
            if errorEncoded, err := errorCodec.BinaryFromNative(nil, responseError); err == nil {{
                msg.Respond(errorEncoded)
            }} else {{
                msg.Term()
            }}
        }}";
                }
                else
                {
                    mainTemplate = mainTemplate.Replace("$errorCodec", "");
                    handlerTemplate =
         @$"response := {nameCamelCase}()
        if responseEncoded, err := responseCodec.BinaryFromNative(nil, response); err == nil {{
            msg.Respond(responseEncoded)
        }} else {{
            msg.Term()
        }}";
                }
                mainTemplate = mainTemplate.Replace("$handler", handlerTemplate);
            }
            else
            {
                mainTemplate =
@$"func ListenTo{namePascalCase}(conn *nats.Conn, {nameCamelCase} {namePascalCase}) {{
    {mainTemplate}
}}";
                mainTemplate = mainTemplate.Replace("$requestCodec", "");
                mainTemplate = mainTemplate.Replace("$responseCodec", "");
                if (error != null)
                {
                    mainTemplate = mainTemplate.Replace("$errorCodec", "");
                    handlerTemplate =
         @$"if error := {nameCamelCase}(); error == nil {{
            msg.Ack()
        }} else {{
            msg.Term()
        }}";
                }
                else
                {
                    mainTemplate = mainTemplate.Replace("$errorCodec", "");
                    handlerTemplate =
         @$"{nameCamelCase}()
            msg.Ack()";
                }
                mainTemplate = mainTemplate.Replace("$handler", handlerTemplate);
            }
        }
        return mainTemplate;
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