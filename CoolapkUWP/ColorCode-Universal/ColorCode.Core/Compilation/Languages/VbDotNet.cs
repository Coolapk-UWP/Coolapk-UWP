// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;
using System.Collections.Generic;

namespace ColorCode.Compilation.Languages
{
    public class VbDotNet : ILanguage
    {
        public string Id
        {
            get { return LanguageId.VbDotNet; }
        }

        public string Name
        {
            get { return "VB.NET"; }
        }

        public string CssClassName
        {
            get { return "vb-net"; }
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
                                   @"('''[^\n]*?)\r?$",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new LanguageRule(
                                   @"((?:'|REM\s+).*?)\r?$",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new LanguageRule(
                                   @"""(?:""""|[^""\n])*""",
                                   new Dictionary<int, string>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new LanguageRule(
                                   @"(?:\s|^)(\#End\sRegion|\#Region|\#Const|\#End\sExternalSource|\#ExternalSource|\#If|\#Else|\#End\sIf)(?:\s|\(|\r?$)",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.PreprocessorKeyword },
                                       }),
                               new LanguageRule(
                                   @"(?i)\b(AddHandler|AddressOf|Aggregate|Alias|All|And|AndAlso|Ansi|Any|As|Ascending|(?<!<)Assembly|Auto|Average|Boolean|By|ByRef|Byte|ByVal|Call|Case|Catch|CBool|CByte|CChar|CDate|CDec|CDbl|Char|CInt|Class|CLng|CObj|Const|Continue|Count|CShort|CSng|CStr|CType|Date|Decimal|Declare|Default|DefaultStyleSheet|Delegate|Descending|Dim|DirectCast|Distinct|Do|Double|Each|Else|ElseIf|End|Enum|Equals|Erase|Error|Event|Exit|Explicit|False|Finally|For|Friend|From|Function|Get|GetType|GoSub|GoTo|Group|Group|Handles|If|Implements|Imports|In|Inherits|Integer|Interface|Into|Is|IsNot|Join|Let|Lib|Like|Long|LongCount|Loop|Max|Me|Min|Mod|Module|MustInherit|MustOverride|My|MyBase|MyClass|Namespace|New|Next|Not|Nothing|NotInheritable|NotOverridable|(?<!\.)Object|Off|On|Option|Optional|Or|Order|OrElse|Overloads|Overridable|Overrides|ParamArray|Partial|Preserve|Private|Property|Protected|Public|RaiseEvent|ReadOnly|ReDim|RemoveHandler|Resume|Return|Select|Set|Shadows|Shared|Short|Single|Skip|Static|Step|Stop|String|Structure|Sub|Sum|SyncLock|Take|Then|Throw|To|True|Try|TypeOf|Unicode|Until|Variant|When|Where|While|With|WithEvents|WriteOnly|Xor|SByte|UInteger|ULong|UShort|Using|CSByte|CUInt|CULng|CUShort|Async|Await)\b",
                                   new Dictionary<int, string>
                                       {
                                           { 1, ScopeName.Keyword },
                                       }),
                           };
            }
        }

        public bool HasAlias(string lang)
        {
            switch (lang.ToLower())
            {
                case "vb.net":
                case "vbnet":
                case "vb":
                case "visualbasic":
                case "visual basic":
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