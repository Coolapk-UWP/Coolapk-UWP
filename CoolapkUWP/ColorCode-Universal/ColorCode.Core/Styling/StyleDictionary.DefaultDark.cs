// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;

namespace ColorCode.Styling
{
    /// <summary>
    /// Defines the Default Dark Theme.
    /// </summary>
    public partial class StyleDictionary
    {
        private const string VSDarkBackground = "#FF1E1E1E";
        private const string VSDarkPlainText = "#FFDADADA";

        private const string VSDarkXMLDelimeter = "#FF808080";
        private const string VSDarkXMLName = "#FF#E6E6E6";
        private const string VSDarkXMLAttribute = "#FF92CAF4";
        private const string VSDarkXAMLCData = "#FFC0D088";
        private const string VSDarkXMLComment = "#FF608B4E";

        private const string VSDarkComment = "#FF57A64A";
        private const string VSDarkKeyword = "#FF569CD6";
        private const string VSDarkGray = "#FF9B9B9B";
        private const string VSDarkNumber = "#FFB5CEA8";
        private const string VSDarkClass = "#FF4EC9B0";
        private const string VSDarkString = "#FFD69D85";

        /// <summary>
        /// A theme with Dark Colors.
        /// </summary>
        public static StyleDictionary DefaultDark
        {
            get
            {
                return new StyleDictionary
                {
                    new Style(ScopeName.PlainText)
                    {
                        Foreground = VSDarkPlainText,
                        Background = VSDarkBackground,
                        ReferenceName = "plainText"
                    },
                    new Style(ScopeName.HtmlServerSideScript)
                    {
                        Background = Yellow,
                        ReferenceName = "htmlServerSideScript"
                    },
                    new Style(ScopeName.HtmlComment)
                    {
                        Foreground = VSDarkComment,
                        ReferenceName = "htmlComment"
                    },
                    new Style(ScopeName.HtmlTagDelimiter)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "htmlTagDelimiter"
                    },
                    new Style(ScopeName.HtmlElementName)
                    {
                        Foreground = DullRed,
                        ReferenceName = "htmlElementName"
                    },
                    new Style(ScopeName.HtmlAttributeName)
                    {
                        Foreground = Red,
                        ReferenceName = "htmlAttributeName"
                    },
                    new Style(ScopeName.HtmlAttributeValue)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "htmlAttributeValue"
                    },
                    new Style(ScopeName.HtmlOperator)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "htmlOperator"
                    },
                    new Style(ScopeName.Comment)
                    {
                        Foreground = VSDarkComment,
                        ReferenceName = "comment"
                    },
                    new Style(ScopeName.XmlDocTag)
                    {
                        Foreground = VSDarkXMLComment,
                        ReferenceName = "xmlDocTag"
                    },
                    new Style(ScopeName.XmlDocComment)
                    {
                        Foreground = VSDarkXMLComment,
                        ReferenceName = "xmlDocComment"
                    },
                    new Style(ScopeName.String)
                    {
                        Foreground = VSDarkString,
                        ReferenceName = "string"
                    },
                    new Style(ScopeName.StringCSharpVerbatim)
                    {
                        Foreground = VSDarkString,
                        ReferenceName = "stringCSharpVerbatim"
                    },
                    new Style(ScopeName.Keyword)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "keyword"
                    },
                    new Style(ScopeName.PreprocessorKeyword)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "preprocessorKeyword"
                    },
                    new Style(ScopeName.HtmlEntity)
                    {
                        Foreground = Red,
                        ReferenceName = "htmlEntity"
                    },
                    new Style(ScopeName.XmlAttribute)
                    {
                        Foreground = VSDarkXMLAttribute,
                        ReferenceName = "xmlAttribute"
                    },
                    new Style(ScopeName.XmlAttributeQuotes)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "xmlAttributeQuotes"
                    },
                    new Style(ScopeName.XmlAttributeValue)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "xmlAttributeValue"
                    },
                    new Style(ScopeName.XmlCDataSection)
                    {
                        Foreground = VSDarkXAMLCData,
                        ReferenceName = "xmlCDataSection"
                    },
                    new Style(ScopeName.XmlComment)
                    {
                        Foreground = VSDarkComment,
                        ReferenceName = "xmlComment"
                    },
                    new Style(ScopeName.XmlDelimiter)
                    {
                        Foreground = VSDarkXMLDelimeter,
                        ReferenceName = "xmlDelimiter"
                    },
                    new Style(ScopeName.XmlName)
                    {
                        Foreground = VSDarkXMLName,
                        ReferenceName = "xmlName"
                    },
                    new Style(ScopeName.ClassName)
                    {
                        Foreground = VSDarkClass,
                        ReferenceName = "className"
                    },
                    new Style(ScopeName.CssSelector)
                    {
                        Foreground = DullRed,
                        ReferenceName = "cssSelector"
                    },
                    new Style(ScopeName.CssPropertyName)
                    {
                        Foreground = Red,
                        ReferenceName = "cssPropertyName"
                    },
                    new Style(ScopeName.CssPropertyValue)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "cssPropertyValue"
                    },
                    new Style(ScopeName.SqlSystemFunction)
                    {
                        Foreground = Magenta,
                        ReferenceName = "sqlSystemFunction"
                    },
                    new Style(ScopeName.PowerShellAttribute)
                    {
                        Foreground = PowderBlue,
                        ReferenceName = "powershellAttribute"
                    },
                    new Style(ScopeName.PowerShellOperator)
                    {
                        Foreground = VSDarkGray,
                        ReferenceName = "powershellOperator"
                    },
                    new Style(ScopeName.PowerShellType)
                    {
                        Foreground = Teal,
                        ReferenceName = "powershellType"
                    },
                    new Style(ScopeName.PowerShellVariable)
                    {
                        Foreground = OrangeRed,
                        ReferenceName = "powershellVariable"
                    },

                    new Style(ScopeName.Type)
                    {
                        Foreground = Teal,
                        ReferenceName = "type"
                    },
                    new Style(ScopeName.TypeVariable)
                    {
                        Foreground = Teal,
                        Italic = true,
                        ReferenceName = "typeVariable"
                    },
                    new Style(ScopeName.NameSpace)
                    {
                        Foreground = Navy,
                        ReferenceName = "namespace"
                    },
                    new Style(ScopeName.Constructor)
                    {
                        Foreground = Purple,
                        ReferenceName = "constructor"
                    },
                    new Style(ScopeName.Predefined)
                    {
                        Foreground = Navy,
                        ReferenceName = "predefined"
                    },
                    new Style(ScopeName.PseudoKeyword)
                    {
                        Foreground = Navy,
                        ReferenceName = "pseudoKeyword"
                    },
                    new Style(ScopeName.StringEscape)
                    {
                        Foreground = VSDarkGray,
                        ReferenceName = "stringEscape"
                    },
                    new Style(ScopeName.ControlKeyword)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "controlKeyword"
                    },
                    new Style(ScopeName.Number)
                    {
                        ReferenceName = "number",
                        Foreground = VSDarkNumber
                    },
                    new Style(ScopeName.Operator)
                    {
                        ReferenceName = "operator"
                    },
                    new Style(ScopeName.Delimiter)
                    {
                        ReferenceName = "delimiter"
                    },

                    new Style(ScopeName.MarkdownHeader)
                    {
                        Foreground = VSDarkKeyword,
                        Bold = true,
                        ReferenceName = "markdownHeader"
                    },
                    new Style(ScopeName.MarkdownCode)
                    {
                        Foreground = VSDarkString,
                        ReferenceName = "markdownCode"
                    },
                    new Style(ScopeName.MarkdownListItem)
                    {
                        Bold = true,
                        ReferenceName = "markdownListItem"
                    },
                    new Style(ScopeName.MarkdownEmph)
                    {
                        Italic = true,
                        ReferenceName = "italic"
                    },
                    new Style(ScopeName.MarkdownBold)
                    {
                        Bold = true,
                        ReferenceName = "bold"
                    },

                    new Style(ScopeName.BuiltinFunction)
                    {
                        Foreground = OliveDrab,
                        Bold = true,
                        ReferenceName = "builtinFunction"
                    },
                    new Style(ScopeName.BuiltinValue)
                    {
                        Foreground = DarkOliveGreen,
                        Bold = true,
                        ReferenceName = "builtinValue"
                    },
                    new Style(ScopeName.Attribute)
                    {
                        Foreground = DarkCyan,
                        Italic = true,
                        ReferenceName = "attribute"
                    },
                    new Style(ScopeName.SpecialCharacter)
                    {
                        ReferenceName = "specialChar"
                    },
                };
            }
        }
    }
}