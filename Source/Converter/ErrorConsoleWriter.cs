using System.Text;

namespace Converter
{
    /// <summary>
    /// Simple wrapper around Console.Error so its possible to write error messages in red
    /// <code>using var errorWriter = new ErrorConsoleWriter();</code>
    /// </summary>
    public class ErrorConsoleWriter : TextWriter 
    {
        private readonly TextWriter _original;

        public ErrorConsoleWriter()
        {
            _original = Console.Error;
            Console.SetError(this);
        }

        public override Encoding Encoding => _original.Encoding;

        public override void WriteLine(string? value)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                _original.WriteLine(value);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        protected override void Dispose(bool disposing)
        {
            Console.SetError(_original);
            base.Dispose(disposing);
        }
    }
}