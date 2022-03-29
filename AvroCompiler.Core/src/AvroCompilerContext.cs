using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.AvroAPI;
using AvroCompiler.Core.Parser;
using AvroCompiler.Core.Storage;

namespace AvroCompiler.Core;

public class AvroCompilerContext
{
    private readonly string avroIdlStr;
    private readonly ILanguageFeature languageFeature;
    public AvroCompilerContext(string path, ILanguageFeature languageFeature)
    {
        avroIdlStr = File.ReadAllText(path);
        this.languageFeature = languageFeature;
    }
    public async Task Compile(string outputPath)
    {
        try
        {
            string preProcessedFile = await PreProcessors.UrlImportAsync(avroIdlStr);
            string tempFileName = TempFiles.Current.Value.GetTempFileName();
            File.WriteAllText(tempFileName, preProcessedFile);
            Stream stream = await AvroToolsApi.Idl(tempFileName);
            AvroSchemaParser avroSchemaParser = new AvroSchemaParser(stream, languageFeature);
            string parsedSchema = await avroSchemaParser.Parse();
            File.WriteAllText(outputPath, parsedSchema);
        }
        finally
        {
            TempFiles.Current.Value.DeleteAll();
        }
    }
}