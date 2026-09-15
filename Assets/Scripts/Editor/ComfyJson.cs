using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RunnerZone.EditorTools.ComfyUI
{
    public static class ComfyJson
    {
        // ========== SERIALIZATION ==========
        public static string Serialize(object obj)
        {
            StringBuilder sb = new StringBuilder(4096);
            Write(sb, obj);
            return sb.ToString();
        }

        private static void Write(StringBuilder sb, object v)
        {
            if (v == null) { sb.Append("null"); return; }
            if (v is bool b) { sb.Append(b ? "true" : "false"); return; }
            if (v is string s) { WriteString(sb, s); return; }
            if (v is char c) { WriteString(sb, c.ToString(CultureInfo.InvariantCulture)); return; }
            if (v is byte || v is sbyte || v is short || v is ushort || v is int || v is uint || v is long || v is ulong)
            { sb.Append(Convert.ToString(v, CultureInfo.InvariantCulture)); return; }
            if (v is float f) { sb.Append(f.ToString("R", CultureInfo.InvariantCulture)); return; }
            if (v is double d) { sb.Append(d.ToString("R", CultureInfo.InvariantCulture)); return; }
            if (v is decimal dec) { sb.Append(dec.ToString(CultureInfo.InvariantCulture)); return; }
            System.Collections.IDictionary dict = v as System.Collections.IDictionary;
            if (dict != null) { WriteDictionary(sb, dict); return; }
            System.Collections.IEnumerable en = v as System.Collections.IEnumerable;
            if (en != null && !(v is byte[])) { WriteArray(sb, en); return; }
            WriteString(sb, v.ToString());
        }

        private static void WriteDictionary(StringBuilder sb, System.Collections.IDictionary dict)
        {
            sb.Append('{');
            bool first = true;
            foreach (System.Collections.DictionaryEntry kv in dict)
            {
                if (!first) sb.Append(',');
                first = false;
                WriteString(sb, kv.Key == null ? "" : kv.Key.ToString());
                sb.Append(':');
                Write(sb, kv.Value);
            }
            sb.Append('}');
        }

        private static void WriteArray(StringBuilder sb, System.Collections.IEnumerable en)
        {
            sb.Append('[');
            bool first = true;
            foreach (object item in en)
            {
                if (!first) sb.Append(',');
                first = false;
                Write(sb, item);
            }
            sb.Append(']');
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                            sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        // ========== PARSING ==========
        public static object Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            StringReader r = new StringReader(json);
            SkipWs(r);
            object res = ParseAny(r);
            SkipWs(r);
            return res;
        }

        // Helpers to cast without System.Text.Json
        public static bool TryGet(object node, string key, out object val)
        {
            val = null;
            Dictionary<string, object> dict = node as Dictionary<string, object>;
            if (dict == null) return false;
            return dict.TryGetValue(key, out val);
        }

        public static string AsString(object v) => v == null ? null : Convert.ToString(v, CultureInfo.InvariantCulture);

        public static int AsInt(object v) { if (v == null) return 0; if (v is int i) return i; try { return Convert.ToInt32(v, CultureInfo.InvariantCulture); } catch { return 0; } }

        public static bool? AsBool(object v)
        {
            if (v == null) return null;
            if (v is bool b) return b;
            string s = AsString(v);
            if (s == "true") return true;
            if (s == "false") return false;
            return null;
        }

        public static Dictionary<string, object> AsDict(object v) => v as Dictionary<string, object>;

        public static List<object> AsList(object v) => v as List<object>;

        public static IEnumerable<object> ItemsOf(object v) { List<object> l = AsList(v); if (l == null) yield break; foreach (object o in l) yield return o; }

        public static IEnumerable<KeyValuePair<string, object>> PropsOf(object v)
        {
            Dictionary<string, object> d = AsDict(v);
            if (d == null) yield break;
            foreach (KeyValuePair<string, object> kv in d) yield return kv;
        }

        private static void SkipWs(StringReader r)
        {
            int c;
            while ((c = r.Peek()) != -1)
            {
                char ch = (char)c;
                if (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r') r.Read();
                else break;
            }
        }

        private static object ParseAny(StringReader r)
        {
            SkipWs(r);
            int c = r.Peek();
            if (c == -1) return null;
            char ch = (char)c;
            switch (ch)
            {
                case '{': return ParseObject(r);
                case '[': return ParseArray(r);
                case '"': return ParseString(r);
                case 't': case 'f': return ParseBool(r);
                case 'n': return ParseNull(r);
                default:
                    if (ch == '-' || (ch >= '0' && ch <= '9')) return ParseNumber(r);
                    throw new FormatException("Unexpected char: '" + ch + "' at pos... JSON parse.");
            }
        }

        private static Dictionary<string, object> ParseObject(StringReader r)
        {
            Dictionary<string, object> res = new Dictionary<string, object>
                (StringComparer.Ordinal);
            Expect(r, '{');
            SkipWs(r);
            if (PeekChar(r) == '}') { r.Read(); return res; }
            while (true)
            {
                SkipWs(r);
                string key = ParseString(r);
                SkipWs(r);
                Expect(r, ':');
                SkipWs(r);
                object val = ParseAny(r);
                res[key] = val;
                SkipWs(r);
                int p = r.Peek();
                if (p == ',') { r.Read(); continue; }
                if (p == '}') { r.Read(); return res; }
                throw new FormatException("Expected ',' or '}' in JSON object.");
            }
        }

        private static List<object> ParseArray(StringReader r)
        {
            List<object> res = new List<object>();
            Expect(r, '[');
            SkipWs(r);
            if (PeekChar(r) == ']') { r.Read(); return res; }
            while (true)
            {
                SkipWs(r);
                res.Add(ParseAny(r));
                SkipWs(r);
                int p = r.Peek();
                if (p == ',') { r.Read(); continue; }
                if (p == ']') { r.Read(); return res; }
                throw new FormatException("Expected ',' or ']' in JSON array.");
            }
        }

        private static string ParseString(StringReader r)
        {
            Expect(r, '"');
            StringBuilder sb = new StringBuilder(256);
            while (true)
            {
                int c = r.Read();
                if (c == -1) throw new FormatException("Unterminated string in JSON");
                char ch = (char)c;
                if (ch == '"') return sb.ToString();
                if (ch == '\\')
                {
                    int esc = r.Read();
                    if (esc == -1) throw new FormatException("Bad escape in JSON");
                    switch (esc)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            char[] hex = new char[4];
                            for (int i = 0; i < 4; i++)
                            {
                                int h = r.Read();
                                if (h == -1) throw new FormatException("Bad \\u in JSON");
                                hex[i] = (char)h;
                            }
                            string code = new string(hex);
                            sb.Append((char)int.Parse(code, NumberStyles.HexNumber,
                                CultureInfo.InvariantCulture));
                            break;
                        default:
                            sb.Append((char)esc);
                            break;
                    }
                }
                else sb.Append(ch);
            }
        }

        private static object ParseNumber(StringReader r)
        {
            StringBuilder sb = new StringBuilder(32);
            bool isFloat = false;
            if (PeekChar(r) == '-') { sb.Append((char)r.Read()); }
            while (r.Peek() != -1)
            {
                char ch = (char)r.Peek();
                if (ch >= '0' && ch <= '9') sb.Append((char)r.Read());
                else if (ch == '.' || ch == 'e' || ch == 'E' || ch == '+' || ch == '-')
                { isFloat = true; sb.Append((char)r.Read()); }
                else break;
            }
            string s = sb.ToString();
            if (isFloat)
            {
                if (double.TryParse(s, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double d)) return d;
                return 0d;
            }
            if (long.TryParse(s, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out long l))
            {
                if (l >= int.MinValue && l <= int.MaxValue) return (int)l;
                return l;
            }
            return 0;
        }

        private static bool ParseBool(StringReader r)
        {
            if (PeekChar(r) == 't')
            {
                if (r.Read() != 't' || r.Read() != 'r' || r.Read() != 'u' || r.Read() != 'e')
                    throw new FormatException("Bad 'true' in JSON");
                return true;
            }
            if (r.Read() != 'f' || r.Read() != 'a' || r.Read() != 'l' ||
                r.Read() != 's' || r.Read() != 'e')
                throw new FormatException("Bad 'false' in JSON");
            return false;
        }

        private static object ParseNull(StringReader r)
        {
            if (r.Read() != 'n' || r.Read() != 'u' || r.Read() != 'l' || r.Read() != 'l')
                throw new FormatException("Bad 'null' in JSON");
            return null;
        }

        private static char PeekChar(StringReader r)
        {
            int p = r.Peek();
            return p == -1 ? '\0' : (char)p;
        }

        private static void Expect(StringReader r, char c)
        {
            int g = r.Read();
            if (g == -1 || (char)g != c)
                throw new FormatException("Expected '" + c + "' in JSON but got '" +
                    (g == -1 ? "EOF" : ((char)g).ToString()) + "'.");
        }
    }
}
