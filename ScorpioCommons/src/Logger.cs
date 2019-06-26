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
        public static void info(object format) {
            if (logger != null) logger.info(format == null ? "" : format.ToString());
        }
        public static void info(string format) {
            if (logger != null) logger.info(format);
        }
        public static void info(string format, params object[] args) {
            if (logger != null) logger.info(string.Format(format, args));
        }
        public static void warn(object format) {
            if (logger != null) logger.warn(format == null ? "" : format.ToString());
        }
        public static void warn(string format) {
            if (logger != null) logger.warn(format);
        }
        public static void warn(string format, params object[] args) {
            if (logger != null) logger.warn(string.Format(format, args));
        }
        public static void error(object format) {
            if (logger != null) logger.error(format == null ? "" : format.ToString());
        }
        public static void error(string format) {
            if (logger != null) logger.error(format);
        }
        public static void error(string format, params object[] args) {
            if (logger != null) logger.error(string.Format(format, args));
        }
    }
}
