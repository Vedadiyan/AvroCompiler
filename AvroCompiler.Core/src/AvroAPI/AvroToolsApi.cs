using System.Diagnostics;
using System.Reflection;

namespace AvroCompiler.Core.AvroAPI;

public class AvroToolsApi
{
    private static readonly string tempFileName;
    static AvroToolsApi()
    {
        tempFileName = Path.GetTempFileName();
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
        string target = Path.GetTempFileName();
        ProcessStartInfo processStartInfo = new ProcessStartInfo("java", $" -jar {tempFileName} idl {source} {target}");
        processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
        Process? process = Process.Start(processStartInfo);
        if (process != null)
        {
            await process.WaitForExitAsync();
            if(File.Exists(target)) {
                return new StreamReader(target).BaseStream;
            }
            else {
                throw new Exception();
            }
        }
        else
        {
            throw new Exception();
        }
    }
    public static void CleanWorkSpace() {
        File.Delete(tempFileName);
    }
}