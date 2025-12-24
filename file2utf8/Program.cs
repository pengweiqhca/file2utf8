using System.CommandLine;
using System.Text;
using UtfUnknown;

namespace File2Utf8;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var pathArgument = new Argument<string>(
            name: "path",
            description: "Path to the file or directory to convert");

        var searchPatternArgument = new Argument<string>(
            name: "searchPattern",
            getDefaultValue: () => "*.cs",
            description: "If Path is directory, the search string to match against the names of files in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.");

        var bomOption = new Option<bool>(
            aliases: new[] { "-b", "--bom" },
            description: "UTF8 with BOM.");

        var confidenceOption = new Option<float>(
            aliases: new[] { "-c", "--confidence" },
            getDefaultValue: () => 0.9f,
            description: "The confidence of the found encoding. Between 0 and 1.");

        var debugOption = new Option<bool>(
            aliases: new[] { "-d", "--debug" },
            description: "Show unmatched files, does not convert.");

        var ignoreUtf8FilesOption = new Option<bool>(
            aliases: new[] { "-i", "--ignore-utf8-files" },
            description: "Ignore utf-8 files, even not match bom.");

        var rootCommand = new RootCommand("Convert a non-utf8 file to UTF-8 (default is without BOM)")
        {
            pathArgument,
            searchPatternArgument,
            bomOption,
            confidenceOption,
            debugOption,
            ignoreUtf8FilesOption
        };

        rootCommand.SetHandler(Execute, pathArgument, searchPatternArgument, bomOption, confidenceOption, debugOption, ignoreUtf8FilesOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static void Execute(string path, string searchPattern, bool withBom, float confidence, bool debug, bool ignoreUtf8Files)
    {
        if (string.IsNullOrEmpty(path))
        {
            Console.WriteLine("You must specify the path to a directory or file to convert");
            Environment.Exit(1);
        }

        if (!File.Exists(path) && !Directory.Exists(path))
        {
            Console.WriteLine($"Path '{path}' does not exist");
            Environment.Exit(1);
        }

        if (confidence < 0.0f || confidence > 1.0f)
        {
            Console.WriteLine("Confidence must be between 0 and 1");
            Environment.Exit(1);
        }

        var utf8 = withBom ? Encoding.UTF8 : new UTF8Encoding(false);

        foreach (var file in Directory.Exists(path)
            ? Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)
            : new[] { path })
        {
            if (CharsetDetector.DetectFromFile(file) is not { Detected.Encoding: { } encoding } cd ||
                (withBom ? GetEncodingName(encoding) != "utf-8-bom" : GetEncodingName(encoding) is "utf-8" or "us-ascii") ||
                ignoreUtf8Files && encoding.WebName == "utf-8") continue;

            if (cd.Detected.Confidence < confidence)
                Console.WriteLine($"Have no enough confidence: {GetEncodingName(encoding)} {cd.Detected.Confidence * 100:0.##}% {file}");
            else
            {
                if (!debug) File.WriteAllText(file, File.ReadAllText(file, encoding), utf8);

                Console.WriteLine($"{GetEncodingName(encoding)} {cd.Detected.Confidence * 100:0.##}% {file}");
            }
        }
    }

    private static string GetEncodingName(Encoding encoding) =>
        encoding.WebName != "utf-8" ? encoding.WebName : encoding.Preamble.IsEmpty ? "utf-8" : "utf-8-bom";
}
