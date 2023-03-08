// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;
using System.Collections.Generic;

namespace ColorCode.Compilation
{
    public static class RuleCaptures
    {
        public static IDictionary<int, string> JavaScript;
        public static IDictionary<int, string> CSharpScript;
        public static IDictionary<int, string> VbDotNetScript;

        static RuleCaptures()
        {
            JavaScript = BuildCaptures(LanguageId.JavaScript);
            CSharpScript = BuildCaptures(LanguageId.CSharp);
            VbDotNetScript = BuildCaptures(LanguageId.VbDotNet);
        }

        private static IDictionary<int, string> BuildCaptures(string languageId)
        {
            return new Dictionary<int, string>
                       {
                           {1, ScopeName.HtmlTagDelimiter},
                           {2, ScopeName.HtmlElementName},
                           {3, ScopeName.HtmlAttributeName},
                           {4, ScopeName.HtmlOperator},
                           {5, ScopeName.HtmlAttributeValue},
                           {6, ScopeName.HtmlAttributeName},
                           {7, ScopeName.HtmlOperator},
                           {8, ScopeName.HtmlAttributeValue},
                           {9, ScopeName.HtmlAttributeName},
                           {10, ScopeName.HtmlOperator},
                           {11, ScopeName.HtmlAttributeValue},
                           {12, ScopeName.HtmlAttributeName},
                           {13, ScopeName.HtmlAttributeName},
                           {14, ScopeName.HtmlOperator},
                           {15, ScopeName.HtmlAttributeValue},
                           {16, ScopeName.HtmlAttributeName},
                           {17, ScopeName.HtmlOperator},
                           {18, ScopeName.HtmlAttributeValue},
                           {19, ScopeName.HtmlAttributeName},
                           {20, ScopeName.HtmlOperator},
                           {21, ScopeName.HtmlAttributeValue},
                           {22, ScopeName.HtmlAttributeName},
                           {23, ScopeName.HtmlOperator},
                           {24, ScopeName.HtmlAttributeValue},
                           {25, ScopeName.HtmlAttributeName},
                           {26, ScopeName.HtmlTagDelimiter},
                           {27, string.Format("{0}{1}", ScopeName.LanguagePrefix, languageId)},
                           {28, ScopeName.HtmlTagDelimiter},
                           {29, ScopeName.HtmlElementName},
                           {30, ScopeName.HtmlTagDelimiter}
                       };
        }
    }
}