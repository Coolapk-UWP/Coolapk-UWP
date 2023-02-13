// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace ColorCode.Common
{
    public static class Guard
    {
        public static void ArgNotNull(object arg, string paramName)
        {
            if (arg == null)
                throw new ArgumentNullException(paramName);
        }

        public static void ArgNotNullAndNotEmpty(string arg, string paramName)
        {
            if (arg == null)
                throw new ArgumentNullException(paramName);

            if (string.IsNullOrEmpty(arg))
                throw new ArgumentException(string.Format("The {0} argument value must not be empty.", paramName), paramName);
        }

        public static void EnsureParameterIsNotNullAndNotEmpty<TKey, TValue>(IDictionary<TKey, TValue> parameter, string parameterName)
        {
            if (parameter == null || parameter.Count == 0)
                throw new ArgumentNullException(parameterName);
        }

        public static void ArgNotNullAndNotEmpty<T>(IList<T> arg, string paramName)
        {
            if (arg == null)
                throw new ArgumentNullException(paramName);

            if (arg.Count == 0)
                throw new ArgumentException(string.Format("The {0} argument value must not be empty.", paramName), paramName);
        }
    }
}
