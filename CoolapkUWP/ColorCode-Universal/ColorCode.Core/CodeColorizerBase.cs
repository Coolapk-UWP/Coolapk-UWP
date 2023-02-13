// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Compilation;
using ColorCode.Parsing;
using ColorCode.Styling;
using System.Collections.Generic;

namespace ColorCode
{
    /// <summary>
    /// Creates a <see cref="CodeColorizerBase"/>, for creating Syntax Highlighted code.
    /// </summary>
    /// <param name="Style">The Custom styles to Apply to the formatted Code.</param>
    /// <param name="languageParser">The language parser that the <see cref="CodeColorizerBase"/> instance will use for its lifetime.</param>
    public abstract class CodeColorizerBase
    {
        public CodeColorizerBase(StyleDictionary Styles, ILanguageParser languageParser)
        {
            this.languageParser = languageParser
                ?? new LanguageParser(new LanguageCompiler(Languages.CompiledLanguages, Languages.CompileLock), Languages.LanguageRepository);

            this.Styles = Styles ?? StyleDictionary.DefaultLight;
        }

        /// <summary>
        /// Writes the parsed source code to the ouput using the specified style sheet.
        /// </summary>
        /// <param name="parsedSourceCode">The parsed source code to format and write to the output.</param>
        /// <param name="scopes">The captured scopes for the parsed source code.</param>
        protected abstract void Write(string parsedSourceCode, IList<Scope> scopes);

        /// <summary>
        /// The language parser that the <see cref="CodeColorizerBase"/> instance will use for its lifetime.
        /// </summary>
        protected readonly ILanguageParser languageParser;

        /// <summary>
        /// The styles to Apply to the formatted Code.
        /// </summary>
        public readonly StyleDictionary Styles;
    }
}