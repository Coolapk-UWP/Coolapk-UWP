// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    [System.Diagnostics.DebuggerDisplay("U = {U} V = {V}")]
    internal struct UvMeasure
    {
        internal static readonly UvMeasure Zero = default;

        internal double U { get; set; }

        internal double V { get; set; }

        public UvMeasure(Orientation orientation, double width, double height)
        {
            if (orientation == Orientation.Horizontal)
            {
                U = width;
                V = height;
            }
            else
            {
                U = height;
                V = width;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is UvMeasure measure && (measure.U == U) && (measure.V == V);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}