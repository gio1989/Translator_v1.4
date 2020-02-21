using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Translator.Classes
{
    public class HelperMethods
    {
        private static readonly byte[] _k = { 107, 101, 114, 111, 107, 101, 121, 107, 101, 114, 111, 107, 101, 121, 107, 101, 114, 111, 107, 101, 121, 107, 101, 121 };
        private static readonly byte[] _v = { 107, 101, 114, 111, 118, 101, 99, 116, 111, 114 };

        public static LoginHelperObject LoadLoginHelperObjectJson()
        {
            var path = Path.GetFullPath(@"loginHelper.json");
            if (File.Exists(path))
            {
                using (var r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<LoginHelperObject>(json);
                    result.Password = Decrypt(Convert.FromBase64String(result.Password));
                    return result;
                }
            }
            return null;
        }

        public static void WriteLoginHelperObjectJson(LoginHelperObject loginHelperObject)
        {
            if (loginHelperObject == null) return;

            if (!string.IsNullOrEmpty(loginHelperObject.Password))
                loginHelperObject.Password = Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(loginHelperObject.Password)));

            var path = Path.GetFullPath(@"loginHelper.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, string.Empty);

            string json = JsonConvert.SerializeObject(loginHelperObject, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static byte[] Encrypt(byte[] value)
        {
            var trippleDes = TripleDES.Create();
            trippleDes.Mode = CipherMode.CBC;

            var encryptor = trippleDes.CreateEncryptor(_k, _v);
            byte[] enc = encryptor.TransformFinalBlock(value, 0, value.Length);

            return enc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Decrypt(byte[] value)
        {
            if (value.Length == 0) return "";
            var trippleDes = TripleDES.Create();
            trippleDes.Mode = CipherMode.CBC;

            var decryptor = trippleDes.CreateDecryptor(_k, _v);
            byte[] decr = decryptor.TransformFinalBlock(value, 0, value.Length);
            var decryptedValue = Encoding.UTF8.GetString(decr);

            return decryptedValue;
        }
    }
}
