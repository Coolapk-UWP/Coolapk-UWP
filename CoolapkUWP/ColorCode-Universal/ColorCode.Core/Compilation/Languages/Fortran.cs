// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Copyright (c) 2015 Christopher Pardi.

using System.Collections.Generic;
using ColorCode.Common;

namespace ColorCode.Compilation.Languages
{
    public class Fortran : ILanguage
    {
        public string Id
        {
            get { return LanguageId.Fortran; }
        }

        public string Name
        {
            get { return "Fortran"; }
        }

        public string CssClassName
        {
            get { return "fortran"; }
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
                               // Comments
                                new LanguageRule(
                                   @"!.*",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                                // String type 1
                                new LanguageRule(
                                   @""".*?""|'.*?'",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                                // Program keywords
                                new LanguageRule(
                                   @"(?i)\b(?:program|endprogram)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Module keywords
                                new LanguageRule(
                                   @"(?i)\b(?:module|endmodule|contains)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Type keywords
                                new LanguageRule(
                                   @"(?i)\b(?:type|endtype|abstract)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Interface definition keywords
                                new LanguageRule(
                                   @"(?i)\b(?:interface|endinterface|operator|assignment)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Procedure definition keywords
                                new LanguageRule(
                                   @"(?i)\b(?:function|endfunction|subroutine|endsubroutine|elemental|recursive|pure)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                               // Variable keywords
                                new LanguageRule(
                                   @"(?i)INTEGER|REAL|DOUBLE\s*PRECISION|COMPLEX|CHARACTER|LOGICAL|TYPE",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Attribute keywords
                                new LanguageRule(
                                   @"(?i)\b(?:parameter|allocatable|optional|pointer|save|dimension|target)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Intent keywords
                                new LanguageRule(
                                   @"(?i)\b(intent)\s*(\()\s*(in|out|inout)\s*(\))",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Keyword },
                                           { 2, ScopeName.Brackets },
                                           { 3, ScopeName.Keyword },
                                           { 4, ScopeName.Brackets },
                                       }),
                                // Namelist
                                new LanguageRule(
                                   @"(?i)\b(namelist)(/)\w+(/)",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Keyword },
                                           { 2, ScopeName.Brackets },
                                           { 3, ScopeName.Brackets },
                                       }),
                                // Intrinsic functions
                                new LanguageRule(
                                   @"(?i)\b(PRESENT" +
                                    "|INT|REAL|DBLE|CMPLX|AIMAG|CONJG|AINT|ANINT|NINT|ABS|MOD|SIGN|DIM|DPROD|MODULO|FLOOR|CEILING|MAX|MIN" +
                                    "|SQRT|EXP|LOG|LOG10|SIN|COS|TAN|ASIN|ACOS|ATAN|ATAN2|SINH|COSH|TANH" +
                                    "|ICHAR|CHAR|LGE|LGT|LLE|LLT|IACHAR|ACHAR|INDEX|VERIFY|ADJUSTL|ADJUSTR|SCAN|LEN_TRIM|REPEAT|TRIM" +
                                    "|KIND|SELECTED_INT_KIND|SELECTED_REAL_KIND" +
                                    "|LOGICAL" +
                                    "|IOR|IAND|NOT|IEOR| ISHFT|ISHFTC|BTEST|IBSET|IBCLR|BITSIZE" +
                                    "|TRANSFER" +
                                    "|RADIX|DIGITS|MINEXPONENT|MAXEXPONENT|PRECISION|RANGE|HUGE|TINY|EPSILON" +
                                    "|EXPONENT|SCALE|NEAREST|FRACTION|SET_EXPONENT|SPACING|RRSPACING" +
                                    "|LBOUND|SIZE|UBOUND" +
                                    "|MASK" +
                                    "|MATMUL|DOT_PRODUCT" +
                                    "|SUM|PRODUCT|MAXVAL|MINVAL|COUNT|ANY|ALL" +
                                    "|ALLOCATED|SIZE|SHAPE|LBOUND|UBOUND" +
                                    "|MERGE|SPREAD|PACK|UNPACK" +
                                    "|RESHAPE" +
                                    "|TRANSPOSE|EOSHIFT|CSHIFT" +
                                    "|MAXLOC|MINLOC" +
                                   @"|ASSOCIATED)\b(\()"
                                    ,
                                   new Dictionary<int, string>
                                       {
                                           {1, ScopeName.Intrinsic },
                                           {2, ScopeName.Brackets },
                                       }),
                                // Intrinsic functions
                                new LanguageRule(
                                   @"(?i)(call)\s+(" +
                                   "DATE_AND_TIME|SYSTEM_CLOCK" +
                                   "|RANDOM_NUMBER|RANDOM_SEED" +
                                   "|MVBITS" +
                                   ")\b"
                                    ,
                                   new Dictionary<int, string>
                                       {
                                           {1, ScopeName.Keyword },
                                           {2, ScopeName.Intrinsic },
                                       }),
                                // Special Character
                                new LanguageRule(
                                   @"\=|\*|\+|\\|\-|\.\w+\.|>|<|::|%|,|;|\?|\$",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.SpecialCharacter },
                                       }),
                                // Line Continuation
                                new LanguageRule(
                                   @"&",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Continuation },
                                       }),
                                // Number
                                new LanguageRule(
                                   @"\b[0-9.]+(_\w+)?\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Number },
                                       }),
                                // Brackets
                                new LanguageRule(
                                   @"[\(\)\[\]]",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Brackets },
                                       }),
                                // Preprocessor keywords
                                new LanguageRule(
                                   @"(?i)(?:#define|#elif|#elifdef|#elifndef|#else|#endif|#error|#if|#ifdef|#ifndef|#include|#line|#pragma|#undef)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.PreprocessorKeyword },
                                       }),
                                // General keywords
                                new LanguageRule(
                                   @"(?i)\b(?:end|use|do|enddo|select|endselect|case|endcase|if|then|else|endif|implicit|none"
                                        + @"|do\s+while|call|public|private|protected|where|go\s*to|file|block|data|blockdata"
                                        + @"|end\s*blockdata|default|procedure|include|go\s*to|allocate|deallocate|open|close|write|stop|return)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.Keyword },
                                       })
                           };
            }
        }

        public bool HasAlias(string lang)
        {
            switch (lang.ToLower())
            {
                case "fortran":
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