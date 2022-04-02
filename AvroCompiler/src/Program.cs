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
            AvroCompilerContext avroCompilerContext = new AvroCompilerContext("test.avdl", new GoLanguageFeatures());
            await avroCompilerContext.Compile("Test.go");
        }
        catch(AvroCompilationException exception) {
            Console.WriteLine("{0}\r\n{1}", exception.Message, exception.Data["Error"]);
        }
    }
}