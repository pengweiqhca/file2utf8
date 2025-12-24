Convert a non-utf8 file to UTF-8

## Installation

The latest release of file2utf8 requires the [8.0.100](https://dotnet.microsoft.com/download/dotnet/8.0) .NET Core SDK or newer.
Once installed, run this command:

```
dotnet tool install -g file2utf8
```

## Usage

```
Usage: file2utf8 <path> [<searchPattern>] [options]

Arguments:
  <path>           Path to the file or directory to convert
  <searchPattern>  If path is a directory, the search string to match against the names of files in path.
                   This parameter can contain a combination of valid literal path and wildcard (* and ?)
                   characters, but it doesn't support regular expressions. [default: *.cs]

Options:
  -b, --bom                      Add UTF-8 BOM (Byte Order Mark).
  -c, --confidence <confidence>  The confidence of the found encoding. Between 0 and 1. [default: 0.9]
  -d, --debug                    Show unmatched files without converting them.
  -i, --ignore-utf8-files        Ignore UTF-8 files, even if BOM doesn't match.
  -?, -h, --help                 Show help and usage information
  --version                      Show version information
```
