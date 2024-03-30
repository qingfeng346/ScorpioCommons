using System;
namespace Scorpio.Commons {
    public class ConsoleLogger : ILogger {
        public void debug(string value) {
            Console.WriteLine($"[debug] {value}");
        }
        public void info(string value) {
            Console.WriteLine(value);
        }
        public void warn(string value) {
            Console.WriteLine($"[warn] {value}");
        }
        public void error(string value) {
            Console.Error.WriteLine($"[error] {value}");
        }
    }
}
