namespace AvroCompiler.Go;

public class NatsClientCreator
{
    private class PlaceHolders
    {
        public static readonly string FunctionNamePascalCase = "$function_name_pascal_case";
        public static readonly string FunctionNameCamelCase = "$function_name_camel_case";
        public static readonly string RequestNameCamelCase = "$request_name_camel_case";
        public static readonly string RequestNamePascalCase = "$request_name_pascal_case";
        public static readonly string ResponseNameCamelCase = "$response_name_camel_case";
        public static readonly string ResponseNamePascalCase = "$response_name_pascal_case";
        public static readonly string ErrorNameCamelCase = "$error_name_camel_case";
        public static readonly string ErrorNamePascalCase = "$error_name_pascal_case";
        public static readonly string RequestCodec = "$request_codec";
        public static readonly string ResponseCodec = "$response_codec";
        public static readonly string ErrorCodec = "$error_codec";
        public static readonly string Namespace = "$namespace";
        public static readonly string Handler = "$handler";
        public static readonly string Next = "$next";
        public static readonly string ErrorHandler = "$error_handler";
        public static readonly string Error = "$error";
    }
    private class Templates
    {
        public static readonly string Function =
@$"func {PlaceHolders.FunctionNamePascalCase}Client(conn *nats.Conn) {PlaceHolders.FunctionNamePascalCase} {{
{PlaceHolders.RequestCodec}
{PlaceHolders.ResponseCodec}
{PlaceHolders.ErrorCodec}
{PlaceHolders.Handler}
}}";
        public static readonly string RequestCodec =
@$"
requestCodec, requestCodecError := {PlaceHolders.RequestNamePascalCase}.Codec({PlaceHolders.RequestNamePascalCase}{{}})
if requestCodecError != nil {{
    panic(requestCodecError)
}}
";
        public static readonly string ResponseCodec =
@$"
responseCodec, responseCodecError := {PlaceHolders.ResponseNamePascalCase}.Codec({PlaceHolders.ResponseNamePascalCase}{{}})
if responseCodecError != nil {{
    panic(responseCodecError)
}}
";

        public static readonly string ErrorCodec =
@$"
errorCodec, errorCodecError := {PlaceHolders.ErrorNamePascalCase}.Codec({PlaceHolders.ErrorNamePascalCase}{{}})
if errorCodecError != nil {{
    panic(errorCodec)
}}
";
        public static string GetFunction(FunctionTypes functionType)
        {
            if ((functionType & FunctionTypes.ERROR) != FunctionTypes.ERROR)
            {
                if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
                {
                    string template =
@$"
return func({PlaceHolders.RequestNameCamelCase} {PlaceHolders.RequestNamePascalCase}) (*{PlaceHolders.ResponseNamePascalCase}, error) {{
    if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, {PlaceHolders.RequestNameCamelCase}.ToMap()); requestEncodedError == nil {{
        if response, error := conn.Request(""{PlaceHolders.Namespace}"", requestEncoded, time.Second * 10); error == nil {{
            if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {{
                if response, ok := responseDecoded.({PlaceHolders.ResponseNamePascalCase}); ok {{
                    return &response, nil
                }} else {{
                    return nil, errors.New(""Invalid message type"")
                }}
            }} else {{
                return nil, responseDecodedError
            }}
        }} else {{
            return nil, error
        }}
    }} else {{
        return nil, requestEncodedError
    }}
}}
";
                    return template;
                }
                else if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
                {
                    string template =
@$"
return func({PlaceHolders.RequestNameCamelCase} {PlaceHolders.RequestNamePascalCase}) error {{
    if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, {PlaceHolders.RequestNameCamelCase}.ToMap()); requestEncodedError == nil {{
        return conn.Publish(""{PlaceHolders.Namespace}"", requestEncoded)
    }} else {{
        return requestEncodedError
    }}
}}
";
                    return template;
                }
                else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
                {
                    return
@$"
return func({PlaceHolders.RequestNameCamelCase} {PlaceHolders.RequestNamePascalCase}) (*{PlaceHolders.ResponseNamePascalCase}, error) {{
    if response, error := conn.Request(""{PlaceHolders.Namespace}"", []byte {{}}, time.Second * 10); error == nil {{
        if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {{
            if response, ok := responseDecoded.(map[string]any); ok {{
                _res := new{PlaceHolders.ResponseNamePascalCase}(response)
                return &_res, nil
            }} else {{
                return nil, errors.New(""Invalid message type"")
            }}
        }} else {{
            return nil, responseDecodedError
        }}
    }} else {{
        return nil, error
    }}
}}
";
                }
                else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
                {
                    return
@$"
return func() error {{
    return conn.Publish(""{PlaceHolders.Namespace}"", []byte {{}})
}}
";
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
                {
                    string template =
@$"
return func({PlaceHolders.RequestNameCamelCase} {PlaceHolders.RequestNamePascalCase}) (*{PlaceHolders.ResponseNamePascalCase}, *{PlaceHolders.ErrorNamePascalCase}, error) {{
    if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, {PlaceHolders.RequestNameCamelCase}.ToMap()); requestEncodedError == nil {{
        if response, error := conn.Request(""{PlaceHolders.Namespace}"", requestEncoded, time.Second * 10); error == nil {{
            if response.Header.Get(""Status"") == ""200"" {{
                if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {{
                    if response, ok := responseDecoded.(map[string]any); ok {{
                        _res := new{PlaceHolders.ResponseNamePascalCase}(response)
                        return &_res, nil, nil
                    }} else {{
                        return nil, nil, errors.New(""Invalid message type"")
                    }}
                }} else {{
                    return nil, nil, responseDecodedError
                }}
            }} else {{
                if errorDecoded, _, errorDecodedError := errorCodec.NativeFromBinary(response.Data); errorDecodedError == nil {{
                    if error, ok := errorDecoded.(map[string]any); ok {{
                        _res := new{PlaceHolders.ErrorNamePascalCase}(error)
                        return nil, &_res, nil
                    }} else {{
                        return nil, nil, errors.New(""Invalid error type"")
                    }}
                }} else {{
                    return nil, nil, errorDecodedError
                }}
            }}

        }} else {{
            return nil, nil, error
        }}
    }} else {{
        return nil, nil, requestEncodedError
    }}
}}
";
                    return template;
                }
                else if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
                {
                    return GetFunction(functionType ^ FunctionTypes.ERROR);
                }
                else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
                {
                    return
@$"
return func() (*{PlaceHolders.ResponseNamePascalCase}, *{PlaceHolders.ErrorNamePascalCase}, error) {{
   if response, error := conn.Request(""{PlaceHolders.Namespace}"", []byte {{}}, time.Second * 10); error == nil {{
        if response.Header.Get(""Status"") == ""200"" {{
            if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {{
                if response, ok := responseDecoded.(map[string]any); ok {{
                    _res := new{PlaceHolders.ResponseNamePascalCase}(response)
                    return &_res, nil, nil
                }} else {{
                    return nil, nil, errors.New(""Invalid message type"")
                }}
            }} else {{
                return nil, nil, responseDecodedError
            }}
        }} else {{
            if errorDecoded, _, errorDecodedError := errorCodec.NativeFromBinary(response.Data); errorDecodedError == nil {{
                if error, ok := errorDecoded.(map[string]any); ok {{
                    _res := new{PlaceHolders.ErrorNamePascalCase}(error)
                    return nil, &_res, nil
                }} else {{
                    return nil, nil, errors.New(""Invalid error type"")
                }}
            }} else {{
                return nil, nil, errorDecodedError
            }}
        }}

    }} else {{
        return nil, nil, error
    }}
}}
";
                }
                else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
                {
                    return GetFunction(functionType ^ FunctionTypes.ERROR);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }
    }
    private readonly string functionName;
    private readonly string @namespace;
    private readonly string? request;
    private readonly string? response;
    private readonly string? error;
    private readonly FunctionTypes functionType;
    public NatsClientCreator(string functionName, string @namespace, string? request, string? response, string? error)
    {
        this.functionName = functionName;
        this.@namespace = @namespace;
        if (request != null)
        {
            this.request = request;
            functionType |= FunctionTypes.REQUEST;
        }
        if (response?.Equals("null") == false)
        {
            this.response = response;
            functionType |= FunctionTypes.RESPONSE;
        }
        if (error != null)
        {
            this.error = error;
            functionType |= FunctionTypes.ERROR;
        }
    }
    public string GetFunction()
    {
        string template = Templates.Function.Replace(PlaceHolders.Handler, Templates.GetFunction(functionType));
        if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST)
        {
            template = template.Replace(PlaceHolders.RequestCodec, Templates.RequestCodec);
            template = template.Replace(PlaceHolders.RequestNamePascalCase, request!.ToPascalCase());
            template = template.Replace(PlaceHolders.RequestNameCamelCase, request!.ToCamelCase());
        }
        if ((functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
        {
            template = template.Replace(PlaceHolders.ResponseCodec, Templates.ResponseCodec);
            template = template.Replace(PlaceHolders.ResponseNamePascalCase, response!.ToPascalCase());
        }
        if ((functionType & FunctionTypes.ERROR) == FunctionTypes.ERROR)
        {
            template = template.Replace(PlaceHolders.ErrorCodec, Templates.ErrorCodec);
            template = template.Replace(PlaceHolders.ErrorNamePascalCase, error!.ToPascalCase());
        }
        template = template.Replace(PlaceHolders.RequestCodec, "");
        template = template.Replace(PlaceHolders.ResponseCodec, "");
        template = template.Replace(PlaceHolders.ErrorCodec, "");
        template = template.Replace(PlaceHolders.FunctionNameCamelCase, functionName.ToCamelCase());
        template = template.Replace(PlaceHolders.FunctionNamePascalCase, functionName.ToPascalCase());
        template = template.Replace(PlaceHolders.Namespace, @namespace);
        return template;
    }
    public string GetFunctionType()
    {
        if ((functionType & FunctionTypes.ERROR) != FunctionTypes.ERROR)
        {
            if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func({request!.ToCamelCase()} {request!.ToPascalCase()}) (*{response!.ToPascalCase()}, error)";
            }
            else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func() (*{response!.ToPascalCase()}, error)";
            }
            else if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func({request!.ToCamelCase()} {request!.ToPascalCase()}) error";
            }
            else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func() error";
            }
            else
            {
                throw new ArgumentException();
            }
        }
        else
        {
            if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func({request!.ToCamelCase()} {request!.ToPascalCase()}) (*{response!.ToPascalCase()}, *{error!.ToPascalCase()}, error)";
            }
            else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func() (*{response!.ToPascalCase()}, *{error!.ToPascalCase()}, error)";
            }
            else if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func({request!.ToCamelCase()} {request!.ToPascalCase()}) error";
            }
            else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func() error";
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}