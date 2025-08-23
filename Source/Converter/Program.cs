using CommandLine;

namespace Converter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var errorWriter = new ErrorConsoleWriter();
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
}
