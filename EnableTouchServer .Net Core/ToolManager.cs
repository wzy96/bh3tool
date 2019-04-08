using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UABT;

namespace bh3tool
{
    class ToolManager
    {
        //server list
        Dictionary<string, Server_modinfo> mod_servers = new Dictionary<string, Server_modinfo>
            {
                { "android01", new Server_modinfo(Gameserver.android01) },
                { "bb01", new Server_modinfo(Gameserver.bb01) },
                { "hun01", new Server_modinfo(Gameserver.hun01) },
                { "hun02", new Server_modinfo(Gameserver.hun02) },
                { "ios01", new Server_modinfo(Gameserver.ios01) },
                { "yyb01", new Server_modinfo(Gameserver.yyb01) }
            };
        private FiddlerTool Fiddlertool = new FiddlerTool();

        public int port = 8877;
        //ios data
        public byte[] i_dataversion;
        public byte[] i_setting;
        public byte[] i_excel_output;
        public DateTime i_time = new DateTime(0);
        //android data
        public byte[] a_dataversion;
        public byte[] a_setting;
        public byte[] a_excel_output;
        public DateTime a_time = new DateTime(0);

        bool Enableandroid = true;
        bool Enableios = true;
        /// <summary>
        /// start
        /// </summary>
        /// <param name="iport">proxy port</param>
        /// <param name="Bh3Only">is only response *.bh3.com and *.mihoyo.com</param>
        /// <param name="Enableandroid">Enable android</param>
        /// <param name="Enableios">Enable IOS</param>
        public void Start(int iport, bool Bh3Only, bool Enableandroid, bool Enableios)
        {
            this.Enableandroid = Enableandroid;
            this.Enableios = Enableios;
            Console.WriteLine("init server info");

            Console.WriteLine("start Fiddler,port:" + iport);
            FiddlerTool.bh3only = Bh3Only;

            port = iport;
            Fiddlertool.Start(port, this);

            Console.WriteLine("Get server info");
            string[] severlist = mod_servers.Keys.ToArray();

            foreach (string se in severlist)
                mod_servers[se].Enabletouch = Enableandroid;

            mod_servers["ios01"].Enabletouch = Enableios;
            //get all server info
            foreach (var se in severlist)
            {
                if (mod_servers[se].Enabletouch)
                {
                    if (!Getseverinfo(se))
                    {
                        Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " Get " + se + " info fail");
                        return;
                    }

                    if (string.IsNullOrEmpty(mod_servers[se].mod_gameserver_rsp))
                        Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " Get " + se + " info fail");
                    else
                        Console.WriteLine(se + " " + mod_servers[se].server_ip + " " + mod_servers[se].oa_ip);
                }
            }

            var ios = mod_servers["ios01"];
            var android = mod_servers["android01"];
            Console.WriteLine("process file");
            //Get File
            try
            {
                if (Enableios)
                {
                    var m = new FileMod();
                    if (m.ModFile(true, i_time))
                    {
                        i_dataversion = m._data;
                        i_excel_output = m._excel;
                        i_setting = m._setting;
                        i_time = m.time;
                    }
                    m = null;
                }
                if (Enableandroid)
                {
                    var c = new FileMod();
                    if (c.ModFile(false, a_time))
                    {
                        a_dataversion = c._data;
                        a_excel_output = c._excel;
                        a_setting = c._setting;
                        a_time = c.time;
                    }
                    c = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
            //start fiddler
            if (!Fiddler.FiddlerApplication.IsStarted())
            {

                port = iport;
                Fiddlertool.Start(port, this);
            }
            FiddlerTool.modinfos = mod_servers.Values.ToList();
        }

        /// <summary>
        /// Update File
        /// </summary>
        public void UpdateFile()
        {
            try
            {
                if (Enableios)
                {
                    var m = new FileMod();
                    if (m.ModFile(true, i_time))
                    {
                        i_dataversion = m._data;
                        i_excel_output = m._excel;
                        i_setting = m._setting;
                        i_time = m.time;
                    }
                    m = null;
                }
                if (Enableandroid)
                {
                    var c = new FileMod();
                    if (c.ModFile(false, a_time))
                    {
                        a_dataversion = c._data;
                        a_excel_output = c._excel;
                        a_setting = c._setting;
                        a_time = c.time;
                    }
                    c = null;
                }
                GC.Collect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }


        /// <summary>
        /// Get Server Info
        /// </summary>
        /// <param name="se"></param>
        /// <returns></returns>
        bool Getseverinfo(string se)
        {
            //用于匹配数据的正则
            Regex reg_serverurl = new Regex("(dispatch_url\":\")(.*?)\"");
            Regex reg_gameserver = new Regex("(gameserver\":)(\\{.*?\\})");
            Regex reg_geteway = new Regex("(gateway\":)(\\{.*?\\})");
            Regex reg_oaserver = new Regex("(oaserver_url\":)(\".*?\")");

            WebClient client = new WebClient();
            try
            {
                //note version number
                string version = "?version=3.0.0_" + GetserverStr(mod_servers[se].server);
                string globaleDispatchUrl = "http://global1.bh3.com/query_dispatch" + version;

                string rsp = client.DownloadString(globaleDispatchUrl);
                //get dispatch_url
                string dispatch_url = reg_serverurl.Match(rsp).Groups[2].Value;

                string qserverurl = dispatch_url + version;
                //get server rsponse
                string qrsp = client.DownloadString(qserverurl);
                //match server ip
                string gameserver = reg_gameserver.Match(qrsp).Groups[2].Value;
                string getway = reg_geteway.Match(qrsp).Groups[2].Value;
                string oa = reg_oaserver.Match(qrsp).Groups[2].Value;

                mod_servers[se].gameserver_url = qserverurl;
                mod_servers[se].gameserver_rsp = qrsp;
                mod_servers[se].server_ip = gameserver;
                mod_servers[se].getway_ip = getway;
                mod_servers[se].oa_ip = oa;
                //Replace "https" with "http"
                mod_servers[se].mod_gameserver_rsp = qrsp.Replace("https://bundle.bh3.com", "http://bundle.bh3.com");
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// get a server name
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        private string GetserverStr(Gameserver server)
        {
            switch (server)
            {
                case Gameserver.android01: return "gf_android";
                case Gameserver.bb01: return "bilibili";
                case Gameserver.hun01: return "oppo";
                case Gameserver.hun02: return "xiaomi";
                case Gameserver.ios01: return "gf_ios";
                case Gameserver.yyb01: return "tencent";
                default: return "gf_android";
            }

        }

    }

    /// <summary>
    /// edit touch buff
    /// </summary>
    class TouchBuff
    {
        static readonly int buffId = 6;
        static string effect = "EmotionBuff_01";
        static string buffDetail = "TouchBuff_SKL_CHG";
        static float param1 = 0.0299999993f;
        static float param1Add = 0.00999999978f;

        static public byte[] GetNew(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            reader.BaseStream.Position = 0x30;
            int count = reader.ReadInt32();
            EndianBinaryWriter writer = new EndianBinaryWriter(new MemoryStream(), EndianType.LittleEndian);
            reader.BaseStream.Position = 0;
            byte[] header = new byte[0x34];
            reader.BaseStream.Read(header, 0, header.Length);
            writer.Write(header);
            for (int i = 0; i < count; i++)
            {
                writer.Write(buffId);
                writer.WriteAlignedString(effect);
                writer.WriteAlignedString(buffDetail);
                writer.Write(param1);
                writer.Write(0);
                writer.Write(0);
                writer.Write(param1Add);
                writer.Write(0);
                writer.Write(0);
            }
            byte[] output = new byte[writer.BaseStream.Length];
            writer.BaseStream.Position = 0;
            writer.BaseStream.Read(output, 0, output.Length);
            return output;
        }
    }

    /// <summary>
    /// unity TextAsset
    /// </summary>
    class Textasset
    {
        public string name;
        public string text;
        public string m_pathname;
        /// <summary>
        ///  TextAsset
        /// </summary>
        /// <param name="data"></param>
        public Textasset(byte[] data)
        {
            EndianBinaryReader reader = new EndianBinaryReader(new MemoryStream(data), EndianType.LittleEndian);
            name = reader.ReadAlignedString();
            text = reader.ReadAlignedString();
            m_pathname = reader.ReadAlignedString();
        }
        /// <summary>
        /// TextAsset to byte[]
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            EndianBinaryWriter writer = new EndianBinaryWriter(new MemoryStream(), EndianType.LittleEndian);
            writer.WriteAlignedString(name);
            writer.WriteAlignedString(text);
            writer.WriteAlignedString(m_pathname);
            writer.Position = 0;
            byte[] data = new byte[writer.BaseStream.Length];
            writer.BaseStream.Read(data, 0, data.Length);
            return data;
        }
    }

    /// <summary>
    /// serverinfo
    /// </summary>
    public class Server_modinfo
    {
        /// <summary>
        /// Server
        /// </summary>
        public Gameserver server;
        /// <summary>
        /// Enable?
        /// </summary>
        public bool Enabletouch;
        public string gameserver_url;
        public string gameserver_rsp;
        public string server_ip;
        public string getway_ip;
        public string oa_ip;
        /// <summary>
        /// modified response
        /// </summary>
        public string mod_gameserver_rsp;

        public Server_modinfo(Gameserver sser)
        {
            server = sser;
        }
    }

    /// <summary>
    /// Server
    /// </summary>
    public enum Gameserver
    {
        android01,
        ios01,
        hun01,
        hun02,
        bb01,
        yyb01
    }


}
