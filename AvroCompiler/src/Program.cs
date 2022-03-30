using System.Diagnostics;
using AvroCompiler.Core;
using AvroCompiler.Core.AvroAPI;
using AvroCompiler.Core.Parser;
using AvroCompiler.Go;

namespace AvroCompiler;

public class Program
{
    public static async Task Main(string[] args)
    {
        AvroCompilerContext avroCompilerContext = new AvroCompilerContext("test.avdl", new GoLanguageFeatures());
        await avroCompilerContext.Compile("Test.go");
    }
}