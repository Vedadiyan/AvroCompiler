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
        // AvroCompilerContext avroCompilerContext = new AvroCompilerContext("test.avdl", new GoLanguageFeatures());
        // await avroCompilerContext.Compile("Test.go");

        AutoMappers autoMappers = new AutoMappers(Core.Contants.AvroTypes.ARRAY, "Test", "Field", new GoLanguageFeatures().GetType);
        var test = autoMappers.getBackwardedPimitiveArrayType(3, Core.Contants.AvroTypes.INT);
        System.IO.File.WriteAllText("test.txt", test);
    }
}