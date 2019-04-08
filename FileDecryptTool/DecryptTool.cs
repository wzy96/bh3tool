using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FileDecryptTool
{
    class FileDecryptTool
    {
        static public void AB_Decrypt(string path)
        {
            byte[] AESkey = GetAESkey();
            if (AESkey == null) return;

            byte[] AES_Key = AESkey.Take(32).ToArray();
            byte[] AES_IV = AESkey.Skip(32).Take(16).ToArray();

            byte[] data = File.ReadAllBytes(path);

            byte[] Decrypt = AESDecrypt(data, AES_Key, AES_IV);
            if (Decrypt == null) return;

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.RestoreDirectory = true;
            saveFile.Filter = "AssetBundle(*.unity3d)|*.unity3d";
            if (saveFile.ShowDialog() == DialogResult.OK)
                File.WriteAllBytes(saveFile.FileName, Decrypt);
        }

        static public string Hash(byte[] data, byte[] key)
        {
            var hmac = new HMACSHA1(key);
            byte[] hash = hmac.ComputeHash(data);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            var hex = sb.ToString();
            return hex;
        }

        static public void AB_Encrypt(string path)
        {
            byte[] AESkey = GetAESkey();

            byte[] AES_Key = AESkey.Take(32).ToArray();
            byte[] AES_IV = AESkey.Skip(32).Take(16).ToArray();
            byte[] HMACkey = AESkey.Skip(48).ToArray();

            byte[] data = File.ReadAllBytes(path);

            byte[] Encrypt = AESEncrypt(data, AES_Key, AES_IV);
            if (Encrypt == null) return;

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.RestoreDirectory = true;
            saveFile.Filter = "AssetBundle(*.unity3d)|*.unity3d";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                string outpath = Path.GetDirectoryName(saveFile.FileName) + @"\\" + Path.GetFileNameWithoutExtension(saveFile.FileName) + "_" + Hash(Encrypt, HMACkey) + @".unity3d";
                File.WriteAllBytes(outpath, Encrypt);
            }

        }

        static byte[] GetAESkey()
        {
            string RSAKey = File.ReadAllText(Application.StartupPath + @"\rsakey.xml");
            byte[] AESkey = File.ReadAllBytes(Application.StartupPath + @"\RSA_Encrypted-AESkey");
            try
            {
                RSACryptoServiceProvider rSACrypto = new RSACryptoServiceProvider();
                rSACrypto.FromXmlString(RSAKey);
                byte[] DecryptAESKey = rSACrypto.Decrypt(AESkey, false);
                return DecryptAESKey;
            }
            catch
            {
                MessageBox.Show("RSA解密时出错!");
                return null;
            }
        }

        static byte[] AESDecrypt(byte[] data, byte[] AES_key, byte[] AES_IV)
        {
            Aes AES = Aes.Create();
            try
            {
                var Decryptor = AES.CreateDecryptor(AES_key, AES_IV);
                return Decryptor.TransformFinalBlock(data, 0, data.Length);
            }
            catch
            {
                MessageBox.Show("AES解密时出错!");
                return null;
            }
        }

        static byte[] AESEncrypt(byte[] data, byte[] AES_key, byte[] AES_IV)
        {
            Aes AES = Aes.Create();
            try
            {
                var Encryptor = AES.CreateEncryptor(AES_key, AES_IV);
                return Encryptor.TransformFinalBlock(data, 0, data.Length);
            }
            catch
            {
                MessageBox.Show("AES加密时出错!");
                return null;
            }
        }

        static public void Data_xor(string path)
        {
            byte[] file = File.ReadAllBytes(path);

            for (int i = 0; i < file.Length; i++) file[i] ^= 0xA5;

            path = path + "_xor";

            File.WriteAllBytes(path, file);
        }

    }
}
