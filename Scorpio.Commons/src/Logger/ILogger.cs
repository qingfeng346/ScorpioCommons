namespace Scorpio.Commons {
    public interface ILogger {
        /// <summary> debug 信息 </summary>
        void debug(string format);
        /// <summary> info 信息 </summary>
        void info(string format);
        /// <summary> 警告 信息 </summary>
        void warn(string format);
        /// <summary> 错误 信息 </summary>
        void error(string format);
    }
}
