using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Scorpio.Commons {
    public class ParamterInfoAttribute : Attribute {
        public bool Required { get; set; }
        public string Label { get; set; }
        public string[] Defaults { get; set; }
        public string[] Params { get; set; }
        public string Default { 
            get => string.Join("|", Defaults); 
            set => Defaults = value.Split('|');
        }
        public string Param { 
            get => string.Join("|", Params);
            set => Params = value.Split('|');
        }
        internal void SetName(string name) {
            var pars = new HashSet<string>();
            if (Params != null && Params.Length > 0) {
                pars.UnionWith(Params);
            } else {
                pars.Add($"-{name}");
            }
            pars.Remove("");
            finishParam = pars.ToArray();
        }
        internal string[] finishParam { get; private set; }
        internal string finishParamLabel => string.Join("|", finishParam);
    }
    public class Perform {
        public class ExecuteData {
            public string type;
            public string help;
            public string desc;
            public Delegate execute;
            public string FullHelp => $@"{type} - {desc}
------------------
{help}";
        }
        private Dictionary<string, ExecuteData> executes = new Dictionary<string, ExecuteData>();
        public string Help = "";
        public string Type { get; private set; }
        public CommandLine CommandLine { get; private set; }
        public event Action<Perform, string[]> PreAction;
        public event Action<Perform, string[]> PostAction;
        public void Start(string[] args) {
            CommandLine = CommandLine.Parse(args);
            Type = CommandLine.Type.ToLowerInvariant();
            var hasHelp = CommandLine.HadValue("-help", "-h", "--help");
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
            PreAction?.Invoke(this, args);
            var data = executes[Type];
            if (hasHelp) {
                logger.info(data.FullHelp);
            } else {
                try {
                    data.execute.DynamicInvoke(GetParameters(data.execute, CommandLine));
                } catch (Exception) {
                    logger.info(data.FullHelp);
                    throw;
                }
            }
            PostAction?.Invoke(this, args);
        }
        static object[] GetParameters(Delegate dele, CommandLine commandLine) {
            var parameters = dele.GetMethodInfo().GetParameters();
            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; ++i) {
                var param = parameters[i];
                var info = (ParamterInfoAttribute)param.GetCustomAttribute(typeof(ParamterInfoAttribute));
                var paramType = param.ParameterType;
                if (paramType == typeof(CommandLine)) {
                    args[i] = commandLine;
                    continue;
                }
                if (info != null) {
                    info.SetName(param.Name);
                    if (commandLine.HadValue(info.finishParam)) {
                        args[i] = commandLine.GetValue(paramType, info.finishParam);
                    } else if (info.Required) {
                        throw new Exception($"参数 {info.finishParamLabel} 是必须的, 不可为空");
                    } else if (info.Defaults != null && info.Defaults.Length > 0) {
                        args[i] = CommandLine.ChangeType(info.Defaults, paramType);
                    }
                    continue;
                }
                var name = $"-{param.Name}";
                if (commandLine.HadValue(name)) {
                    args[i] = commandLine.GetValue(name, paramType);
                } else if (param.HasDefaultValue) {
                    args[i] = param.DefaultValue;
                } else {
                    args[i] = paramType.IsValueType ? Activator.CreateInstance(paramType) : null;
                }
            }
            return args;
        }
        static string GetHelp(Delegate dele) {
            var builder = new StringBuilder();
            var parameters = dele.GetMethodInfo().GetParameters();
            var maxLength = 0;
            for (var i = 0; i < parameters.Length; ++i) {
                var param = parameters[i];
                var info = (ParamterInfoAttribute)param.GetCustomAttribute(typeof(ParamterInfoAttribute));
                if (info == null) { continue; }
                info.SetName(param.Name);
                maxLength = Math.Max(maxLength, info.finishParamLabel.Length);
            }
            for (var i = 0; i < parameters.Length; ++i) {
                var param = parameters[i];
                var info = (ParamterInfoAttribute)param.GetCustomAttribute(typeof(ParamterInfoAttribute));
                if (info == null) { continue; }
                info.SetName(param.Name);
                builder.Append($"  {info.finishParamLabel}".GetAlign(maxLength + 4));
                builder.Append(info.Required ? "(必须)" : "(选填)");
                builder.Append(info.Label);
                if (info.Defaults != null && info.Defaults.Length > 0) {
                    var def = string.Join(" ", info.Defaults);
                    builder.Append($" 默认值:{def}");
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
        void PrintHelp() {
            if (!Help.isNullOrWhiteSpace()) { logger.info(Help); }
            var maxLength = 0;
            foreach (var pair in executes) {
                if (pair.Key != "") {
                    maxLength = Math.Max(maxLength, pair.Key.Length);
                }
            }
            var builder = new StringBuilder();
            builder.AppendLine("所有命令");
            foreach (var pair in executes) {
                if (pair.Key != "") {
                    builder.Append(' ');
                    builder.Append(pair.Key.GetAlign(maxLength + 4));
                    builder.AppendLine(pair.Value.desc);
                }
            }
            logger.info(builder.ToString());
        }
        public void AddExecute(string type, Delegate execute) {
            AddExecute(type, type, execute);
        }
        public void AddExecute(string type, string desc, Delegate execute) {
            type = type.ToLowerInvariant();
            executes[type] = new ExecuteData() { type = type, desc = desc, help = GetHelp(execute), execute = execute };
        }
        public string GetPath(params string[] keys) {
            return GetPath(keys, false);
        }
        public string GetPath(string[] keys, bool throwException = false) {
            var path = CommandLine.GetValue(keys);
            if (path.isNullOrWhiteSpace()) {
                if (throwException) {
                    throw new Exception($"找不到 {string.Join("|", keys)} 参数");
                }
                path = "";
            }
            return Path.GetFullPath(Path.Combine(ScorpioUtil.CurrentDirectory, path));
        }
    }
}
