using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Codeterpret
{
    public static class Extensions
    {
        
        /// <summary>
        /// Indents all lines in a string with the [indentChar] X [indentCount]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="indentChar"></param>
        /// <param name="indentCount"></param>
        /// <returns></returns>
        public static string Indent(this String str, char indentChar, int indentCount)
        {
            StringBuilder sb = new StringBuilder();
            string[] lines = str.Trim().Split('\n');
            string toAdd = "";

            for (int x = 0; x < indentCount; x++)
            {
                toAdd += indentChar;
            }

            foreach(string l in lines)
            {
                sb.AppendLine(toAdd + l.Replace("\r",""));
            }

            return sb.ToString();
        }

        public static string IncludeNamespace(this String str, string Namespace)
        {
            string ret = str;

            if (Namespace != "")
            {
                ret = $"namespace {Namespace}\n{{\n{str.Indent(' ', 4)}\n}}";
            }

            return ret;
        }

        /// <summary>
        /// Replaces every occurance of [replaceThis] with [withThis] between positions startIndex and stopIndex
        /// </summary>
        /// <param name="str"></param>
        /// <param name="replaceThis"></param>
        /// <param name="withThis"></param>
        /// <param name="startIndex"></param>
        /// <param name="stopIndex"></param>
        /// <returns></returns>
        public static string ReplaceBetween(this String str, char replaceThis, char withThis, int startIndex, int stopIndex)
        {
            StringBuilder ret = new StringBuilder();

            int index = -1;
            foreach(char c in str)
            {
                index++;
                if (c == replaceThis && index >= startIndex && index <= stopIndex)
                {
                    ret.Append(withThis);
                }
                else
                {
                    ret.Append(c);
                }
            }

            return ret.ToString();
        }

        /// <summary>
        /// Replaces all substrings seperated by | (Pipe) character with a string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pipeDelimitedStringToReplace">String seperated by | (Pipe) to all be replaced with withThis</param>
        /// <param name="withThis">String that you want to replace each substring with</param>
        /// <returns></returns>
        public static string ReplaceEach(this String str, string pipeDelimitedStringToReplace, string withThis)
        {
            string ret = str;
            string[] subs = pipeDelimitedStringToReplace.Split('|');
            foreach(string  s in subs)
            {
                ret = ret.Replace(s, withThis);
            }

            return ret;
        }

        /// <summary>
        /// Extracts a substring from a string that falls between the first occurance of [startChar] and [stopChar] begining at position [startIndex]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startChar"></param>
        /// <param name="stopChar"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string SubstringBetween(this String str, char startChar, char stopChar, int startIndex = 0)
        {
            StringBuilder ret = new StringBuilder();

            bool startCollecting = false;
            int index = -1;
            foreach (char c in str)
            {
                index++;
                if (index >= startIndex)
                {
                    if (c == stopChar) break;

                    if (startCollecting) ret.Append(c);

                    if (c == startChar) startCollecting = true;
                }
                               
            }

            return ret.ToString();
        }

        /// <summary>
        /// Appends a comma to a string prior to appending [toAppend] IF the string is not empty
        /// </summary>
        /// <param name="str"></param>
        /// <param name="toAppend"></param>
        /// <returns></returns>
        public static string CommaAppend(this String str, string toAppend)
        {
            if (str != null)
            {
                return (str.Trim() == "" ? "" : ", ") + toAppend;
            }
            else
            {
                return toAppend;
            }
        }

        /// <summary>
        /// Pads a string with [pad] if the string starts with [startsWith]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startsWith"></param>
        /// <param name="pad"></param>
        /// <returns></returns>
        public static string PadIfNotStartsWith(this String str, string startsWith, string pad)
        {
            if (str != null)
            {
                return (str.StartsWith(startsWith) == false ? pad + str : str);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Quick and Dirty way to count words in a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int WordCount(this String str)
        {
            if (str.Trim() != "")
            {
                return str.Trim().Count(x => x == ' ') + 1;
            }
            else
            {
                return 0;
            }
        }

        public static string WrapIfNotEmpty(this String str, string wrapString)
        {
            if (str != null)
            {
                return (str != "" ? wrapString + str + wrapString : "");
            }
            else
            {
                return "";
            }
        }

        public static string WrapIfNotEmpty(this String str, string wrapStringLeft, string wrapStringRight)
        {
            if (str != null)
            {
                return (str != "" ? wrapStringLeft + str + wrapStringRight : "");
            }
            else
            {
                return "";
            }
        }

        public static int ContainsHowMany(this String str, string ofWhat)
        {
            int p = str.IndexOf(ofWhat);
            int count = 0;
            while(p >= 0)
            {
                count++;
                p = str.IndexOf(ofWhat, p + 1);
            }

            return count;
        }

        public static string ToCommaList(this List<string> list)
        {
            string ret = "";
            foreach (string i in list)
            {
                if (ret != "") ret += ", ";
                ret += i;
            }

            return ret;
        }

        public static string ToLocalVariable(this string name)
        {
            if (name == "ID") name = "Id";

            string ret = name;

            if (name.Length > 0)
            {
                name = ret.Substring(0, 1).ToLower() + ret.Substring(1, ret.Length - 1);
                if (name != ret)
                    ret = name;
                else
                    ret = "_" + ret;
            }

            return ret;
        }

        public static string GetDescription(this Enum GenericEnum)
        {
            // Thanks to: https://www.codingame.com/playgrounds/2487/c---how-to-display-friendly-names-for-enumerations
            Type genericEnumType = GenericEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if ((_Attribs != null && _Attribs.Count() > 0))
                {
                    return ((DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return GenericEnum.ToString();
        }




    }
}
