namespace Scorpio.Commons {
    /// <summary> 日志类 </summary>
    public static class logger {
        static logger() {
            SetLogger(new ConsoleLogger());
        }
        private static ILogger log = null;
        /// <summary> 设置日志对象 </summary>
        public static void SetLogger(ILogger ilog) {
            log = ilog;
        }
        /// <summary> debug输出 </summary>
        public static void debug(object format) {
            if (log == null) return;
            log.debug($"{format}");
        }
        /// <summary> debug输出 </summary>
        public static void debug(string format) {
            if (log == null) return;
            log.debug(format);
        }
        /// <summary> debug输出 </summary>
        public static void debug(string format, params object[] args) {
            if (log == null) return;
            log.debug(string.Format(format, args));
        }
        
        /// <summary> info输出 </summary>
        public static void info(object format) {
            if (log == null) return;
            log.info($"{format}");
        }
        /// <summary> info输出 </summary>
        public static void info(string format) {
            if (log == null) return;
            log.info(format);
        }
        /// <summary> info输出 </summary>
        public static void info(string format, params object[] args) {
            if (log == null) return;
            log.info(string.Format(format, args));
        }

        /// <summary> warn输出 </summary>
        public static void warn(object format) {
            if (log == null) return;
            log.warn($"{format}");
        }
        /// <summary> warn输出 </summary>
        public static void warn(string format) {
            if (log == null) return;
            log.warn(format);
        }
        /// <summary> warn输出 </summary>
        public static void warn(string format, params object[] args) {
            if (log == null) return;
            log.warn(string.Format(format, args));
        }
        
        /// <summary> error输出 </summary>
        public static void error(object format) {
            if (log == null) return;
            log.error($"{format}");
        }
        /// <summary> error输出 </summary>
        public static void error(string format) {
            if (log == null) return;
            log.error(format);
        }
        /// <summary> error输出 </summary>
        public static void error(string format, params object[] args) {
            if (log == null) return;
            log.error(string.Format(format, args));
        }
    }
}