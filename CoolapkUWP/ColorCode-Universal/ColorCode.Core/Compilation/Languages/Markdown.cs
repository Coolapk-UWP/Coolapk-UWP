// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using ColorCode.Common;

namespace ColorCode.Compilation.Languages
{
    public class Markdown : ILanguage
    {
        public string Id
        {
            get { return LanguageId.Markdown; }
        }

        public string Name
        {
            get { return "Markdown"; }
        }

        public string CssClassName
        {
            get { return "markdown"; }
        }

        public string FirstLinePattern
        {
            get
            {
                return null;
            }
        }

        private string link(string content = @"([^\]\n]+)")
        {
            return @"\!?\[" + content + @"\][ \t]*(\([^\)\n]*\)|\[[^\]\n]*\])";
        }


        
        public IList<LanguageRule> Rules
        {
            get
            {
                return new List<LanguageRule>
                           {
                               // Heading
                               new LanguageRule(
                                   @"^(\#.*)\r?|^.*\r?\n([-]+|[=]+)\r?$",    
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.MarkdownHeader },
                                       }),


                               // code block
                               new LanguageRule(
                                   @"^([ ]{4}(?![ ])(?:.|\r?\n[ ]{4})*)|^(```+[ \t]*\w*)((?:[ \t\r\n]|.)*?)(^```+)[ \t]*\r?$",    
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.MarkdownCode },
                                           { 2, ScopeName.XmlDocTag },
                                           { 3, ScopeName.MarkdownCode },
                                           { 4, ScopeName.XmlDocTag },
                                       }),

                               // Line
                               new LanguageRule(
                                   @"^\s*((-\s*){3}|(\*\s*){3}|(=\s*){3})[ \t\-\*=]*\r?$",    
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.MarkdownHeader }, 
                                       }),

                               
                               // List
                               new LanguageRule(
                                   @"^[ \t]*([\*\+\-]|\d+\.)",    
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.MarkdownListItem }, 
                                       }),

                               // escape
                               new LanguageRule(
                                   @"\\[\\`\*_{}\[\]\(\)\#\+\-\.\!]",    
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.StringEscape }, 
                                       }),

                               // link
                               new LanguageRule(
                                   link(link()) + "|" + link(),  // support nested links (mostly for images)
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.MarkdownBold }, 
                                           { 2, ScopeName.XmlDocTag }, 
                                           { 3, ScopeName.XmlDocTag },    
                                           { 4, ScopeName.MarkdownBold }, 
                                           { 5, ScopeName.XmlDocTag },    
                                       }),
                               new LanguageRule(
                                   @"^[ ]{0,3}\[[^\]\n]+\]:(.|\r|\n[ \t]+(?![\r\n]))*$",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.XmlDocTag },    // nice gray
                                       }),

                               // bold
                               new LanguageRule(
                                   @"\*(?!\*)([^\*\n]|\*\w)+?\*(?!\w)|\*\*([^\*\n]|\*(?!\*))+?\*\*",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.MarkdownBold }, 
                                       }),

                               // emphasized 
                               new LanguageRule(
                                   @"_([^_\n]|_\w)+?_(?!\w)|__([^_\n]|_(?=[\w_]))+?__(?!\w)",    
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.MarkdownEmph }, 
                                       }),
                               
                               // inline code
                               new LanguageRule(
                                   @"`[^`\n]+?`|``([^`\n]|`(?!`))+?``",    
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.MarkdownCode }, 
                                       }),

                               // strings
                               new LanguageRule(
                                   @"""[^""\n]+?""|'[\w\-_]+'",    
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String }, 
                                       }),

                               // html tag
                               new LanguageRule(
                                   @"</?\w.*?>",    
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.HtmlTagDelimiter },
                                       }),

                               // html entity
                               new LanguageRule(
                                   @"\&\#?\w+?;",    
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.HtmlEntity },
                                       }),
                           };
            }
        }

        public bool HasAlias(string lang)
        {
            switch (lang.ToLower())
            {
                case "md":
                case "markdown":
                    return true;

                default:
                    return false;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}