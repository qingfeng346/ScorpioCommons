using System;
using System.IO;
using System.Collections.Generic;
namespace Scorpio.Commons {
    public class Perform {
        public class ExecuteData {
            public string help;
            public Action<CommandLine, string[]> execute;
        }
        private CommandLine command;
        private Dictionary<string, ExecuteData> executes = new Dictionary<string, ExecuteData>();
        public void Start(string[] args, Action<CommandLine, string[]> pre, Action<CommandLine, string[]> post) {
            var type = "";
            try {
                command = CommandLine.Parse(args);
                var hasHelp = command.HadValue("-help", "-h");
                type = command.Type.ToLower();
                if (!executes.ContainsKey(type)) { type = ""; }
                pre?.Invoke(command, args);
                var data = executes[type];
                if (hasHelp) {
                    Logger.info(data.help);
                } else {
                    data.execute(command, args);
                }
                post?.Invoke(command, args);
            } catch (Exception e) {
                Logger.error("执行命令 [{0}] 出错 : {1}", type, e.ToString());
            }
        }
        public void AddExecute(string type, string help, Action<CommandLine, string[]> execute) {
            executes[type.ToLower()] = new ExecuteData() { help = help, execute = execute };
        }
        public string GetPath(params string[] keys) {
            return GetPath(keys, false);
        }
        public string GetPath(string[] keys, bool throwException = false) {
            var path = command.GetValue(keys);
            if (path.isNullOrWhiteSpace()) {
                if (throwException) {
                    throw new Exception($"找不到 {string.Join("|", keys)} 参数");
                }
                path = "";
            }
            return Path.GetFullPath(Util.CurrentDirectory + "/" + path);
        }
    }
}
