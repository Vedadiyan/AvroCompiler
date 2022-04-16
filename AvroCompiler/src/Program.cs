using System.Diagnostics;
using AvroCompiler.Core;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.AvroAPI;
using AvroCompiler.Core.Exceptions;
using AvroCompiler.Core.Parser;
using AvroCompiler.Go;
using CommandLine;

namespace AvroCompiler;

public class Program
{
    public static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed(x =>
        {
            switch (x.GenerationType!.ToLower())
            {
                case "server":
                    {
                        ILanguageFeature languageFeature;
                        switch (x.Language!.ToLower())
                        {
                            case "go":
                                {
                                    languageFeature = new GoLanguageFeaturesServer();
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unsupported language ({0})", x.Language!);
                                    return;
                                }
                        }
                        try
                        {
                            AvroCompilerContext avroCompilerServerContext = new AvroCompilerContext(x.TargetAvdlFile!, languageFeature);
                            avroCompilerServerContext.Compile(x.DestinationPath!, x.OutputFileName!).Wait();
                        }
                        catch (AvroCompilationException exception)
                        {
                            Console.WriteLine("{0}\r\n{1}", exception.Message, exception.Data["Error"]);
                        }
                        break;
                    }
                case "client":
                    {
                        ILanguageFeature languageFeature;
                        switch (x.Language!.ToLower())
                        {
                            case "go":
                                {
                                    languageFeature = new GoLanguageFeaturesClient();
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unsupported language ({0})", x.Language!);
                                    return;
                                }
                        }
                        try
                        {
                            AvroCompilerContext avroCompilerClientContext = new AvroCompilerContext(x.TargetAvdlFile!, languageFeature);
                            avroCompilerClientContext.Compile(x.DestinationPath!, x.OutputFileName!).Wait();
                        }
                        catch (AvroCompilationException exception)
                        {
                            Console.WriteLine("{0}\r\n{1}", exception.Message, exception.Data["Error"]);
                        }
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unsupported generation type ({0})", x.GenerationType!);
                        return;
                    }
            }
        });

    }
}