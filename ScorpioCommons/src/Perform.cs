using System;
using System.IO;
using System.Collections.Generic;
namespace Scorpio.Commons {
    public class Perform {
        public class ExecuteData {
            public string help;
            public Action<Perform, CommandLine, string[]> execute;
        }
        private CommandLine command;
        private Dictionary<string, ExecuteData> executes = new Dictionary<string, ExecuteData>();
        public string Help = "";
        public void Start(string[] args) {
            Start(args, null, null);
        }
        public void Start(string[] args, Action<Perform, string, CommandLine, string[]> pre, Action<Perform, string, CommandLine, string[]> post) {
            var type = "";
            try {
                command = CommandLine.Parse(args);
                var hasHelp = command.HadValue("-help", "-h", "--help");
                type = command.Type;
                if (type == "help") {
                    PrintHelp();
                    return;
                }
                var hasDef = executes.ContainsKey("");                      //默认执行函数
                if (!executes.ContainsKey(type)) {
                    if (hasDef) {
                        type = "";
                    } else {
                        PrintHelp();
                        return;
                    }
                }
                pre?.Invoke(this, type, command, args);
                var data = executes[type];
                if (hasHelp) {
                    Logger.info(data.help);
                } else {
                    data.execute?.Invoke(this, command, args);
                }
                post?.Invoke(this, type, command, args);
            } catch (Exception e) {
                Logger.error("执行命令 [{0}] 出错 : {1}", type, e.ToString());
            }
        }
        void PrintHelp() {
            if (!Help.isNullOrWhiteSpace()) { Logger.info(Help); }
            foreach (var pair in executes) {
                if (pair.Key != "") {
                    Logger.info("-------------------------------------");
                    Logger.info(pair.Key + "\n  " + pair.Value.help);
                }
            }
        }
        public void AddExecute(string type, string help, Action<Perform, CommandLine, string[]> execute) {
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
            return Path.GetFullPath(Path.Combine(Util.CurrentDirectory, path));
        }
    }
}
