using System.CommandLine;
using System.Text;
using UtfUnknown;

var pathArgument = new Argument<string>("path")
{
    Description = "Path to the file or directory to convert"
};

var searchPatternArgument = new Argument<string>("searchPattern")
{
    DefaultValueFactory = _ => "*.cs",
    Description = "If path is a directory, the search string to match against the names of files in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions."
};

var bomOption = new Option<bool>("--bom", "-b")
{
    Description = "Add UTF-8 BOM (Byte Order Mark)."
};

var confidenceOption = new Option<float>("--confidence", "-c")
{
    DefaultValueFactory = _ => 0.9f,
    Description = "The confidence of the found encoding. Between 0 and 1."
};

var debugOption = new Option<bool>("--debug", "-d")
{
    Description = "Show unmatched files without converting them."
};

var ignoreUtf8FilesOption = new Option<bool>("--ignore-utf8-files", "-i")
{
    Description = "Ignore UTF-8 files, even if BOM doesn't match."
};

var rootCommand = new RootCommand("Convert a non-utf8 file to UTF-8 (default is without BOM)")
{
    pathArgument,
    searchPatternArgument,
    bomOption,
    confidenceOption,
    debugOption,
    ignoreUtf8FilesOption
};

rootCommand.SetAction(result =>
{
    var path = result.GetRequiredValue(pathArgument);
    var searchPattern = result.GetRequiredValue(searchPatternArgument);
    var withBom = result.GetValue(bomOption);
    var confidence = result.GetValue(confidenceOption);
    var debug = result.GetValue(debugOption);
    var ignoreUtf8Files = result.GetValue(ignoreUtf8FilesOption);

    if (string.IsNullOrEmpty(path))
    {
        Console.WriteLine("You must specify the path to a directory or file to convert");
        return 1;
    }

    if (!File.Exists(path) && !Directory.Exists(path))
    {
        Console.WriteLine($"Path '{path}' does not exist");
        return 1;
    }

    if (confidence is < 0.0f or > 1.0f)
    {
        Console.WriteLine("Confidence must be between 0 and 1");
        return 1;
    }

    var utf8 = withBom ? Encoding.UTF8 : new UTF8Encoding(false);

    foreach (var file in Directory.Exists(path)
                 ? Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)
                 : new[] { path })
    {
        if (CharsetDetector.DetectFromFile(file) is not { Detected.Encoding: { } encoding } cd ||
            (withBom
                ? GetEncodingName(encoding) != "utf-8-bom"
                : GetEncodingName(encoding) is "utf-8" or "us-ascii") ||
            ignoreUtf8Files && encoding.WebName == "utf-8") continue;

        if (cd.Detected.Confidence < confidence)
            Console.WriteLine(
                $"Have no enough confidence: {GetEncodingName(encoding)} {cd.Detected.Confidence * 100:0.##}% {file}");
        else
        {
            if (!debug) File.WriteAllText(file, File.ReadAllText(file, encoding), utf8);

            Console.WriteLine($"{GetEncodingName(encoding)} {cd.Detected.Confidence * 100:0.##}% {file}");
        }
    }

    return 0;
});

return await rootCommand.Parse(args).InvokeAsync();

static string GetEncodingName(Encoding encoding) =>
    encoding.WebName != "utf-8" ? encoding.WebName : encoding.Preamble.IsEmpty ? "utf-8" : "utf-8-bom";
