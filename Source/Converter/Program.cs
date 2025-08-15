using CommandLine;

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
}
