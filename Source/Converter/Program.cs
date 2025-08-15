using CommandLine;
using System.Globalization;

namespace Converter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var options = Parser.Default.ParseArguments<Options>(args)
                .Value;

            if (options == null)
            {
                return;
            }

            var converter = new Converter(options);

            converter.Run();
        }
    }

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

        public bool HasWrittenHeader { get; set; }
    }


    public class Converter
    {
        private readonly Options _options;
        private bool _alreadyOverwritten;

        public Converter(Options options)
        {
            _options = options;
        }

        public void Run()
        {
            Console.WriteLine("Source path:\t\t{0}", _options.SourcePath);
            Console.WriteLine("Destination path:\t{0}", _options.DestinationPath);
            Console.WriteLine("Using single destination file: {0}", _options.SingleDestinationFile);
            if (!Directory.Exists(_options.SourcePath))
            {
                Console.WriteLine("Source path:{0} does not exist", _options.SourcePath);
                return;
            }

            if (!Directory.Exists(_options.DestinationPath))
            {
                if (!_options.CreateDestinationPath)
                {
                    Console.WriteLine("Destination path: '{0}' does not exist and --createdest is not specified", _options.DestinationPath);
                    return;
                }
                Directory.CreateDirectory(_options.DestinationPath);
            }

            foreach (var file in Directory.GetFiles(_options.SourcePath, _options.FilePattern, SearchOption.AllDirectories))
            {
                ConvertFile(file);

            }
        }

        private void ConvertFile(string file)
        {
            Console.WriteLine("Converting file: '{0}'", file);
            using var destination = GetDestinationFile(file);

            using var streamWriter = new StreamWriter(destination);
            bool hasWrittenHeader = _options.SingleDestinationFile && _options.HasWrittenHeader;
            int lineNo = 1;
            foreach (var line in File.ReadLines(file))
            {
                if (line.StartsWith("Mtr_Version", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!hasWrittenHeader)
                    {
                        streamWriter.WriteLine(line);
                        hasWrittenHeader = true;
                    }

                    ++lineNo;
                    continue;
                }

                var parts = line.Split(',', StringSplitOptions.None); // We want to keep empty fields
                var epochString = parts[1];
                if (!int.TryParse(epochString, CultureInfo.InvariantCulture, out var epoch))
                {
                    Console.Error.WriteLine("Failed to parse:'{0}' into a number on line:{1}", epochString, lineNo);
                }
                var time = DateTime.UnixEpoch.AddSeconds(epoch);
                var timeStr = time.ToString("s");// .ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture);
                parts[1] = timeStr;
                var result = string.Join(',', parts);
                streamWriter.WriteLine(result);
                ++lineNo;
            }
            _options.HasWrittenHeader = hasWrittenHeader;
        }

        private FileStream GetDestinationFile(string sourceFile)
        {
            if (_options.SingleDestinationFile)
            {
                var file = Path.Combine(_options.DestinationPath, _options.DestinationFile);
                return OpenFileStream(file);
            }
            var sourceDir = Path.GetDirectoryName(sourceFile);
            var fileName = Path.GetFileName(sourceFile);

            var sourcePath = Path.GetFullPath(_options.SourcePath);

            var subPath = sourceDir![sourcePath.Length..];

            if (subPath.StartsWith('\\'))
            {
                subPath = subPath[1..];
            }

            var destinationSubPath = Path.Combine(_options.DestinationPath, subPath);

            var destinationFile = Path.Combine(destinationSubPath, fileName);

            return OpenFileStream(destinationFile);
        }

        private FileStream OpenFileStream(string destinationFile)
        {
            var destinationDir = Path.GetDirectoryName(destinationFile);
            Directory.CreateDirectory(destinationDir!);
            if (_options.Overwrite && File.Exists(destinationFile) && !_alreadyOverwritten)
            {
                _alreadyOverwritten = true;
                return File.Open(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            return File.Open(destinationFile, FileMode.Append, FileAccess.Write, FileShare.None);
        }
    }
}
