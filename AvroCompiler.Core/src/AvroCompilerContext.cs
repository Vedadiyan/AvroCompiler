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
            StringBuilder finalCode = new StringBuilder();
            finalCode.AppendLine(languageFeature.GetComments());
            finalCode.AppendLine(languageFeature.GetNamespace());
            finalCode.AppendLine(languageFeature.GetImports());
            foreach(var avroElement in avroElements) {
                finalCode.AppendLine(avroElement.Template());
            }
            finalCode.AppendLine(languageFeature.GetCodec());
            string code = await languageFeature.Format(finalCode.ToString());
            File.WriteAllText(outputPath, code);
        }
        finally
        {
            TempFiles.Current.Value.DeleteAll();
        }
    }
}