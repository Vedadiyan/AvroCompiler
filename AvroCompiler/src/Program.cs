using System.Diagnostics;
using AvroCompiler.Core;
using AvroCompiler.Core.AvroAPI;
using AvroCompiler.Core.Exceptions;
using AvroCompiler.Core.Parser;
using AvroCompiler.Go;

namespace AvroCompiler;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            AvroCompilerContext avroCompilerServerContext = new AvroCompilerContext("nounce.avdl", new GoLanguageFeaturesServer());
            await avroCompilerServerContext.Compile(@"C:\Users\Pouya\Desktop\Playground\Booqall.Protocols\go\", "nouncesrv.go");
            // AvroCompilerContext avroCompilerClientContext = new AvroCompilerContext("nounce.avdl", new GoLanguageFeaturesClient());
            // await avroCompilerClientContext.Compile(@"C:\Users\Pouya\Desktop\Playground\Booqall.Protocols\go\", "nounceclient.go");
        }
        catch (AvroCompilationException exception)
        {
            Console.WriteLine("{0}\r\n{1}", exception.Message, exception.Data["Error"]);
        }
    }
}