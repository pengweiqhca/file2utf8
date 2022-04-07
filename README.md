Convert a non-utf8 file to UTF-8

## Installation

The latest release of config2json requires the [6.0.100](https://dotnet.microsoft.com/download/dotnet/6.0) .NET Core SDK or newer.
Once installed, run this command:

```
dotnet tool install -g file2utf8
```

## Usage

```
Usage: file2utf8 [options] <path> <searchPattern>

Arguments:
  path              Path to the file or directory to convert
  searchPattern     If Path is directory, the search string to match against the names of files in path. This parameter can contain a
                    combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.
                    Default value is: *.cs.

Options:
  -?|-h|--help            Show help information.
  -b|--bom                UTF8 or UTF8 with BOM.
  -c|--confidence         The confidence of the found encoding. Between 0 and 1.
                          Default value is: 0.9.
  -d|--debug              Show unmatched files, does not convert.
  -i|--ignore-utf8-files  Ignore utf-8 files, even not match bom.
```
