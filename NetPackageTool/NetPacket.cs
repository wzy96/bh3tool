using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using ProtoBuf;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace NetPackageTool
{
    static class NetPacket
    {
        static public void ExportFromHexDump(string hexdumpPath, string cmdidPath)
        {
            Dictionary<int, string> cmdid = new Dictionary<int, string>();
            if (File.Exists(cmdidPath))
            {
                string[] cmdidstr = File.ReadAllLines(cmdidPath);
                foreach (string cmdidline in cmdidstr)
                {
                    string[] s = cmdidline.Split(' ');
                    cmdid.Add(Convert.ToInt32(s[0]), s[1]);
                }
            }

            List<MemoryStream> block = new List<MemoryStream>();

            FileStream file = new FileStream(hexdumpPath, FileMode.Open);
            StreamReader reader = new StreamReader(file);

            string line;
            bool firstCharIsSpace = false;
            MemoryStream ms = new MemoryStream();

            while ((line = reader.ReadLine()) != null)
            {
                if ((line.First() == ' ') != firstCharIsSpace)
                {
                    if (ms.Length > 0) block.Add(ms);

                    firstCharIsSpace = (line.First() == ' ');
                    ms = new MemoryStream();
                }

                byte[] bytes = HexDump2bytes(line);
                ms.Write(bytes, 0, bytes.Length);
            }
            block[0].Position = 0;
            block[1].Position = 0;
            //combine block
            for (int i = block.Count - 1; i > 1; i--)
            {
                var stream = block[i];
                if (stream == null) continue;
                stream.Position = 0;
                byte[] buffer = stream.GetBuffer();
                if (buffer[0] == 0x01 && buffer[1] == 0x23 && buffer[2] == 0x45 & buffer[3] == 0x67) continue;
                stream.WriteTo(block[i - 2]);
                block[i] = null;
            }

            StringBuilder strb = new StringBuilder();
            int packetcont = 0;
            for (int i = 0; i < block.Count; i++)
            {
                if (block[i] == null) continue;
                BinaryReader breader = new BinaryReader(block[i]);
                //read NetPacketV1 data
                while (breader.BaseStream.Position < breader.BaseStream.Length)
                {

                    uint head_magic_ = breader.ReadUIntBig();
                    ushort packet_version_ = breader.ReadUshortBig();
                    ushort client_version_ = breader.ReadUshortBig();
                    uint time_ = breader.ReadUIntBig();
                    uint user_id_ = breader.ReadUIntBig();
                    uint user_ip_ = breader.ReadUIntBig();
                    uint user_session_id_ = breader.ReadUIntBig();
                    uint gateway_ip_ = breader.ReadUIntBig();
                    ushort cmd_id_ = breader.ReadUshortBig();
                    uint body_len_ = breader.ReadUIntBig();
                    ushort sign_type_ = breader.ReadUshortBig();
                    uint sign_ = breader.ReadUIntBig();
                    byte[] body_ = breader.ReadBytes(body_len_);
                    uint tail_magic_ = breader.ReadUIntBig();

                    //to string
                    string cmdclass = cmdid.ContainsKey(cmd_id_) ? cmdid[cmd_id_] : "ERROR!";
                    strb.AppendLine("[packet] " + packetcont + " cmd_id = " + cmd_id_ + " " + cmdclass);

                    string jsonStr = ProtoToJson(body_).ToString(Formatting.Indented);
                    strb.AppendLine(jsonStr);

                    packetcont++;
                }
            }

            FileDialog fileDialog = new SaveFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
                File.WriteAllText(fileDialog.FileName, strb.ToString());

        }

        private static byte[] HexDump2bytes(string hex)
        {
            int offset = hex.StartsWith(" ") ? 4 : 0;
            string byteStr = hex.Substring(offset + 10, 48).Replace(" ", string.Empty);

            int len = byteStr.Length / 2;
            byte[] data = new byte[len];
            for (int i = 0; i < len; i++)
            {
                data[i] = (byte)Convert.ToInt32(byteStr.Substring(i * 2, 2), 16);
            }
            return data;
        }

        static public JObject ProtoToJson(byte[] protodata)
        {
            Stream stream = new MemoryStream(protodata);
            ProtoReader s = ProtoReader.Create(stream, stream.Length);
            Regex regex = new Regex("[\u0000-\u0009\u000e-\u001f]");
            JObject jObject = new JObject();

            for (; s.Position < protodata.Length;)
            {
                int fieldNumber = s.ReadFieldHeader();
                string name = "field_" + fieldNumber;

                switch (s.WireType)
                {
                    case WireType.Variant:
                    case WireType.Fixed32:
                    case WireType.Fixed64:
                    case WireType.SignedVariant:
                        {
                            string data = "0x" + s.ReadInt64().ToString("X");
                            jObject.AddEx(name, data);
                            break;
                        }
                    case WireType.String:
                        {
                            byte[] b = s.ReadStringBytes();
                            string readstr = "";
                            if (b != null) readstr = Encoding.UTF8.GetString(b);

                            if (b != null && regex.IsMatch(readstr))
                            {
                                try
                                {
                                    jObject.AddEx(name, ProtoToJson(b));
                                }
                                catch
                                {
                                    jObject.AddEx(name, readstr);
                                }
                            }
                            else
                            {
                                jObject.AddEx(name, readstr);
                            }
                            break;
                        }
                    case WireType.StartGroup:
                    //@string.Append('\t', depth + 1); @string.Append("Start Group\n"); break;
                    case WireType.EndGroup:
                    //@string.Append('\t', depth + 1); @string.Append("End Group\n"); break;
                    default: break;
                }
            }
            return jObject;
        }

    }
    static public class JObjectExtention
    {
        public static void AddEx(this JObject jObject, string name, string data)
        {
            if (jObject.ContainsKey(name))
            {
                if (jObject[name].Type != JTokenType.Array)
                {
                    JArray jArray = new JArray();
                    jArray.Add(jObject[name]);
                    jArray.Add(data);
                    jObject[name] = jArray;
                }
                else
                {
                    ((JArray)jObject[name]).Add(data);
                }
            }
            else
            {
                jObject.Add(name, data);
            }
        }
        public static void AddEx(this JObject jObject, string name, JObject data)
        {
            if (jObject.ContainsKey(name))
            {
                if (jObject[name].Type != JTokenType.Array)
                {
                    JArray jArray = new JArray();
                    jArray.Add(jObject[name]);
                    jArray.Add(data);
                    jObject[name] = jArray;
                }
                else
                {
                    ((JArray)jObject[name]).Add(data);
                }
            }
            else
            {
                jObject.Add(name, data);
            }
        }
    }
    static class FileStreamExtention
    {
        public static uint ReadUIntBig(this BinaryReader file)
        {
            byte[] t = new byte[4];
            if (file.Read(t, 0, 4) != 4)
                return 0;
            Array.Reverse(t);
            return BitConverter.ToUInt32(t, 0);
        }

        public static ushort ReadUshortBig(this BinaryReader file)
        {
            byte[] t = new byte[2];
            if (file.Read(t, 0, 2) != 2)
                return 0;
            Array.Reverse(t);
            return BitConverter.ToUInt16(t, 0);
        }

        public static byte[] ReadBytes(this BinaryReader file, uint length)
        {
            byte[] data = new byte[length];
            file.Read(data, 0, data.Length);
            return data;
        }
    }
}
