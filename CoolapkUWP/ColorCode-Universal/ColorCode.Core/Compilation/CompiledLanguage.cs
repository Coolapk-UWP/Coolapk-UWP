// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ColorCode.Compilation
{
    public class CompiledLanguage
    {
        public CompiledLanguage(string id,
                                string name,
                                Regex regex,
                                IList<string> captures)
        {
            Guard.ArgNotNullAndNotEmpty(id, "id");
            Guard.ArgNotNullAndNotEmpty(name, "name");
            Guard.ArgNotNull(regex, "regex");
            Guard.ArgNotNullAndNotEmpty(captures, "captures");

            Id = id;
            Name = name;
            Regex = regex;
            Captures = captures;
        }

        public IList<string> Captures { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public Regex Regex { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}