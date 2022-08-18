using System;
using System.Collections.Generic;

namespace Scorpio.Commons {
    public class CommandLine {
        private readonly string[] EMPTY_ARGS = new string[0];
        public class Argument {
            private List<string> args = new List<string>();
            public Argument(string name) {
                Name = name;
            }
            public string Name { get; private set; }
            public IReadOnlyList<string> Args => args;
            public void Add(string arg) {
                args.Add(arg);
            }
            public string[] GetValues() {
                return args.ToArray();
            }
            public string GetValue(string def) {
                return args.Count > 0 ? args[0] : def;
            }
        }
        public static CommandLine Parse(string[] args) {
            return new CommandLine().Parser(args);
        }
        private Dictionary<string, Argument> arguments = new Dictionary<string, Argument>();
        public string Type { get; private set; }
        public List<string> Args { get; } = new List<string>();
        public IReadOnlyDictionary<string, Argument> Arguments => arguments;
        public CommandLine Parser(string[] args) {
            arguments.Clear();
            Type = "";
            Argument argument = null;
            for (int i = 0; i < args.Length; ++i) {
                var arg = args[i];
                if (arg.StartsWith("-")) {
                    if (arguments.ContainsKey(arg)) {
                        argument = arguments[arg];
                    } else {
                        argument = new Argument(arg);
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
            return arguments.ContainsKey(key) ? arguments[key].GetValues() : EMPTY_ARGS;
        }
        public string[] GetValues(string[] keys) {
            var values = new List<string>();
            foreach (var key in keys) {
                if (arguments.ContainsKey(key)) {
                    values.AddRange(arguments[key].GetValues());
                }
            }
            return values.ToArray();
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
            return (T)GetValue(key, typeof(T));
        }
        public T GetValue<T>(params string[] keys) {
            return (T)GetValue(typeof(T), keys);
        }
        public object GetValue(string key, Type type) {
            return GetValue(key).ChangeType(type);
        }
        public object GetValue(Type type, params string[] keys) {
            return GetValue(keys).ChangeType(type);
        }
    }
}