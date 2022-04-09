namespace AvroCompiler.Go;

[Flags]
public enum FunctionTypes
{
    REQUEST = 1,
    RESPONSE = 2,
    ERROR = 4
}
public class NatsListenerCreator
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
@$"func {PlaceHolders.FunctionNamePascalCase}Listener(conn *nats.Conn) func({PlaceHolders.FunctionNameCamelCase} {PlaceHolders.FunctionNamePascalCase}) (*nats.Subscription, error) {{
{PlaceHolders.RequestCodec}
{PlaceHolders.ResponseCodec}
{PlaceHolders.ErrorCodec}
    return func({PlaceHolders.FunctionNameCamelCase} {PlaceHolders.FunctionNamePascalCase}) (*nats.Subscription, error) {{
        return conn.Subscribe(""{PlaceHolders.Namespace}"", func(msg *nats.Msg) {{
{PlaceHolders.Handler}
        }})
    }}
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
    panic(errorCodecError)
}}
";
        public static readonly string RequestHandler =
@$"
if requestDecoded, _, requestDecodingError := requestCodec.NativeFromBinary(msg.Data); requestDecodingError == nil {{
    if request, ok := requestDecoded.({PlaceHolders.RequestNamePascalCase}); ok {{
{PlaceHolders.Next}
    }} else {{
        {PlaceHolders.Error}
    }}
}} else {{
    {PlaceHolders.Error}
}}";
        public static readonly string ResponseHandler =
@$"
if responseEncoded, responseEncodingError := responseCodec.BinaryFromNative(nil, *response); responseEncodingError == nil {{
    msg.Respond(responseEncoded)
}} else {{
    {PlaceHolders.Error}
}}
";
        public static readonly string ErrorHandler =
@$"
if errorEncoded, errorEncodingError := errorCodec.BinaryFromNative(nil, *err); errorEncodingError == nil {{
    msg.Respond(errorEncoded)
}} else {{
    {PlaceHolders.Error}
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
response := {PlaceHolders.FunctionNameCamelCase}(request)
{Templates.ResponseHandler}
";
                    return Templates.RequestHandler.Replace(PlaceHolders.Next, template);
                }
                else if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
                {
                    string template =
@$"
{PlaceHolders.FunctionNameCamelCase}(request)
";
                    return Templates.RequestHandler.Replace(PlaceHolders.Next, template);
                }
                else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
                {
                    return
@$"
response := {PlaceHolders.FunctionNameCamelCase}()
{Templates.ResponseHandler}
";
                }
                else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
                {
                    return
@$"
{PlaceHolders.FunctionNameCamelCase}()
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
if response, err := {PlaceHolders.FunctionNameCamelCase}(request); err == nil {{
    {Templates.ResponseHandler}
}} else {{
    {Templates.ErrorHandler}
}}
";
                    return Templates.RequestHandler.Replace(PlaceHolders.Next, template);
                }
                else if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
                {
                    string template =
@$"
if err := {PlaceHolders.FunctionNameCamelCase}(request); err != nil {{
    {Templates.ErrorHandler}
}}
";
                    return Templates.RequestHandler.Replace(PlaceHolders.Next, template);
                }
                else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
                {
                    return
@$"
if response, err := {PlaceHolders.FunctionNameCamelCase}(); err == nil {{
    {Templates.ResponseHandler}
}} else {{
    {Templates.ErrorHandler}
}}
";
                }
                else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
                {
                    return
@$"
if err := {PlaceHolders.FunctionNameCamelCase}(); err != nil {{
    {Templates.ErrorHandler}
}}
";
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
    public NatsListenerCreator(string functionName, string @namespace, string? request, string? response, string? error)
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
        template = template.Replace(PlaceHolders.Error, "msg.Term()");
        template = template.Replace(PlaceHolders.Namespace, @namespace);
        return template;
    }
    public string GetFunctionType()
    {
        if ((functionType & FunctionTypes.ERROR) != FunctionTypes.ERROR)
        {
            if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func({request!.ToCamelCase()} {request!.ToPascalCase()}) *{response!.ToPascalCase()}";
            }
            else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func() *{response!.ToPascalCase()}";
            }
            else if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func({request!.ToCamelCase()} *{request!.ToPascalCase()})";
            }
            else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func()";
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
                return @$"type {functionName.ToPascalCase()} func({request!.ToCamelCase()} {request!.ToPascalCase()}) (*{response!.ToPascalCase()}, *{error!.ToPascalCase()})";
            }
            else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) == FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func() ({response!.ToPascalCase()}, *{error!.ToPascalCase()})";
            }
            else if ((functionType & FunctionTypes.REQUEST) == FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func({request!.ToCamelCase()} {request!.ToPascalCase()}) *({error!.ToPascalCase()})";
            }
            else if ((functionType & FunctionTypes.REQUEST) != FunctionTypes.REQUEST && (functionType & FunctionTypes.RESPONSE) != FunctionTypes.RESPONSE)
            {
                return @$"type {functionName.ToPascalCase()} func() (*{error!.ToPascalCase()}";
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}