using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Scorpio.Commons {
    public class Json {
        public static object Deserialize(string json, bool supportLong = true) {
            if (json == null) {return null; }
            return new Parser(json, supportLong).Parse();
        }
        public static string Serialize(object obj) {
            return new Serializer().Serialize(obj);
        }
        //public static string ToJson(object obj) {
        //    return new SerializerObject().Serialize(obj);
        //}
        //public static T FromJson<T>(string json) {
        //    return default(T);
        //}
        public class Parser {
            protected const char END_CHAR = (char)0;
            protected const char QUOTES = '"';            //引号
            protected const char LEFT_BRACE = '{';        //{
            protected const char RIGHT_BRACE = '}';       //}
            protected const char LEFT_BRACKET = '[';      //[
            protected const char RIGHT_BRACKET = ']';     //]
            protected const char COMMA = ',';             //,
            protected const string TRUE = "true";
            protected const string FALSE = "false";
            protected const string NULL = "null";

            const string WHITE_SPACE = " \t\n\r";
            const string WORD_BREAK = " \t\n\r{}[],:\"";

            protected string m_Buffer;
            protected bool m_SupportLong;         //是否支持 数字无[.]解析成long值
            protected int m_Index;
            protected int m_Length;
            public Parser(string buffer, bool supportLong) {
                m_SupportLong = supportLong;
                m_Buffer = buffer;
                m_Index = 0;
                m_Length = buffer.Length;
            }
            char Read() { return m_Index == m_Length ? END_CHAR : m_Buffer[m_Index++]; }
            char Peek() { return m_Index == m_Length ? END_CHAR : m_Buffer[m_Index]; }
            public object Parse() { return ReadObject(); }
            char EatWhiteSpace {
                get {
                    while (WHITE_SPACE.IndexOf(Peek()) != -1) {
                        ++m_Index;
                        if (Peek() == END_CHAR) {
                            return END_CHAR;
                        }
                    }
                    return Read();
                }
            }
            string NextWord {
                get {
                    var builder = new StringBuilder();
                    while (WORD_BREAK.IndexOf(Peek()) == -1) {
                        builder.Append(Read());
                        if (Peek() == END_CHAR) {
                            return builder.ToString();
                        }
                    }
                    return builder.ToString();
                }
            }
            object ReadObject() {
                var ch = EatWhiteSpace;
                if (ch == END_CHAR) {
                    return null;
                }
                switch (ch) {
                    case QUOTES: return ParseString();
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        --m_Index;
                        return ParseNumber();
                    case LEFT_BRACE: return ParseMap();
                    case LEFT_BRACKET: return ParseArray();
                    default:
                        --m_Index;
                        var word = NextWord;
                        switch (word) {
                            case TRUE: return true;
                            case FALSE: return false;
                            case NULL: return null;
                            default: throw new Exception("Json解析, 未知标识符 : " + word);
                        }
                }
            }
            string ParseString() {
                var m_Builder = new StringBuilder();
                while (true) {
                    if (Peek() == -1) {
                        return m_Builder.ToString();
                    }
                    var ch = Read();
                    if (ch == QUOTES) {
                        return m_Builder.ToString();
                    }
                    switch (ch) {
                        case '\\':
                            ch = Read();
                            switch (ch) {
                                case '\'': m_Builder.Append('\''); break;
                                case '\"': m_Builder.Append('\"'); break;
                                case '\\': m_Builder.Append('\\'); break;
                                case 'a': m_Builder.Append('\a'); break;
                                case 'b': m_Builder.Append('\b'); break;
                                case 'f': m_Builder.Append('\f'); break;
                                case 'n': m_Builder.Append('\n'); break;
                                case 'r': m_Builder.Append('\r'); break;
                                case 't': m_Builder.Append('\t'); break;
                                case 'v': m_Builder.Append('\v'); break;
                                case '0': m_Builder.Append('\0'); break;
                                case '/': m_Builder.Append("/"); break;
                                case 'u': {
                                    var hex = new StringBuilder();
                                    for (int i = 0; i < 4; i++) {
                                        hex.Append(Read());
                                    }
                                    m_Builder.Append((char)System.Convert.ToUInt16(hex.ToString(), 16));
                                    break;
                                }
                            }
                            break;
                        default:
                            m_Builder.Append(ch);
                            break;
                    }
                }
            }
            object ParseNumber() {
                var number = NextWord;
                if (m_SupportLong && number.IndexOf('.') == -1) {
                    long parsedLong;
                    long.TryParse(number, out parsedLong);
                    return parsedLong;
                }
                double parsedDouble;
                double.TryParse(number, out parsedDouble);
                return parsedDouble;
            }
            Dictionary<string, object> ParseMap() {
                var map = new Dictionary<string, object>();
                while (true) {
                    var ch = EatWhiteSpace;
                    switch (ch) {
                        case RIGHT_BRACE: return map;
                        case COMMA: continue;
                        case END_CHAR:
                            throw new Exception("Json解析, 未找到 map 结尾 [}]");
                        case QUOTES: {
                            var key = ParseString();
                            if (EatWhiteSpace != ':') {
                                throw new Exception("Json解析, key值后必须跟 [:] 赋值");
                            }
                            map[key] = ReadObject();
                            break;
                        }
                        default: {
                            throw new Exception("Json解析, key值 未知符号 : " + ch);
                        }
                    }
                }
            }
            List<object> ParseArray() {
                var array = new List<object>();
                while (true) {
                    var ch = EatWhiteSpace;
                    switch (ch) {
                        case RIGHT_BRACKET: return array;
                        case COMMA: continue;
                        case END_CHAR:
                            throw new Exception("Json解析, 未找到array结尾 ]");
                        default: {
                            --m_Index;
                            array.Add(ReadObject());
                            continue;
                        }
                    }
                }
            }
        }
        public class ParserObject : Parser {
            public ParserObject(string buffer, bool supportLong) : base(buffer, supportLong) { }
            public object Parse(Type type) { return ReadObject(type); }
            object ReadObject(Type type) {
                var value = System.Activator.CreateInstance(type);

                return value;
            }
        }
        public class Serializer {
            private StringBuilder builder;
            public Serializer() {
                builder = new StringBuilder();
            }
            public virtual string Serialize(object obj) {
                SerializeValue(obj);
                return builder.ToString();
            }
            protected virtual void SerializeValue(object value) {
                switch (value) {
                    case null: builder.Append("null"); return;
                    case bool b: builder.Append(b ? "true" : "false"); return;
                    case string str: SerializeString(str); return;
                    case char c: SerializeString(c.ToString()); return;
                    case IList list: SerializeArray(list); return;
                    case IDictionary dict: SerializeDict(dict); return;
                    default: SerializeOther(value); return;
                }
            }
            protected void SerializeString(string str) {
                builder.Append('\"');

                char[] charArray = str.ToCharArray();
                foreach (var c in charArray) {
                    switch (c) {
                        case '"':
                            builder.Append("\\\"");
                            break;
                        case '\\':
                            builder.Append("\\\\");
                            break;
                        case '\b':
                            builder.Append("\\b");
                            break;
                        case '\f':
                            builder.Append("\\f");
                            break;
                        case '\n':
                            builder.Append("\\n");
                            break;
                        case '\r':
                            builder.Append("\\r");
                            break;
                        case '\t':
                            builder.Append("\\t");
                            break;
                        default:
                            int codepoint = Convert.ToInt32(c);
                            if ((codepoint >= 32) && (codepoint <= 126)) {
                                builder.Append(c);
                            } else {
                                builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                            }
                            break;
                    }
                }
                builder.Append('\"');
            }
            protected void SerializeArray(IList anArray) {
                builder.Append('[');
                bool first = true;
                foreach (object obj in anArray) {
                    if (!first) {
                        builder.Append(',');
                    }
                    first = false;
                    SerializeValue(obj);
                }
                builder.Append(']');
            }
            protected void SerializeDict(IDictionary obj) {
                bool first = true;
                builder.Append('{');
                foreach (object key in obj.Keys) {
                    if (!first) {
                        builder.Append(',');
                    }
                    SerializeString(key.ToString());
                    builder.Append(':');
                    SerializeValue(obj[key]);
                    first = false;
                }
                builder.Append('}');
            }
            void SerializeOther(object value) {
                if (value is float
                    || value is int
                    || value is uint
                    || value is long
                    || value is double
                    || value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is ulong
                    || value is decimal) {
                    builder.Append(value.ToString());
                } else {
                    SerializeString(value.ToString());
                }
            }
        }
        public class SerializerObject : Serializer {
            private StringBuilder builder;
            public SerializerObject() {
                builder = new StringBuilder();
            }
            public override string Serialize(object obj) {
                SerializeValue(obj);
                return builder.ToString();
            }
            protected override void SerializeValue(object value) {
                switch (value) {
                    case null: builder.Append("null"); return;
                    case bool b: builder.Append(b ? "true" : "false"); return;
                    case string str: SerializeString(str); return;
                    case char c: SerializeString(c.ToString()); return;
                    case IList list: SerializeArray(list); return;
                    case IDictionary dict: SerializeDict(dict); return;
                }
                if (value is sbyte || value is byte || value is short || value is ushort ||
                    value is int || value is uint || value is long || value is ulong ||
                    value is float || value is double || value is decimal) {
                    builder.Append(value.ToString());
                } else {
                    SerializeObject(value);
                }
            }
            void SerializeObject(object value) {
                var fields = value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                bool first = true;
                builder.Append('{');
                foreach (var field in fields) {
                    if (!first) { builder.Append(','); }
                    SerializeString(field.Name);
                    builder.Append(':');
                    SerializeValue(field.GetValue(value));
                }
                builder.Append('}');
            }
        }
    }
}
