// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ColorCode.Common;
using System;
using System.Collections.Generic;

namespace ColorCode.Parsing
{
    public class Scope
    {
        public Scope(string name,
                     int index,
                     int length)
        {
            Guard.ArgNotNullAndNotEmpty(name, "name");

            Name = name;
            Index = index;
            Length = length;
            Children = new List<Scope>();
        }

        public IList<Scope> Children { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }
        public Scope Parent { get; set; }
        public string Name { get; set; }

        public void AddChild(Scope childScope)
        {
            if (childScope.Parent != null)
            {
                throw new InvalidOperationException("The child scope already has a parent.");
            }

            childScope.Parent = this;

            Children.Add(childScope);
        }
    }
}