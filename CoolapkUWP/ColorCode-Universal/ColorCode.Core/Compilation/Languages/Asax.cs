// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using ColorCode.Common;

namespace ColorCode.Compilation.Languages
{
    public class Asax : ILanguage
    {
        public string Id
        {
            get { return LanguageId.Asax; }
        }

        public string Name
        {
            get { return "ASAX"; }
        }

        public string CssClassName
        {
            get { return "asax"; }
        }

        public string FirstLinePattern
        {
            get
            {
                return null;
            }
        }

        public IList<LanguageRule> Rules
        {
            get
            {
                return new List<LanguageRule>
                           {
                               new LanguageRule(
                                   @"(<%)(--.*?--)(%>)",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlComment },
                                           { 3, ScopeName.HtmlServerSideScript }
                                       }),
                               new LanguageRule(
                                   @"(?is)(?<=<%@.+?language=""c\#"".*?%>.*?<script.*?runat=""server"">)(.*)(?=</script>)",
                                   new Dictionary<int, string>
                                       {
                                           { 1, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.CSharp) }
                                       }),
                               new LanguageRule(
                                   @"(?is)(?<=<%@.+?language=""vb"".*?%>.*?<script.*?runat=""server"">)(.*)(?=</script>)",
                                   new Dictionary<int, string>
                                       {
                                           { 1, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.VbDotNet) }
                                       }),
                               new LanguageRule(
                                   @"(?xi)(</?)
                                         (?: ([a-z][a-z0-9-]*)(:) )*
                                         ([a-z][a-z0-9-_]*)
                                         (?:
                                            [\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*([^\s\n""']+?)
                                           |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(""[^\n]+?"")
                                           |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*('[^\n]+?')
                                           |[\s\n]+([a-z0-9-_]+) )*
                                         [\s\n]*
                                         (/?>)",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlTagDelimiter },
                                           { 4, ScopeName.HtmlElementName },
                                           { 5, ScopeName.HtmlAttributeName },
                                           { 6, ScopeName.HtmlOperator },
                                           { 7, ScopeName.HtmlAttributeValue },
                                           { 8, ScopeName.HtmlAttributeName },
                                           { 9, ScopeName.HtmlOperator },
                                           { 10, ScopeName.HtmlAttributeValue },
                                           { 11, ScopeName.HtmlAttributeName },
                                           { 12, ScopeName.HtmlOperator },
                                           { 13, ScopeName.HtmlAttributeValue },
                                           { 14, ScopeName.HtmlAttributeName },
                                           { 15, ScopeName.HtmlTagDelimiter }
                                       }),
                               new LanguageRule(
                                   @"(<%)(@)(?:\s+([a-zA-Z0-9]+))*(?:\s+([a-zA-Z0-9]+)(=)(""[^\n]*?""))*\s*?(%>)",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlTagDelimiter },
                                           { 3, ScopeName.HtmlElementName },
                                           { 4, ScopeName.HtmlAttributeName },
                                           { 5, ScopeName.HtmlOperator },
                                           { 6, ScopeName.HtmlAttributeValue },
                                           { 7, ScopeName.HtmlServerSideScript }
                                       })
                           };
            }
        }

        public bool HasAlias(string lang)
        {
            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}