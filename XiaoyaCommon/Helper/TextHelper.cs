using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCommon.Helper
{
    public class TextHelper
    {
        public static String ToDBC(String input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == '　')
                {
                    c[i] = ' ';
                }
                else if (c[i] == '。')
                {
                    c[i] = '.';
                }
                else if (c[i] > 65280 && c[i] < 65375)
                {
                    c[i] = (char)(c[i] - 65248);
                }
            }
            return new String(c);
        }
    }
}
