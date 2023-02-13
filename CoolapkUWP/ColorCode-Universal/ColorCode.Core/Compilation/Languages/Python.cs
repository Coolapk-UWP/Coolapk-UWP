// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using ColorCode.Common;

namespace ColorCode.Compilation.Languages
{
    public class Python : ILanguage
    {
        public string Id
        {
            get { return LanguageId.Python; }
        }

        public string Name
        {
            get { return "Python"; }
        }

        public string CssClassName
        {
            get { return "python"; }
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
                               // docstring comments
                               new LanguageRule(
                                   @"(?<=:\s*)(""{3})([^""]+)(""{3})",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new LanguageRule(
                                   @"(?<=:\s*)('{3})([^']+)('{3})",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                                       
                               // regular comments
                               new LanguageRule(
                                   @"(#.*)\r?",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                                       
                               // multi-line strings
                               new LanguageRule(
                                   @"(?<==\s*f*b*r*u*)(""{3})([^""]+)(""{3})",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new LanguageRule(
                                   @"(?<==\s*f*b*r*u*)('{3})([^']+)('{3})",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                                       
                               // regular strings
                               new LanguageRule(
                                   @"'[^\n]*?'",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new LanguageRule(
                                   @"""[^\n]*?""",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               
                               // keywords
                               new LanguageRule(
                                   @"(?i)\b(False|await|else|import|pass|None|break|except|in|raise|True|class|finally|is|return|and|continue|for|lambda|try|as|def|from|" + 
                                   @"nonlocal|while|assert|del|global|not|with|async|elif|if|or|yield|self)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Keyword },
                                       }),
                                       
                               // intrinsic functions
                               new LanguageRule(
                                   @"(?i)\b(abs|delattr|hash|memoryview|set|all|dict|help|min|setattr|any|dir|hex|next|slice|ascii|divmod|id|object|sorted|bin|enumerate" +
                                   "|input|oct|staticmethod|bool|eval|int|open|str|breakpoint|exec|isinstance|ord|sum|bytearray|filter|issubclass|pow|super|bytes|float" +
                                   "|iter|print|tuple|callable|format|len|property|type|chr|frozenset|list|range|vars|classmethod|getattr|locals|repr|zip|compile|globals" +
                                   @"|map|reversed|__import__|complex|hasattr|max|round)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Intrinsic },
                                       }),

                                // numbers
                                new LanguageRule(
                                   @"\b([0-9.]|[0-9.]+(e-*)(?=[0-9]))+?\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Number },
                                       }),
                           };
            }
        }

        public bool HasAlias(string lang)
        {
            switch (lang.ToLower())
            {
                case "py":
                    return true;
                
                case "python":
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