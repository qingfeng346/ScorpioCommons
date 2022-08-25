using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Scorpio.Commons {
    public class ParamterInfoAttribute : Attribute {
        public bool required { get; set; } = true;
        public string label { get; set; }
        public string def { get; set; }
        public string param { get; set; }
        public ParamterInfoAttribute() { }
        public ParamterInfoAttribute(string label) {
            this.label = label;
        }
        public ParamterInfoAttribute(string label, bool required) {
            this.label = label;
            this.required = required;
        }
        public ParamterInfoAttribute(string label, string param) {
            this.label = label;
            this.param = param;
        }
        public ParamterInfoAttribute(string label, string param, bool required) {
            this.label = label;
            this.param = param;
            this.required = required;
        }
        public ParamterInfoAttribute(string label, string param, string def) {
            this.label = label;
            this.param = param;
            this.def = def;
        }
        public ParamterInfoAttribute(string label, string param, string def, bool required) {
            this.label = label;
            this.param = param;
            this.def = def;
            this.required = required;
        }
        internal void SetName(string name) {
            var pars = new HashSet<string>();
            pars.Add($"-{name}");
            if (param != null && param.Length > 0) {
                pars.UnionWith(param.Split('|'));
            }
            pars.Remove("");
            finishParam = pars.ToArray();
        }
        internal string[] finishParam { get; private set; }
        internal string finishParamLabel => string.Join("|", finishParam);
    }
    public class Perform {
        public class ExecuteData {
            public string help;
            public string desc;
            public Delegate execute;
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
                logger.info($@"{Type} - {data.desc}
------------------
{data.help}");
            } else {
                data.execute.DynamicInvoke(GetParameters(data.execute, CommandLine));
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
                    } else if (info.required) {
                        throw new Exception($"参数 {info.finishParamLabel} 是必须的, 不可为空");
                    } else if (!string.IsNullOrEmpty(info.def)) {
                        args[i] = info.def.ChangeType(paramType);
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
                builder.Append(info.required ? "(必须)" : "(选填)");
                builder.Append(info.label);
                if (!string.IsNullOrEmpty(info.def)) {
                    builder.Append($" 默认:{info.def}");
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
            executes[type.ToLowerInvariant()] = new ExecuteData() { desc = desc, help = GetHelp(execute), execute = execute };
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
