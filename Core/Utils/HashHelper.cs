using System.Security.Cryptography;
using System.Text;

namespace BackendJX3D.Core.Utils
{
    public static class HashHelper
    {
        public static string ToMd5(string input)
        {
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
        
        public static uint FileNameHash(string? pString)
        {
            if (string.IsNullOrEmpty(pString))
                return 0x12345678;

            uint id = 0;

            for (int i = 0; i < pString.Length; i++)
            {
                uint c = pString[i];

                if (c >= 'A' && c <= 'Z')
                    c += 0x20;
                else if (c == '/')
                    c = '\\';

                id = (id + (uint)((i + 1) * c)) % 0x8000000B;

                unchecked
                {
                    id *= 0xFFFFFFEF;
                }
            }

            return id ^ 0x12345678;
        }
    }
}
