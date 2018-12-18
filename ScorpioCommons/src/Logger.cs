using System;
using System.Collections.Generic;
using System.Text;

namespace Scorpio.Commons {
    public interface ILogger {
        void info(string value);
        void warn(string value);
        void error(string value);
    }
    public static class Logger {
        private static ILogger logger;
        public static void SetLogger(ILogger logger) {
            Logger.logger = logger;
        }
        public static void info(string format, params object[] args) {
            if (logger != null) logger.info(string.Format(format, args));
        }
        public static void warn(string format, params object[] args) {
            if (logger != null) logger.warn(string.Format(format, args));
        }
        public static void error(string format, params object[] args) {
            if (logger != null) logger.error(string.Format(format, args));
        }
    }
}
