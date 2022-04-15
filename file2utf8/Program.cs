using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UtfUnknown;

namespace File2Utf8;

[Command(Name = "file2utf8", Description = "Convert a non-utf8 file to UTF-8 (default is without BOM)")]
[HelpOption]
public class Program
{
    [Required(ErrorMessage = "You must specify the path to a directory or file to convert")]
    [Argument(0, Name = "path", Description = "Path to the file or directory to convert")]
    [FileOrDirectoryExists]
    public string? Path { get; }

    [Argument(1, Name = "searchPattern", Description = "If Path is directory, the search string to match against the names of files in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.")]
    [DefaultValue("*.cs")]
    public string SearchPattern { get; } = "*.cs";

    [Option("-b|--bom", Description = "UTF8 with BOM.")]
    public bool WithBom { get; }

    [Option("-c|--confidence", Description = "The confidence of the found encoding. Between 0 and 1.")]
    [Range(0.0, 1.0)]
    [DefaultValue("0.9")]
    public float Confidence { get; } = 0.9f;

    [Option("-d|--debug", Description = "Show unmatched files, does not convert.")]
    public bool Debug { get; }

    [Option("-i|--ignore-utf8-files", Description = "Ignore utf-8 files, even not match bom.")]
    public bool IgnoreUtf8Files { get; }

    public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

    public int OnExecute(CommandLineApplication app, IConsole console)
    {
        if (Path == null)
        {
            app.ShowHelp();

            return 1;
        }

        var utf8 = WithBom ? Encoding.UTF8 : new UTF8Encoding(false);

        foreach (var file in Directory.Exists(Path)
            ? Directory.GetFiles(Path, SearchPattern, SearchOption.AllDirectories)
            : new[] { Path })
        {
            if (CharsetDetector.DetectFromFile(file) is not { Detected.Encoding: { } encoding } cd ||
                (WithBom ? GetEncodingName(encoding) != "utf-8-bom" : GetEncodingName(encoding) is "utf-8" or "us-ascii") ||
                IgnoreUtf8Files && encoding.WebName == "utf-8") continue;

            if (cd.Detected.Confidence < Confidence)
                console.WriteLine($"Have no enough confidence: {GetEncodingName(encoding)} {cd.Detected.Confidence * 100:0.##}% {file}");
            else
            {
                if (!Debug) File.WriteAllText(file, File.ReadAllText(file, encoding), utf8);

                console.WriteLine($"{GetEncodingName(encoding)} {cd.Detected.Confidence * 100:0.##}% {file}");
            }
        }

        return 0;
    }

    private static string GetEncodingName(Encoding encoding) =>
        encoding.WebName != "utf-8" ? encoding.WebName : encoding.Preamble.IsEmpty ? "utf-8" : "utf-8-bom";
}
