// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Parsing;

namespace ColorCode.Common
{
    public class TextInsertion
    {
        public virtual int Index { get; set; }
        public virtual string Text { get; set; }
        public virtual Scope Scope { get; set; }
    }
}