using System.Diagnostics;
using AvroCompiler.Core.AvroAPI;
using AvroCompiler.Core.Parser;
using AvroCompiler.Go;

namespace AvroCompiler;

public class Program
{
    public static async Task Main(string[] args)
    {
        var result = AvroToolsApi.Idl("test.avdl").Result;
        System.IO.File.WriteAllText("Test.go", await new AvroSchemaParser(result, new GoLanguageFeatures()).Parse());
    }
}