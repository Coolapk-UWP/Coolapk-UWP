// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Parsers.Markdown.Helpers;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using System;
using System.Collections.Generic;

namespace Microsoft.Toolkit.Parsers.Markdown.Blocks
{
    /// <summary>
    /// Represents a block of text that is displayed as a single paragraph.
    /// </summary>
    [Obsolete(Constants.ParserObsoleteMsg)]
    public class ParagraphBlock : MarkdownBlock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParagraphBlock"/> class.
        /// </summary>
        public ParagraphBlock()
            : base(MarkdownBlockType.Paragraph)
        {
        }

        /// <summary>
        /// Gets or sets the contents of the block.
        /// </summary>
        public IList<MarkdownInline> Inlines { get; set; }

        /// <summary>
        /// Parses paragraph text.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <returns> A parsed paragraph. </returns>
        internal static ParagraphBlock Parse(string markdown)
        {
            ParagraphBlock result = new ParagraphBlock
            {
                Inlines = Common.ParseInlineChildren(markdown, 0, markdown.Length)
            };
            return result;
        }

        /// <summary>
        /// Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string ToString()
        {
            return Inlines == null ? base.ToString() : string.Join(string.Empty, Inlines);
        }
    }
}