using System.Diagnostics;
using System.Reflection;
using System.Text;
using AvroCompiler.Core.Exceptions;
using AvroCompiler.Core.Storage;

using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.AvroAPI;

public class AvroToolsApi
{
    private static readonly string tempFileName;
    static AvroToolsApi()
    {
        tempFileName = TempFiles.Current.Value.GetTempFileName();
        using (FileStream fs = new FileStream(tempFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("avro-tools-1.9.1.jar");
            if (stream != null)
            {
                using (stream)
                {
                    stream.CopyTo(fs);
                }
            }
        }
    }
    public static async Task<Stream> Idl(string source)
    {
        StringBuilder output = new StringBuilder();
        string target = TempFiles.Current.Value.GetTempFileName();
        ProcessStartInfo processStartInfo = new ProcessStartInfo("java", $" -jar {tempFileName} idl {source} {target}");
        processStartInfo.RedirectStandardError = true;
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardOutput = true;
        Process process = ShouldOr(Process.Start(processStartInfo), new AvroCompilationException("Could not start initial process"));
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += (sender, x) =>
        {
            output.AppendLine(x.Data);
        };
        process.OutputDataReceived += (sender, x) =>
        {
            output.AppendLine(x.Data);
        };
        await process.WaitForExitAsync();
        FileInfo fileInfo = new FileInfo(target);
        if (fileInfo.Exists && fileInfo.Length > 0)
        {
            return new StreamReader(target).BaseStream;
        }
        else
        {
            throw new AvroCompilationException($"Compilation failed. Please refer to the official Avro Compiler error provided below.\r\n{output.ToString()}");
        }
    }
}