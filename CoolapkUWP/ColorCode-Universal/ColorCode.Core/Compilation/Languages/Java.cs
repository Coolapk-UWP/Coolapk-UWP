// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;
using System.Collections.Generic;

namespace ColorCode.Compilation.Languages
{
    public class Java : ILanguage
    {
        public string Id
        {
            get { return LanguageId.Java; }
        }

        public string Name
        {
            get { return "Java"; }
        }

        public string CssClassName
        {
            get { return "java"; }
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
                                   @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new LanguageRule(
                                   @"(//.*?)\r?$",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Comment }
                                       }),
                               new LanguageRule(
                                   @"'[^\n]*?(?<!\\)'",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new LanguageRule(
                                   @"(?s)(""[^\n]*?(?<!\\)"")",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new LanguageRule(
                                   @"\b(abstract|assert|boolean|break|byte|case|catch|char|class|const|continue|default|do|double|else|enum|extends|false|final|finally|float|for|goto|if|implements|import|instanceof|int|interface|long|native|new|null|package|private|protected|public|return|short|static|strictfp|super|switch|synchronized|this|throw|throws|transient|true|try|void|volatile|while)\b",
                                   new Dictionary<int, string>
                                       {
                                           {0, ScopeName.Keyword},
                                       }),
                               new LanguageRule(
                                   @"\b[0-9]{1,}\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Number }
                                       }),
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