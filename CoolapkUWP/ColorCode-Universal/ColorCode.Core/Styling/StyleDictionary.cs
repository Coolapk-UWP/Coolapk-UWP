// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;

namespace ColorCode.Styling
{
    /// <summary>
    /// A dictionary of <see cref="Style" /> instances, keyed by the styles' scope name.
    /// </summary>
    public partial class StyleDictionary : KeyedCollection<string, Style>
    {
        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override string GetKeyForItem(Style item)
        {
            return item.ScopeName;
        }

        public const string Blue = "#FF0000FF";
        public const string White = "#FFFFFFFF";
        public const string Black = "#FF000000";
        public const string DullRed = "#FFA31515";
        public const string Yellow = "#FFFFFF00";
        public const string Green = "#FF008000";
        public const string PowderBlue = "#FFB0E0E6";
        public const string Teal = "#FF008080";
        public const string Gray = "#FF808080";
        public const string Navy = "#FF000080";
        public const string OrangeRed = "#FFFF4500";
        public const string Purple = "#FF800080";
        public const string Red = "#FFFF0000";
        public const string MediumTurqoise = "FF48D1CC";
        public const string Magenta = "FFFF00FF";
        public const string OliveDrab = "#FF6B8E23";
        public const string DarkOliveGreen = "#FF556B2F";
        public const string DarkCyan = "#FF008B8B";
    }
}