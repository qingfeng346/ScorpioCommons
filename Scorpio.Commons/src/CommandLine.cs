using System;
using System.Collections.Generic;

namespace Scorpio.Commons {
    public class CommandLineArgument {
        private List<string> args = new List<string>();
        public CommandLineArgument(string name) {
            Name = name;
        }
        public string Name { get; private set; }
        public void Add(string arg) {
            args.Add(arg);
        }
        public string[] GetValues() {
            return args.Count > 0 ? args.ToArray() : null;
        }
        public string GetValue(string def) {
            return args.Count > 0 ? args[0] : def;
        }
    }
    public class CommandLine {
        public static CommandLine Parse(string[] args) {
            return new CommandLine().Parser(args);
        }
        private Dictionary<string, CommandLineArgument> arguments = new Dictionary<string, CommandLineArgument>();
        public string Type { get; private set; }
        public List<string> Args { get; } = new List<string>();
        public CommandLine Parser(string[] args) {
            arguments.Clear();
            Type = "";
            CommandLineArgument argument = null;
            for (int i = 0; i < args.Length; ++i) {
                var arg = args[i];
                if (arg.StartsWith("-")) {
                    if (arguments.ContainsKey(arg)) {
                        argument = arguments[arg];
                    } else {
                        argument = new CommandLineArgument(arg);
                        arguments[arg] = argument;
                    }
                } else if (argument != null) {
                    argument.Add(arg);
                } else if (string.IsNullOrWhiteSpace(Type)) {
                    Type = arg;
                } else {
                    Args.Add(arg);
                }
            }
            return this;
        }
        public bool HadValue(string key) {
            return arguments.ContainsKey(key);
        }
        public bool HadValue(params string[] keys) {
            foreach (var key in keys) {
                if (arguments.ContainsKey(key)) {
                    return true;
                }
            }
            return false;
        }
        public string[] GetValues(string key) {
            return arguments.ContainsKey(key) ? arguments[key].GetValues() : null;
        }
        public string GetValue(string key) {
            return GetValueDefault(key, null);
        }
        public string GetValue(params string[] keys) {
            return GetValueDefault(keys, null);
        }
        public string GetValueDefault(string key, string def) {
            return GetValueDefault(new string[] { key }, def);
        }
        public string GetValueDefault(string[] keys, string def) {
            foreach (var key in keys) {
                if (arguments.ContainsKey(key)) {
                    return arguments[key].GetValue(def);
                }
            }
            return def;
        }
        public T GetValue<T>(string key) {
            return (T)Convert.ChangeType(GetValue(key), typeof(T));
        }
        public T GetValue<T>(params string[] keys) {
            return (T)Convert.ChangeType(GetValue(keys), typeof(T));
        }
    }
}