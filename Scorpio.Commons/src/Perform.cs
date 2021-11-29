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
        public string Type { get; private set; }
        public void Start(string[] args) {
            Start(args, null, null);
        }
        public void Start(string[] args, Action<Perform, string, CommandLine, string[]> pre, Action<Perform, string, CommandLine, string[]> post) {
            command = CommandLine.Parse(args);
            Type = command.Type.ToLowerInvariant();
            var hasHelp = command.HadValue("-help", "-h", "--help");
            if (Type == "help") {
                PrintHelp();
                return;
            }
            var hasDef = executes.ContainsKey("");                      //默认执行函数
            if (!executes.ContainsKey(Type)) {
                if (hasDef) {
                    Type = "";
                } else {
                    PrintHelp();
                    return;
                }
            }
            pre?.Invoke(this, Type, command, args);
            var data = executes[Type];
            if (hasHelp) {
                Logger.info(data.help);
            } else {
                data.execute?.Invoke(this, command, args);
            }
            post?.Invoke(this, Type, command, args);
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
            executes[type.ToLowerInvariant()] = new ExecuteData() { help = help, execute = execute };
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
