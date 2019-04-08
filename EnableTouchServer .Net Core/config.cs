using System;
using System.Text.RegularExpressions;

namespace bh3tool
{
    public class config
    {
        public int port;
        public bool Bh3Only;
        public bool EnableIos;
        public bool EnableAndroid ;

        public config(string[] str)
        {
            foreach (string s in str)
            {
                if (s.StartsWith('#'))
                    continue;
                if (s.Contains("Port") || s.Contains("port"))
                {
                    var ss = Regex.Match(s, @"[0-9]+");
                    port = Convert.ToInt32(ss.Value);
                }
                if (s.Contains("Bh3UrlOnly"))
                {
                    if (s.Contains("true"))
                        Bh3Only = true;
                    if (s.Contains("false"))
                        Bh3Only = false;
                }
                if (s.Contains("EnableIos"))
                {
                    if (s.Contains("true"))
                        EnableIos = true;
                    if (s.Contains("false"))
                        EnableIos = false;
                }
                if (s.Contains("EnableAndroid"))
                {
                    if (s.Contains("true"))
                        EnableAndroid = true;
                    if (s.Contains("false"))
                        EnableAndroid = false;
                }
            }
        }
    }
}
