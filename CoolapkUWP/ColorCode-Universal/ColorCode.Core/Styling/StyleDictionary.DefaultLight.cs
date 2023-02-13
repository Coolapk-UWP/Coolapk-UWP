// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;

namespace ColorCode.Styling
{
    /// <summary>
    /// Defines the Default Light Theme.
    /// </summary>
    public partial class StyleDictionary
    {
        /// <summary>
        /// A theme with Light Colors.
        /// </summary>
        public static StyleDictionary DefaultLight
        {
            get
            {
                return new StyleDictionary
                {
                    new Style(ScopeName.PlainText)
                    {
                        Foreground = Black,
                        Background = White,
                        ReferenceName = "plainText"
                    },
                    new Style(ScopeName.HtmlServerSideScript)
                    {
                        Background = Yellow,
                        ReferenceName = "htmlServerSideScript"
                    },
                    new Style(ScopeName.HtmlComment)
                    {
                        Foreground = Green,
                        ReferenceName = "htmlComment"
                    },
                    new Style(ScopeName.HtmlTagDelimiter)
                    {
                        Foreground = Blue,
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
                        Foreground = Blue,
                        ReferenceName = "htmlAttributeValue"
                    },
                    new Style(ScopeName.HtmlOperator)
                    {
                        Foreground = Blue,
                        ReferenceName = "htmlOperator"
                    },
                    new Style(ScopeName.Comment)
                    {
                        Foreground = Green,
                        ReferenceName = "comment"
                    },
                    new Style(ScopeName.XmlDocTag)
                    {
                        Foreground = Gray,
                        ReferenceName = "xmlDocTag"
                    },
                    new Style(ScopeName.XmlDocComment)
                    {
                        Foreground = Green,
                        ReferenceName = "xmlDocComment"
                    },
                    new Style(ScopeName.String)
                    {
                        Foreground = DullRed,
                        ReferenceName = "string"
                    },
                    new Style(ScopeName.StringCSharpVerbatim)
                    {
                        Foreground = DullRed,
                        ReferenceName = "stringCSharpVerbatim"
                    },
                    new Style(ScopeName.Keyword)
                    {
                        Foreground = Blue,
                        ReferenceName = "keyword"
                    },
                    new Style(ScopeName.PreprocessorKeyword)
                    {
                        Foreground = Blue,
                        ReferenceName = "preprocessorKeyword"
                    },
                    new Style(ScopeName.HtmlEntity)
                    {
                        Foreground = Red,
                        ReferenceName = "htmlEntity"
                    },
                    new Style(ScopeName.XmlAttribute)
                    {
                        Foreground = Red,
                        ReferenceName = "xmlAttribute"
                    },
                    new Style(ScopeName.XmlAttributeQuotes)
                    {
                        Foreground = Black,
                        ReferenceName = "xmlAttributeQuotes"
                    },
                    new Style(ScopeName.XmlAttributeValue)
                    {
                        Foreground = Blue,
                        ReferenceName = "xmlAttributeValue"
                    },
                    new Style(ScopeName.XmlCDataSection)
                    {
                        Foreground = Gray,
                        ReferenceName = "xmlCDataSection"
                    },
                    new Style(ScopeName.XmlComment)
                    {
                        Foreground = Green,
                        ReferenceName = "xmlComment"
                    },
                    new Style(ScopeName.XmlDelimiter)
                    {
                        Foreground = Blue,
                        ReferenceName = "xmlDelimiter"
                    },
                    new Style(ScopeName.XmlName)
                    {
                        Foreground = DullRed,
                        ReferenceName = "xmlName"
                    },
                    new Style(ScopeName.ClassName)
                    {
                        Foreground = MediumTurqoise,
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
                        Foreground = Blue,
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
                        Foreground = Gray,
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
                        Foreground = Gray,
                        ReferenceName = "stringEscape"
                    },
                    new Style(ScopeName.ControlKeyword)
                    {
                        Foreground = Blue,
                        ReferenceName = "controlKeyword"
                    },
                    new Style(ScopeName.Number)
                    {
                        ReferenceName = "number"
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
                        Foreground = Blue,
                        Bold = true,
                        ReferenceName = "markdownHeader"
                    },
                    new Style(ScopeName.MarkdownCode)
                    {
                        Foreground = Teal,
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