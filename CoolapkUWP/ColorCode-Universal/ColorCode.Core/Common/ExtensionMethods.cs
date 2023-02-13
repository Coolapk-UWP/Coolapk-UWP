// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace ColorCode.Common
{
    public static class ExtensionMethods
    {
        public static void SortStable<T>(this IList<T> list,
                                         Comparison<T> comparison)
        {
            Guard.ArgNotNull(list, "list");

            int count = list.Count;

            for (int j = 1; j < count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }

                list[i + 1] = key;
            }
        }
    }
}