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
    public async Task Compile(string outputPath, string fileName)
    {
        try
        {
            string preProcessedFile = await PreProcessors.UrlImportAsync(avroIdlStr);
            string tempFileName = TempFiles.Current.Value.GetTempFileName();
            File.WriteAllText(tempFileName, preProcessedFile);
            Stream stream = await AvroToolsApi.Idl(tempFileName);
            AvroAvprParser avroAvprParser = new AvroAvprParser(stream, languageFeature);
            IReadOnlyList<AvroElement> avroElements = avroAvprParser.Parse().ToList();
            StringBuilder preview = new StringBuilder();
            preview.AppendLine(languageFeature.GetComments());
            preview.AppendLine(languageFeature.GetNamespace());
            preview.AppendLine(languageFeature.GetImports());
            foreach(var avroElement in avroElements) {
                preview.AppendLine(avroElement.Template());
            }
            preview.AppendLine(languageFeature.GetCodec());
            StringBuilder finalCode = new StringBuilder();
            foreach(var line in preview.ToString().Split("\r\n")) {
                if(!string.IsNullOrWhiteSpace(line)) {
                    finalCode.AppendLine(line);
                }
            }
            string code = await languageFeature.Format(finalCode.ToString());
            if(!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }
            string finalPath = Path.Combine(outputPath, fileName);
            File.WriteAllText(finalPath, code);
        }
        finally
        {
            TempFiles.Current.Value.DeleteAll();
        }
    }
}