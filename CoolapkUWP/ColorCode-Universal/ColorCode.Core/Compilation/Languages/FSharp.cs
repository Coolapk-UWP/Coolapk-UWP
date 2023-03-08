// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;
using System.Collections.Generic;

namespace ColorCode.Compilation.Languages
{
    public class FSharp : ILanguage
    {
        public string Id
        {
            get { return LanguageId.FSharp; }
        }

        public string Name
        {
            get { return "F#"; }
        }

        public string CssClassName
        {
            get { return "FSharp"; }
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
                                   @"\(\*([^*]|[\r\n]|(\*+([^*)]|[\r\n])))*\*+\)",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new LanguageRule(
                                   @"(///)(?:\s*?(<[/a-zA-Z0-9\s""=]+>))*([^\r\n]*)",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.XmlDocTag },
                                           { 2, ScopeName.XmlDocTag },
                                           { 3, ScopeName.XmlDocComment },
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
                                      @"(?s)@""(?:""""|.)*?""(?!"")",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.StringCSharpVerbatim }
                                       }),
                               new LanguageRule(
                                      @"(?s)""""""(?:""""|.)*?""""""(?!"")",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.StringCSharpVerbatim }
                                       }),
                               new LanguageRule(
                                   @"(?s)(""[^\n]*?(?<!\\)"")",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new LanguageRule(
                                   @"^\s*(\#else|\#endif|\#if).*?$",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.PreprocessorKeyword }
                                       }),
                               new LanguageRule(
                                   @"\b(let!|use!|do!|yield!|return!)\s",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Keyword }
                                       }),
                               new LanguageRule(
                                   @"\b(abstract|and|as|assert|base|begin|class|default|delegate|do|done|downcast|downto|elif|else|end|exception|extern|false|finally|for|fun|function|global|if|in|inherit|inline|interface|internal|lazy|let|match|member|module|mutable|namespace|new|null|of|open|or|override|private|public|rec|return|sig|static|struct|then|to|true|try|type|upcast|use|val|void|when|while|with|yield|atomic|break|checked|component|const|constraint|constructor|continue|eager|fixed|fori|functor|include|measure|method|mixin|object|parallel|params|process|protected|pure|recursive|sealed|tailcall|trait|virtual|volatile|async|let!|use!|do!)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Keyword }
                                       }),
                               new LanguageRule(
                                   @"(\w|\s)(->)(\w|\s)",
                                   new Dictionary<int, string>
                                       {
                                           { 2, ScopeName.Keyword }
                                       }),
                           };
            }
        }

        public bool HasAlias(string lang)
        {
            switch (lang.ToLower())
            {
                case "fs":
                case "f#":
                case "fsharp":
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