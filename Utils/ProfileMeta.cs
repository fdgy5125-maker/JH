using System;
using System.Collections.Generic;

namespace MikrNSN.Utils
{
    public static class ProfileMeta
    {
        public static Dictionary<string, string> Parse(string comment)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(comment)) return dict;
            var parts = comment.Split(';');
            foreach (var p in parts)
            {
                var idx = p.IndexOf('=');
                if (idx > 0)
                {
                    var k = p.Substring(0, idx).Trim();
                    var v = p.Substring(idx + 1).Trim();
                    if (!dict.ContainsKey(k)) dict[k] = v;
                }
            }
            return dict;
        }

        public static string Serialize(Dictionary<string, string> dict)
        {
            var list = new List<string>();
            foreach (var kv in dict)
            {
                list.Add(kv.Key + "=" + kv.Value);
            }
            return string.Join(";", list);
        }
    }
}
