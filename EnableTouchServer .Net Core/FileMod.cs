using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UABT;
using unluac.decompile;
using unluac.parse;
using unluacNet;

namespace bh3tool
{
    class FileMod
    {
        public byte[] _data;
        public byte[] _setting;
        public byte[] _excel;
        public DateTime time;

        /// <summary>
        /// download and modify file
        /// </summary>
        /// <param name="isIOS">Is ios Server</param>
        /// <param name="oldtime">Local File Last Modified Time</param>
        /// <returns></returns>
        public bool ModFile(bool isIOS, DateTime oldtime)
        {
            if (isIOS)
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] Check update :IOS");
            else
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] Check update :Android");
            //file url
            string baseurl = isIOS ? "https://bundle.bh3.com/asset_bundle/ios01/1.0" : "https://bundle.bh3.com/asset_bundle/android01/1.0";
            string settingurl = isIOS ? "/data/iphone_compressed/data/setting_" : "/data/android_compressed/data/setting_";
            string excelurl = isIOS ? "/data/iphone_compressed/data/excel_output_" : "/data/android_compressed/data/excel_output_";
            string dataurl = isIOS ? "/data/iphone_compressed/DataVersion.unity3d" : "/data/android_compressed/DataVersion.unity3d";

            string RSAkey = @"<RSAKeyValue><Modulus>rXoKWm82JSX4UYihkt2FSjrp3pZqTxt6AyJ0ZexHssStYesCFuUOmDBrk0nxPTY2r7oB4ZC9tDhHzmA66Me56wkD47Z3fCEBfLFxmEVdUCvM1RIFdQxQCB7CMaFWXHoVfBhNcD60OtXD71vFusBLioa6HDHbKk8LdgWdV10OWaE=</Modulus><Exponent>EQ==</Exponent><P>16GiwrgCGvcYbgSZOBJRx4G9kioGgexLSyW62iK4EuT0Xu9xyflBDaC4yooFkxrflqEAIiEfTqNGlYeJks+5qw==</P><Q>zfQY4dWi/Dlo38y6xvX4pUEAj1hbeFo/Qiy7H00P089W0KC6Mdi+GY4UuRGJtgX7UZfGQdHRj8mBjijFyhUl4w==</Q><DP>cihlOejyDkaUdnrntEXvD0Svp7vlU9dzJ8iuNz+OoJdUMkKHiQt8yvq8Lv3Gt0p2Xs20xsY9wDhSi2Xfa9diSw==</DP><DQ>GDrVwDdAWeii7SclCFksT61LXCiDO1XpUxRSP+ryzZ/sGIthMwpwt7ZcynqIrAC0J7eAvHMJmHIPPeat24oEdQ==</DQ><InverseQ>P4/vgq1XF77N8K/OxTbcjWFCC1d+v3W5xWQJbmU3KfVF2wOStZeILT2X12s7AHD+uUfN9O/xdEBIeqcSLVxWjw==</InverseQ><D>o0WvZCxvMgWeatrybBvIvlWQ0X6CLFYYe2u42GXpILkbp3PFuzHvnkuwip/yG35RllS2efGjfHE0hgA3cazrNgM6gBDcFa7iznviIiQTySxFuzy3mXpjSQFaGgdvmuUQLgg5qahcdGgT455Fzo5GSu+IyTpD+dNoKy79NLTbvjE=</D></RSAKeyValue>";

            WebClient client = new WebClient();
            //Download DataVersion.unity3d.
            byte[] dataversion = client.DownloadData(baseurl + dataurl);
            string[] key = client.ResponseHeaders.AllKeys;
            //Check Last-Modified time.
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == "Last-Modified")
                {
                    time = DateTime.Parse(client.ResponseHeaders.Get(i));
                }
            }

            if (DateTime.Compare(oldtime, time) >= 0)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] No update");
                return false;
            }

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] Start update");

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] edit dataversion.unity3d");

            //Decrypt DataVersion.unity3d
            for (int i = 0; i < dataversion.Length; i++) dataversion[i] = (byte)(dataversion[i] ^ 0xA5);
            //load bundle asset 
            BundleFile dataversionBundle = new BundleFile(dataversion);
            SerializedFile dataversionFile = new SerializedFile("dataversion", new EndianBinaryReader(dataversionBundle.fileList[0].stream));
            //get textasset bytes
            long Pid = dataversionFile.GetPathIDByName("packageversion.txt");
            var packageversion = dataversionFile.m_Objects.Find(x => x.m_PathID == Pid);
            //load textasset
            Textasset textasset = new Textasset(packageversion.data);

            string[] versiontext = textasset.text.Split('\n');
            //Get AES key IV and HMACSHA1 key
            var rsa = RSA.Create();
            rsa.FromXmlStringA(RSAkey);
            byte[] AES_SHA = rsa.Decrypt(Hex2bytes(versiontext[0]), RSAEncryptionPadding.Pkcs1);
            //AES_SHA 56 bytes, AES key 32 bytes|AES IV 16 bytes|HMACSHA1 8 bytes 
            byte[] AESkey = AES_SHA.Take(32).ToArray();
            byte[] AESIV = AES_SHA.Skip(32).Take(16).ToArray();
            byte[] SHA = AES_SHA.Skip(48).Take(8).ToArray();

            Regex regexCS = new Regex("(CS\":\")([1-9]\\d*)");
            Regex regexCRC = new Regex("(CRC\":\")([0-9A-Z]*)");

            string settingCRC;
            string excelCRC;
            int settingindex;
            int excelindex;
            //Get File CRC 
            if (versiontext[1].Contains("excel_output"))
            {
                excelindex = 1;
                settingindex = 2;
            }
            else
            {
                excelindex = 2;
                settingindex = 1;
            }
            excelCRC = regexCRC.Match(versiontext[excelindex]).Groups[2].Value;
            settingCRC = regexCRC.Match(versiontext[settingindex]).Groups[2].Value;


            //Download setting.unity3d and excel_output.unity3d
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] download setting");
            byte[] settingbytes = client.DownloadData(baseurl + settingurl + settingCRC);
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] download excel_output");
            byte[] excelbytes = client.DownloadData(baseurl + excelurl + excelCRC);
            client.Dispose();

            //Decrypt File
            Aes Aes = Aes.Create();
            var AES_Encryptor = Aes.CreateEncryptor(AESkey, AESIV);

            var AES_Decryptor = Aes.CreateDecryptor(AESkey, AESIV);
            settingbytes = AES_Decryptor.TransformFinalBlock(settingbytes, 0, settingbytes.Length);

            AES_Decryptor = Aes.CreateDecryptor(AESkey, AESIV);
            excelbytes = AES_Decryptor.TransformFinalBlock(excelbytes, 0, excelbytes.Length);

            #region Modify setting.unity3d

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] edit setting");

            BundleFile settingBundle = new BundleFile(settingbytes);
            SerializedFile setting = new SerializedFile("setting", new EndianBinaryReader(settingBundle.fileList[0].stream));

            #region Edit miscdata.txt
            //Get miscdata.txt from bundle
            long miscid = setting.GetPathIDByName("miscdata.txt");
            var misc = setting.m_Objects.Find(x => x.m_PathID == miscid);
            Textasset miscasset = new Textasset(misc.data);
            //Enable All BodyPort Touch
            Regex regexMISC = new Regex("(Face|Chest|Private|Arm|Leg)(\" *\t*: )(false)");
            miscasset.text = regexMISC.Replace(miscasset.text, "$1$2true");
            misc.data = miscasset.GetBytes();
            #endregion

            #region Edit uiluadesign_x_x.lua.txt/.bytes
            bool isLuac = false;
            //version number
            long uiluaid = setting.GetPathIDByName("uiluadesign_3_4.lua.txt");
            if (uiluaid == -1)
            {
                isLuac = true;
                uiluaid = setting.GetPathIDByName("uiluadesign_3_4.lua.bytes");
            }
            var uilua = setting.m_Objects.Find(x => x.m_PathID == uiluaid);
            Textasset uiluaasset = new Textasset(uilua.data);
            List<string> list;
            if (isLuac)
                //Decompile luac to lua string
                list = DecompileLuac(uiluaasset.bytes);
            else
                list = new List<string>(uiluaasset.text.Split('\n'));

            //insert Lua code to function UITable.ModuleEndHandlePacket 
            int index = list.FindIndex(x => x.Contains("UITable.ModuleEndHandlePacket"));
            //Handle NetPacketV1 for GalTouchModule,and set GalTouchModule._canGalTouch to true
            string insertStr = "\tif moduleName == \"GalTouchModule\" and packet:getCmdId() == 111 then\n\t\tlocal gal = __singletonManagerType.GetSingletonInstance(\"MoleMole.GalTouchModule\")\n\t\tif gal == nil then\n\t\t\treturn\n\t\tend\n\t\tgal._canGalTouch = true\n\tend\n";
            list.Insert(index + 1, insertStr);
            uiluaasset.text = String.Join('\n', list);
            uilua.data = uiluaasset.GetBytes();
            #endregion

            #region Edit Assetbundle
            if (isLuac)
            {
                var ab = setting.m_Objects.Find(x => x.m_PathID == 1);
                AssetBundle bundle = new AssetBundle(ab.data);
                bundle.Rename("uiluadesign_3_4.lua.bytes", "uiluadesign_3_4.lua.txt");
                ab.data = bundle.GetBytes();
            }
            #endregion

            #region Edit luahackconfig.txt
            //Add "GalTouchModule" to "UILuaPatchModuleList"
            long luahackpid = setting.GetPathIDByName("luahackconfig.txt");
            var luahack = setting.m_Objects.Find(x => x.m_PathID == luahackpid);
            Textasset hack = new Textasset(luahack.data);
            string inserthack = "		\"GalTouchModule\",";
            list = new List<string>(hack.text.Split('\n'));
            index = list.FindIndex(x => x.Contains("UILuaPatchModuleList"));
            if (list[index].Contains('['))
                list[index] = list[index].Insert(list[index].IndexOf('[') + 1, inserthack);
            else
                list.Insert(index + 2, inserthack);
            hack.text = string.Join('\n', list);
            luahack.data = hack.GetBytes();
            #endregion

            //pack to assetbundle
            misc.data = miscasset.GetBytes();
            settingBundle.fileList[0].stream = new MemoryStream(setting.Pack());
            settingbytes = settingBundle.RePack();
            //ENCRYPT
            settingbytes = AES_Encryptor.TransformFinalBlock(settingbytes, 0, settingbytes.Length);
            //save file
            _setting = settingbytes;
            #endregion

            #region Edit excel_output.unity3d
            //
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] edit excel");

            BundleFile excelBundle = new BundleFile(excelbytes);
            SerializedFile excel = new SerializedFile("excel", new EndianBinaryReader(excelBundle.fileList[0].stream));
            //Edit touch buff
            long touchbuffid = excel.GetPathIDByName("touchbuffdata.asset");
            var bf = excel.m_Objects.Find(x => x.m_PathID == touchbuffid);
            bf.data = TouchBuff.GetNew(bf.data);
            excelBundle.fileList[0].stream = new MemoryStream(excel.Pack());
            excelbytes = excelBundle.RePack();
            //Encrypt excel_output.unity3d
            AES_Encryptor = Aes.CreateEncryptor(AESkey, AESIV);
            excelbytes = AES_Encryptor.TransformFinalBlock(excelbytes, 0, excelbytes.Length);
            _excel = excelbytes;
            #endregion

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] edit dataversion");

            //compute new CRC(hmac-SHA1)
            HMACSHA1 hMACSHA1 = new HMACSHA1(SHA);
            settingCRC = BitConverter.ToString(hMACSHA1.ComputeHash(settingbytes)).Replace("-", "");
            excelCRC = BitConverter.ToString(hMACSHA1.ComputeHash(excelbytes)).Replace("-", "");

            //Replace fileSize("CS") and SHA1("CRC")
            versiontext[excelindex] = regexCRC.Replace(versiontext[1], "CRC\":\"" + excelCRC);
            versiontext[excelindex] = regexCS.Replace(versiontext[1], "CS\":\"" + excelbytes.Length.ToString());
            versiontext[settingindex] = regexCRC.Replace(versiontext[2], "CRC\":\"" + settingCRC);
            versiontext[settingindex] = regexCS.Replace(versiontext[2], "CS\":\"" + settingbytes.Length.ToString());

            //string[] to string
            textasset.text = string.Join("\n", versiontext);
            //get textasset bytes
            packageversion.data = textasset.GetBytes();
            //pack
            dataversionBundle.fileList[0].stream = new MemoryStream(dataversionFile.Pack());
            byte[] dz = dataversionBundle.RePack();
            //encrypt dataversion.unity3d
            for (int i = 0; i < dz.Length; i++) dz[i] = (byte)(dz[i] ^ 0xA5);
            //save file
            _data = dz;
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] Update file success");
            return true;
        }

        /// <summary>
        /// HEX string to byte[]
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private static byte[] Hex2bytes(string hex)
        {
            int ver = hex.Length / 2;
            byte[] data = new byte[ver];
            for (int i = 0; i < ver; i++)
            {
                data[i] = (byte)Convert.ToInt32(hex.Substring(i * 2, 2), 16);
            }
            return data;
        }
        static int[] Opindex = { 0x09,0x22,0x02,0x1a,0x12,0x00,0x21,0x24,
                              0x1e,0x11,0x08,0x0f,0x0c,0x17,0x03,0x23,
                              0x0e,0x0a,0x1c,0x16,0x13,0x0b,0x04,0x0d,
                              0x06,0x14,0x01,0x18,0x20,0x10,0x05,0x25,
                              0x15,0x1d,0x1b,0x19,0x07,0x1f
                            };
        private List<string> DecompileLuac(byte[] data)
        {
            data[5] = 0x00;
            ByteBuffer buffer = new ByteBuffer(data);
            var header = new BHeader(buffer);
            LFunction lmain = header.function.parse(buffer, header);
            decryptcode(lmain);
            Decompiler d = new Decompiler(lmain);
            d.decompile();
            MemoryStream outs = new MemoryStream();
            StreamWriter writer = new StreamWriter(outs);
            d.print(new Output(writer.Write, writer.WriteLine));
            var text = Encoding.UTF8.GetString(outs.ToArray());
            return new List<string>(text.Split('\n'));
        }

        static void decryptcode(LFunction function)
        {
            int[] code = function.code;
            for (int i = 0; i < code.Length; i++)
            {
                var opcode = code[i] & 0x3f;
                code[i] &= ~0x3f;
                code[i] |= Opindex[opcode];
            }
            foreach (var f in function.functions)
                decryptcode(f);
        }

    }
}
