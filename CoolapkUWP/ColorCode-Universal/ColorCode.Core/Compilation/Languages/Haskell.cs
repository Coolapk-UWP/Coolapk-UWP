// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;
using System.Collections.Generic;

namespace ColorCode.Compilation.Languages
{
    public class Haskell : ILanguage
    {
        public string Id
        {
            get { return LanguageId.Haskell; }
        }

        public string Name
        {
            get { return "Haskell"; }
        }

        public string CssClassName
        {
            get { return "haskell"; }
        }

        public string FirstLinePattern
        {
            get
            {
                return null;
            }
        }

        private const string nonnestComment = @"((?:--.*\r?\n|{-(?:[^-]|-(?!})|[\r\n])*-}))";

        private const string incomment = @"([^-{}]|{(?!-)|-(?!})|(?<!-)})*";
        private const string keywords = @"case|class|data|default|deriving|do|else|foreign|if|import|in|infix|infixl|infixr|instance|let|module|newtype|of|then|type|where";
        private const string opKeywords = @"\.\.|:|::|=|\\|\||<-|->|@|~|=>";

        private const string vsymbol = @"[\!\#\$%\&\⋆\+\./<=>\?@\\\^\-~\|]";
        private const string symbol = @"(?:" + vsymbol + "|:)";

        private const string varop = vsymbol + "(?:" + symbol + @")*";
        private const string conop = ":(?:" + symbol + @")*";

        private const string conid = @"(?:[A-Z][\w']*|\(" + conop + @"\))";
        private const string varid = @"(?:[a-z][\w']*|\(" + varop + @"\))";

        private const string qconid = @"((?:[A-Z][\w']*\.)*)" + conid;
        private const string qvarid = @"((?:[A-Z][\w']*\.)*)" + varid;
        private const string qconidop = @"((?:[A-Z][\w']*\.)*)(?:" + conid + "|" + conop + ")";

        private const string intype = @"(\bforall\b|=>)|" + qconidop + @"|(?!deriving|where|data|type|newtype|instance|class)([a-z][\w']*)|\->|[ \t!\#]|\r?\n[ \t]+(?=[\(\)\[\]]|->|=>|[A-Z])";
        private const string toptype = "(?:" + intype + "|::)";
        private const string nestedtype = @"(?:" + intype + ")";

        private const string datatype = "(?:" + intype + @"|[,]|\r?\n[ \t]+|::|(?<!" + symbol + @"|^)([=\|])\s*(" + conid + ")|" + nonnestComment + ")";

        private const string inexports = @"(?:[\[\],\s]|(" + conid + ")|" + varid
                                          + "|" + nonnestComment
                                          + @"|\((?:[,\.\s]|(" + conid + ")|" + varid + @")*\)"
                                          + ")*";

        public IList<LanguageRule> Rules
        {
            get
            {
                return new List<LanguageRule> {
                    // Nested block comments: note does not match no unclosed block comments.
                    new LanguageRule(
                        // Handle nested block comments using named balanced groups
                        @"{-+" + incomment +
                        @"(?>" +
                        @"(?>(?<comment>{-)" + incomment + ")+" +
                        @"(?>(?<-comment>-})" + incomment + ")+" +
                        @")*" +
                        @"(-+})",
                        new Dictionary<int, string>
                            {
                                { 0, ScopeName.Comment },
                            }),
           
           
                    // Line comments
                    new LanguageRule(
                        @"(--.*?)\r?$",
                        new Dictionary<int, string>
                            {
                                { 1, ScopeName.Comment }
                            }),    
        
                    // Types
                    new LanguageRule(
                        // Type highlighting using named balanced groups to balance parenthesized sub types
                        // 'toptype' and 'nestedtype' capture three groups: type keywords, namespaces, and type variables 
                        @"(?:" + @"\b(class|instance|deriving)\b"
                                + @"|::(?!" + symbol + ")"
                                + @"|\b(type)\s+" + toptype + @"*\s*(=)"
                                + @"|\b(data|newtype)\s+" + toptype + @"*\s*(=)\s*(" + conid + ")"
                                + @"|\s+(\|)\s*(" + conid + ")"
                          + ")" + toptype + "*" +
                        @"(?:" +
                            @"(?:(?<type>[\(\[<])(?:" + nestedtype + @"|[,]" + @")*)+" +
                            @"(?:(?<-type>[\)\]>])(?:" + nestedtype + @"|(?(type)[,])" + @")*)+" +
                        @")*",
                        new Dictionary<int,string> {
                            { 0, ScopeName.Type },

                            { 1, ScopeName.Keyword },   // class instance etc

                            { 2, ScopeName.Keyword},        // type
                            { 3, ScopeName.Keyword},
                            { 4, ScopeName.NameSpace },
                            { 5, ScopeName.TypeVariable },
                            { 6, ScopeName.Keyword},

                            { 7, ScopeName.Keyword},        // data , newtype
                            { 8, ScopeName.Keyword},
                            { 9, ScopeName.NameSpace },
                            { 10, ScopeName.TypeVariable },
                            { 11, ScopeName.Keyword },       // = conid
                            { 12, ScopeName.Constructor },

                            { 13, ScopeName.Keyword },       // | conid
                            { 14, ScopeName.Constructor },

                            { 15, ScopeName.Keyword},
                            { 16, ScopeName.NameSpace },
                            { 17, ScopeName.TypeVariable },

                            { 18, ScopeName.Keyword },
                            { 19, ScopeName.NameSpace },
                            { 20, ScopeName.TypeVariable },

                            { 21, ScopeName.Keyword },
                            { 22, ScopeName.NameSpace },
                            { 23, ScopeName.TypeVariable },
                        }),

                        
                    // Special sequences
                    new LanguageRule(
                        @"\b(module)\s+(" + qconid + @")(?:\s*\(" + inexports + @"\))?",
                        new Dictionary<int, string>
                            {
                                { 1, ScopeName.Keyword },
                                { 2, ScopeName.NameSpace },
                                { 4, ScopeName.Type },
                                { 5, ScopeName.Comment },
                                { 6, ScopeName.Constructor }
                            }),
                    new LanguageRule(
                        @"\b(module|as)\s+(" + qconid + ")",
                        new Dictionary<int, string>
                            {
                                { 1, ScopeName.Keyword },
                                { 2, ScopeName.NameSpace }
                            }),

                    new LanguageRule(
                        @"\b(import)\s+(qualified\s+)?(" + qconid + @")\s*"
                            + @"(?:\(" + inexports + @"\))?"
                            + @"(?:(hiding)(?:\s*\(" + inexports + @"\)))?",
                        new Dictionary<int, string>
                            {
                                { 1, ScopeName.Keyword },
                                { 2, ScopeName.Keyword },
                                { 3, ScopeName.NameSpace },
                                { 5, ScopeName.Type },
                                { 6, ScopeName.Comment },
                                { 7, ScopeName.Constructor },
                                { 8, ScopeName.Keyword},
                                { 9, ScopeName.Type },
                                { 10, ScopeName.Comment },
                                { 11, ScopeName.Constructor }
                            }),
    
                    // Keywords
                    new LanguageRule(
                        @"\b(" + keywords + @")\b",
                        new Dictionary<int, string>
                            {
                                { 1, ScopeName.Keyword },
                            }),
                    new LanguageRule(
                        @"(?<!" + symbol +")(" + opKeywords + ")(?!" + symbol + ")",
                        new Dictionary<int, string>
                            {
                                { 1, ScopeName.Keyword },
                            }),
                                   
                    // Names
                    new LanguageRule(
                        qvarid,
                        new Dictionary<int, string>
                            {
                                { 1, ScopeName.NameSpace }
                            }),
                    new LanguageRule(
                        qconid,
                        new Dictionary<int, string>
                            {
                                { 0, ScopeName.Constructor },
                                { 1, ScopeName.NameSpace },
                            }),

                    // Operators and punctuation
                    new LanguageRule(
                        varop,
                        new Dictionary<int, string>
                            {
                                { 0, ScopeName.Operator }
                            }),
                    new LanguageRule(
                        conop,
                        new Dictionary<int, string>
                            {
                                { 0, ScopeName.Constructor }
                            }),

                    new LanguageRule(
                        @"[{}\(\)\[\];,]",
                        new Dictionary<int, string>
                            {
                                { 0, ScopeName.Delimiter }
                            }),

                    // Literals
                    new LanguageRule(
                        @"0[xX][\da-fA-F]+|\d+(\.\d+([eE][\-+]?\d+)?)?",
                        new Dictionary<int, string>
                            {
                                { 0, ScopeName.Number }
                            }),


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
                };
            }
        }

        public bool HasAlias(string lang)
        {
            switch (lang.ToLower())
            {
                case "hs":
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