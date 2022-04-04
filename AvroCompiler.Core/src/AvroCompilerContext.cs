using System.Text;
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
            AvroAvprParser avroAvprParser = new AvroAvprParser(stream, languageFeature);
            IReadOnlyList<AvroElement> avroElements = avroAvprParser.Parse().ToList();
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var avroElement in avroElements) {
                stringBuilder.AppendLine(avroElement.Template());
            }
            stringBuilder.AppendLine(languageFeature.GetCodec());
            File.WriteAllText(outputPath, stringBuilder.ToString());
        }
        finally
        {
            TempFiles.Current.Value.DeleteAll();
        }
    }
}