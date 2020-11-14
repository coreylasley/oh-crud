using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codeterpret.Common
{
    /// <summary>
    /// Used for code generation for coloring syntax when code is displayed in the browser.
    /// The Color() method will wrap a block of text with characters that can be converted to HTML
    /// By using the static method CodeColor.RenderWithColor() with text that contains the color coding characters, the string will be converted to HTML
    /// By using the static method CodeColor.RenderWithNonColor() with text contains any number of color coding characters, the string will be stripped of color coding characters, and returned as plain text
    /// The reason HTML isnt simply inserted via the Color() method is so that the text can be stripped of appended coloring code much easier.
    /// </summary>
    public class CodeColoring
    {
        public enum ColorPalettes
        {
            None,
            CSharp_VSDark,
            TypeScript_VSDark
        }

        public enum ColorTypes
        {
            Default,
            Background,
            MethodName,
            MethodCall,
            ClassName,
            InterfaceName,
            Type,
            PrimitiveType,
            Operator,
            Flow,
            Enum,
            Parameter,
            Property,
            String,
            Comment,
            Accessibility
        }

        public static string LessThanAlternate = "^&^";
        public static string GreateThanAlternate = "^*^";

        private Dictionary<ColorTypes, string> ColorCodes { get; set; }

        public CodeColoring(Dictionary<ColorTypes, string> customColors)
        {
            ColorCodes = customColors;
        }

        public CodeColoring(ColorPalettes codeType = ColorPalettes.None)
        {
            switch (codeType)
            {
                case ColorPalettes.None:
                    ColorCodes = new Dictionary<ColorTypes, string>();
                    ColorCodes.Add(ColorTypes.Default, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.Background, "white");
                    ColorCodes.Add(ColorTypes.MethodName, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.MethodCall, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.ClassName, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.InterfaceName, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.Type, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.PrimitiveType, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.Operator, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.Flow, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.Enum, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.Parameter, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.Property, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.String, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.Accessibility, "#1e1e1e");
                    break;

                case ColorPalettes.CSharp_VSDark:
                    // Defaults based on C# / Visual Studio / Dark Mode
                    ColorCodes = new Dictionary<ColorTypes, string>();
                    ColorCodes.Add(ColorTypes.Default, "#dcdcdc");
                    ColorCodes.Add(ColorTypes.Background, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.MethodName, "#4ec9b0");
                    ColorCodes.Add(ColorTypes.MethodCall, "#dcdcaa");
                    ColorCodes.Add(ColorTypes.ClassName, "#4ec9b0");
                    ColorCodes.Add(ColorTypes.InterfaceName, "#b8d7a3");
                    ColorCodes.Add(ColorTypes.Type, "#4ec9b0");
                    ColorCodes.Add(ColorTypes.PrimitiveType, "#559ad3");
                    ColorCodes.Add(ColorTypes.Operator, "#dcdcdc");
                    ColorCodes.Add(ColorTypes.Flow, "#d8a0df");
                    ColorCodes.Add(ColorTypes.Enum, "#b8d7a3");
                    ColorCodes.Add(ColorTypes.Parameter, "#9cdcfe");
                    ColorCodes.Add(ColorTypes.Property, "#dcdcdc");
                    ColorCodes.Add(ColorTypes.String, "#d69d85");
                    ColorCodes.Add(ColorTypes.Comment, "#608b4e");
                    ColorCodes.Add(ColorTypes.Accessibility, "#559ad3");
                    break;

                case ColorPalettes.TypeScript_VSDark:
                    // Defaults based on C# / Visual Studio / Dark Mode
                    ColorCodes = new Dictionary<ColorTypes, string>();
                    ColorCodes.Add(ColorTypes.Default, "#dcdcdc");
                    ColorCodes.Add(ColorTypes.Background, "#1e1e1e");
                    ColorCodes.Add(ColorTypes.MethodName, "#4ec9b0");
                    ColorCodes.Add(ColorTypes.MethodCall, "#dcdcaa");
                    ColorCodes.Add(ColorTypes.ClassName, "#4ec9b0");
                    ColorCodes.Add(ColorTypes.InterfaceName, "#b8d7a3");
                    ColorCodes.Add(ColorTypes.Type, "#4ec9b0");
                    ColorCodes.Add(ColorTypes.PrimitiveType, "#559ad3");
                    ColorCodes.Add(ColorTypes.Operator, "#dcdcdc");
                    ColorCodes.Add(ColorTypes.Flow, "#d8a0df");
                    ColorCodes.Add(ColorTypes.Enum, "#b8d7a3");
                    ColorCodes.Add(ColorTypes.Parameter, "#9cdcfe");
                    ColorCodes.Add(ColorTypes.Property, "#dcdcdc");
                    ColorCodes.Add(ColorTypes.String, "#d69d85");
                    ColorCodes.Add(ColorTypes.Comment, "#608b4e");
                    ColorCodes.Add(ColorTypes.Accessibility, "#559ad3");
                    break;
            }
        }

        /// <summary>
        /// Used to add characters around text that will be converted to HTML when using 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="colorType"></param>
        /// <returns></returns>
        public string Color(string text, ColorTypes colorType)
        {
            return $"&@@@span style=\"color:{GetColor(colorType)}\"@@@&{text}*/@@@span@@@*";
        }

        /// <summary>
        /// Gets the assigned color value for the specified color type 
        /// </summary>
        /// <param name="colorType"></param>
        /// <returns></returns>
        public string GetColor(ColorTypes colorType)
        {
            return ColorCodes.FirstOrDefault(x => x.Key == colorType).Value;
        }

        /// <summary>
        /// Used to wrap text with colorizing characters which can convert to HTML for display in a browser when using CodeColor.RenderWithColor()
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string RenderWithColor(string text)
        {
            //string css = "";

            string css = "<style>"
                         + ".code-render {"
                         + "    counter-reset: linenumber;"
                         + "    padding: 15px;"
                         + "    height: 500px;"
                         + "    overflow-y: scroll;"
                         + "    color: silver;"
                         + "    resize: vertical;"
                         + $"    background-color: {GetColor(ColorTypes.Background)};"
                         + "}\n";
            /*
                         + ".code-render > .code-l:not(:only-child)::before {"
                         + "    content: counter(linenumber, decimal-leading-zero) \"     \";"
                         + "    counter-increment: linenumber;"
                         + "    color:#2b90af;"
                         + "}"
            */
                         css += "</style>";
            

            text = text.Replace("@@@&", ">").Replace("@@@*", ">").Replace("&@@@", "<").Replace("*/@@@", "</");
            string final = text;
            
            /*
            string[] lines = text.Split('\n');
            foreach(string l in lines)
            {
                final += $"<code class=\"code-l\">{l}</code>\n";
            }
            */
            text = $"{css}<pre class=\"code-render\"><span style=\"color:{GetColor(ColorTypes.Default)}\">\n{final}</span></pre>";

            text = text.Replace(CodeColoring.GreateThanAlternate, "&gt;").Replace(CodeColoring.LessThanAlternate, "&lt;");

            return text;
        }

        /// <summary>
        /// Renders the text without any of the color coding
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string RenderWithNoColor(string text)
        {
            string startTag = "&@@@span style=\"color:";
            string endTag = "@@@&";
            text = text.Replace("*/@@@span@@@*", "");

            int sPos = 0;
            int ePos = 0;
            while (sPos > -1)
            {
                sPos = text.IndexOf(startTag);
                if (sPos > -1)
                {
                    ePos = text.IndexOf(endTag, sPos);
                    if (ePos > -1)
                    {
                        string remove = text.Substring(sPos, (ePos - sPos) + endTag.Length);
                        text = text.Replace(remove, "");
                    }
                }
            }

            text = text.Replace(CodeColoring.GreateThanAlternate, ">").Replace(CodeColoring.LessThanAlternate, "<");

            return text;
        }
    }
}
