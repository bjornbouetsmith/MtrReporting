using CommandLine;

namespace Converter
{
    public class Options
    {
        [Option('s', "source", Required = true, HelpText = "Path where files that should be converted are located")]
        public required string SourcePath { get; set; }

        [Option('d', "dest", Required = true, HelpText = "Path where files should be written when converted")]
        public required string DestinationPath { get; set; }

        [Option('c', "create", Required = false, HelpText = "Whether or not to create destination path if it does not exist")]
        public bool CreateDestinationPath { get; set; }

        [Option('S', "single", Required = false, HelpText = "Whether or not to convert source files into a single destination file at destination directory")]
        public bool SingleDestinationFile { get; set; }

        [Option('p', "pattern", Default = "*.csv", HelpText = "The file pattern to use when looking for files to convert")]
        public required string FilePattern { get; set; }

        [Option('D', "file", Default = "converted.csv", HelpText = "The filename for the destination file when using a single output file")]
        public required string DestinationFile { get; set; }

        [Option('o', "overwrite", Default = true, HelpText = "Whether or not to overwrite destination files - or append")]
        public bool Overwrite { get; set; }

        [Option('C', "convert", Default = true, HelpText = "Whether or not to convert numbers from the invariant format to current culture")]
        public bool ConvertNumbers { get; set; }

        [Option('f', "separator", Default = ',', HelpText = "Separator char to use when writing values to the file")]
        public char FieldSeparator { get; set; }

        [Option('t', "trim", HelpText = "Whether or not to trim outfiles so they only contain lines where loss is not 0")]
        public bool TrimZeroLossLines { get; set; }

        public bool HasWrittenHeader { get; set; }
    }
}