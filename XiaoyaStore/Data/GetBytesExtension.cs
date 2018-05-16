using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data
{
    public static class GetBytesExtension
    {
        public static int GetInt(this byte[] data)
        {
            return BitConverter.ToInt32(data, 0);
        }

        public static long GetLong(this byte[] data)
        {
            return BitConverter.ToInt64(data, 0);
        }

        public static string GetString(this byte[] data, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return encoding.GetString(data);
        }

        public static byte[] GetBytes(this string str, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return encoding.GetBytes(str);
        }

        public static byte[] GetBytes(this int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(this long value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}
