using System.Reflection;
using System.Text;
using AvroCompiler.Core.Contants;

namespace AvroCompiler.Go;

public class GoLanguageFeaturesClient : GoLanguageFeaturesBase
{
    public override string GetMessage(string name, IReadOnlyDictionary<string, string> request, string response, object? options)
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
                object @namespace = _options?.FirstOrDefault(x => x.Name == "Namespace")?.GetValue(options) ?? throw new ArgumentNullException(); ;
                NatsClientCreator natsClientCreator = new NatsClientCreator(name, (string)@namespace, type, response, error != null ? (string)error : null);
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
            object @namespace = _options?.FirstOrDefault(x => x.Name == "Namespace")?.GetValue(options) ?? throw new ArgumentNullException();
            NatsClientCreator natsClientCreator = new NatsClientCreator(name, (string)@namespace, null, response, error != null ? (string)error : null);
            output.AppendLine();
            output.AppendLine(natsClientCreator.GetFunctionType());
            output.AppendLine(natsClientCreator.GetFunction());
            return output.ToString();
        }
        return "";
    }
}