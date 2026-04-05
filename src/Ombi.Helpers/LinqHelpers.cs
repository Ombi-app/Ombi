using System;
using System.Collections.Generic;
using System.Linq;

namespace Ombi.Helpers
{
    public static class LinqHelpers
    {
        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

    }
}