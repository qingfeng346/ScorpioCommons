using System;
using System.Collections.Generic;
using System.Linq;

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
            args = Array.ConvertAll(args, arg => arg.Trim());
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
            return ChangeType(GetValues(key), type);
        }
        public object GetValue(Type type, params string[] keys) {
            return ChangeType(GetValues(keys), type);
        }
        public static object ChangeType(string[] values, Type type) {
            if (type.IsArray) {
                var elementType = type.GetElementType();
                var vals = new List<string>();
                foreach (var v in values) {
                    vals.AddRange(v.Split(','));
                }
                var result = Array.CreateInstance(elementType, vals.Count);
                for (var i = 0; i < vals.Count; ++i) {
                    result.SetValue(ChangeElementType(vals[i], elementType), i);
                }
                return result;
            } else {
                return ChangeElementType(values.FirstOrDefault(), type);
            }
        }
        public static object ChangeElementType(string value, Type type) {
            if (type == typeof(string)) {
                return value;
            } else if (type == typeof(bool)) {
                if (string.IsNullOrEmpty(value)) {
                    return true;
                } else {
                    value = value.ToLowerInvariant();
                    return value == "true" || value == "yes" || value == "1";
                }
            } else if (type == typeof(sbyte) ||
                       type == typeof(byte) ||
                       type == typeof(short) ||
                       type == typeof(ushort) ||
                       type == typeof(int) ||
                       type == typeof(uint) ||
                       type == typeof(long) ||
                       type == typeof(ulong) ||
                       type == typeof(float) ||
                       type == typeof(double) ||
                       type == typeof(decimal)) {
                return Convert.ChangeType(value, type);
            } else if (type.IsEnum) {
                if (int.TryParse(value, out var result)) {
                    return Enum.ToObject(type, result);
                } else {
                    return Enum.Parse(type, value, true);
                }
            } else if (type == typeof(DateTime)) {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(double.Parse(value));
            } else {
                return null;
            }
        }
    }
}