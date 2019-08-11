using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetPackageTool
{
    class CmdidList
    {
        public static void SaveListToFile(string srcFilePath)
        {
            List<KeyValuePair<int, string>> list = GetList(srcFilePath);

            StringBuilder @string = new StringBuilder();

            list.Sort((x, y) => x.Key.CompareTo(y.Key));
            @string = new StringBuilder();
            foreach (var a in list)
            {
                @string.AppendLine(a.Key + " " + a.Value);
            }

            FileDialog fileDialog = new SaveFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
                File.WriteAllText(fileDialog.FileName, @string.ToString());

        }

        public static List<KeyValuePair<int, string>> GetList(string srcFilePath)
        {
            FileStream fileStream = new FileStream(srcFilePath, FileMode.Open);
            StreamReader reader = new StreamReader(fileStream);
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();

            string classname = "";
            int cmdid;
            string line;
            Regex rege_cmdid = new Regex("	public const ([a-zA-Z]+)\\.CmdId CMD_ID = (\\d+)");
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("	public const ") && rege_cmdid.IsMatch(line))
                {

                    classname = rege_cmdid.Match(line).Groups[1].Value;
                    cmdid = Convert.ToInt32(rege_cmdid.Match(line).Groups[2].Value);
                    list.Add(new KeyValuePair<int, string>(cmdid, classname));
                }
            }
            return list;
        }
    }
}
