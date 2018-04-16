using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaNLP.Helper
{
    public class TextHelper
    {
        public static string ReplaceSpaces(string input, string replacement = "\n")
        {
            return CommonRegex.Trimmer.Replace(input.Trim(), replacement);
        }

        public static string FullWidthCharToHalfWidthChar(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                switch(c[i])
                {
                    case '　':
                        c[i] = ' ';
                        break;
                    case '。':
                        c[i] = '.';
                        break;
                    case '，':
                    case '、':
                    case '､':
                    case '〟':
                    case '„':
                        c[i] = ',';
                        break;
                    case '？':
                        c[i] = '?';
                        break;
                    case '！':
                        c[i] = '!';
                        break;
                    case '“':
                    case '‟':
                    case '”':
                    case '〃':
                    case '〝':
                    case '〞':
                        c[i] = '"';
                        break;
                    case '‘':
                    case '’':
                    case '‛':
                        c[i] = '\'';
                        break;
                    case '＃':
                        c[i] = '#';
                        break;
                    case '＠':
                        c[i] = '@';
                        break;
                    case '＄':
                    case '￥':
                        c[i] = '$';
                        break;
                    case '％':
                        c[i] = '%';
                        break;
                    case '＾':
                        c[i] = '^';
                        break;
                    case '＆':
                        c[i] = '&';
                        break;
                    case '＊':
                        c[i] = '*';
                        break;
                    case '（':
                    case '｟':
                        c[i] = '(';
                        break;
                    case '）':
                    case '｠':
                        c[i] = ')';
                        break;
                    case '－':
                    case '–':
                    case '—':
                        c[i] = '-';
                        break;
                    case '＋':
                        c[i] = '+';
                        break;
                    case '／':
                        c[i] = '/';
                        break;
                    case '＼':
                        c[i] = '\\';
                        break;
                    case '｛':
                        c[i] = '{';
                        break;
                    case '｝':
                        c[i] = '}';
                        break;
                    case '［':
                        c[i] = '[';
                        break;
                    case '］':
                        c[i] = ']';
                        break;
                    case '：':
                        c[i] = ':';
                        break;
                    case '；':
                        c[i] = ';';
                        break;
                    case '＜':
                    case '《':
                    case '｢':
                    case '「':
                    case '『':
                    case '【':
                    case '〔':
                    case '〖':
                    case '〘':
                    case '〚':
                        c[i] = '<';
                        break;
                    case '＞':
                    case '》':
                    case '｣':
                    case '」':
                    case '』':
                    case '】':
                    case '〕':
                    case '〗':
                    case '〙':
                    case '〛':
                        c[i] = '>';
                        break;
                    case '｜':
                        c[i] = '|';
                        break;
                    case '＿':
                        c[i] = '_';
                        break;
                    case '＝':
                        c[i] = '=';
                        break;
                    case '～':
                    case '〜':
                    case '〰':
                        c[i] = '~';
                        break;
                    case '…':
                    case '〾':
                    case '‧':
                    case '﹏':
                    case '.':
                        c[i] = '.';
                        break;
                }
            }
            return new String(c);
        }
    }
}
