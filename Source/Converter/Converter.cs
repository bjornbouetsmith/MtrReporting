using System.Globalization;

namespace Converter
{
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
        /// <summary>
        /// Mtr_Version,Start_Time,Status,Host,Hop,Ip,Loss%,Snt, ,Last,Avg,Best,Wrst,StDev,
        /// </summary>
        /// <param name="file"></param>
        private void ConvertFile(string file)
        {
            Console.WriteLine("Converting file: '{0}'", file);
            using var destination = GetDestinationFile(file);

            using var streamWriter = new StreamWriter(destination);
            bool hasWrittenHeader = _options.SingleDestinationFile && _options.HasWrittenHeader;
            int lineNo = 1;
            foreach (var line in File.ReadLines(file))
            {
                var parts = line.Split(',', StringSplitOptions.None); // We want to keep empty fields
                if (parts[0].StartsWith("Mtr_Version", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!hasWrittenHeader)
                    {
                        var header = string.Join(_options.FieldSeparator, parts);
                        streamWriter.WriteLine(header);
                        hasWrittenHeader = true;
                    }

                    ++lineNo;
                    continue;
                }

                var epochString = parts[1];
                if (!int.TryParse(epochString, CultureInfo.InvariantCulture, out var epoch))
                {
                    Console.Error.WriteLine("Failed to parse:'{0}' into a number on line:{1}", epochString, lineNo);
                }
                var time = DateTime.UnixEpoch.AddSeconds(epoch);
                var timeStr = time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                parts[1] = timeStr;

                var lossString = parts[6];
                if (!double.TryParse(lossString, CultureInfo.InvariantCulture, out var loss))
                {
                    Console.Error.WriteLine("Failed to parse:'{0}' into a double on line:{1}", lossString, lineNo);
                }

                // if configured to trim lines where packet loss is 0, then we check and skip line if its 0
                if (_options.TrimZeroLossLines && loss < double.Epsilon)
                {
                    continue;
                }
                
                if (_options.ConvertNumbers)
                {
                    lossString = loss.ToString(CultureInfo.CurrentCulture);
                    parts[6] = lossString;
                }
                var result = string.Join(_options.FieldSeparator, parts);
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