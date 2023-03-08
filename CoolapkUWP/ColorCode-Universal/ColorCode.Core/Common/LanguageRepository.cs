// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ColorCode.Common
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly Dictionary<string, ILanguage> loadedLanguages;
        private readonly ReaderWriterLockSlim loadLock;

        public LanguageRepository(Dictionary<string, ILanguage> loadedLanguages)
        {
            this.loadedLanguages = loadedLanguages;
            loadLock = new ReaderWriterLockSlim();
        }

        public IEnumerable<ILanguage> All
        {
            get { return loadedLanguages.Values; }
        }

        public ILanguage FindById(string languageId)
        {
            Guard.ArgNotNullAndNotEmpty(languageId, "languageId");

            ILanguage language = null;

            loadLock.EnterReadLock();

            try
            {
                // If we have a matching name for the language then use it
                // otherwise check if any languages have that string as an
                // alias. For example: "js" is an alias for Javascript.
                language = loadedLanguages.FirstOrDefault(x => (x.Key.ToLower() == languageId.ToLower()) ||
                                                               x.Value.HasAlias(languageId)).Value;
            }
            finally
            {
                loadLock.ExitReadLock();
            }

            return language;
        }

        public void Load(ILanguage language)
        {
            Guard.ArgNotNull(language, "language");

            if (string.IsNullOrEmpty(language.Id))
            {
                throw new ArgumentException("The language identifier must not be null or empty.", "language");
            }

            loadLock.EnterWriteLock();

            try
            {
                loadedLanguages[language.Id] = language;
            }
            finally
            {
                loadLock.ExitWriteLock();
            }
        }
    }
}