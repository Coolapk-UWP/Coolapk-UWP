// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace ColorCode
{
    /// <summary>
    /// Defines how ColorCode will parse the source code of a given language.
    /// </summary>
    public interface ILanguage
    {
        /// <summary>
        /// Gets the identifier of the language (e.g., csharp).
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the first line pattern (regex) to use when determining if the language matches a source text.
        /// </summary>
        string FirstLinePattern { get; }

        /// <summary>
        /// Gets the "friendly" name of the language (e.g., C#).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the collection of language rules in the language.
        /// </summary>
        IList<LanguageRule> Rules { get; }

        /// <summary>
        /// Get the CSS class name to use for a language
        /// </summary>
        string CssClassName { get; }

        /// <summary>
        /// Returns true if the specified string is an alias for the language
        /// </summary>
        bool HasAlias(string lang);
    }
}