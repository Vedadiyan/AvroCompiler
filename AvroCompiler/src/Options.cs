using CommandLine;

namespace AvroCompiler;

public class Options
{
    [Option('T', "target", Required = true, HelpText = "The path to target .avdl file")]
    public string? TargetAvdlFile { get; set; }
    [Option('D', "destination", Required = true, HelpText = "Destination path")]
    public string? DestinationPath { get; set; }
    [Option('O', "output", Required = true, HelpText = "Output file name")]
    public string? OutputFileName { get; set; }
    [Option('G', "generate", Required = true, HelpText = "Generation Type. Accepted values: server | client")]
    public string? GenerationType { get; set; }
    [Option('L', "language", Required = true, HelpText = "Target language. Accepted value: go")]
    public string? Language { get; set; }
}