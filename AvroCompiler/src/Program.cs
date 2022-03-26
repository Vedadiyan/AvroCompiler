using System.Diagnostics;
using AvroCompiler.Core.AvroAPI;
using AvroCompiler.Core.Parser;

namespace AvroCompiler;

public class Program
{
    public static void Main(string[] args)
    {
        var result = AvroToolsApi.Idl("test.avdl").Result;
        System.IO.File.WriteAllText("Test.go", new AvroSchemaParser(result, new GoLanguageFeatures()).Parse());
    }
}