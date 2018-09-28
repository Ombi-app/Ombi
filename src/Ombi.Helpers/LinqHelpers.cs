using System;
using System.Collections.Generic;

namespace Ombi.Helpers
{
    public static class LinqHelpers
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource source1 in source)
            {
                if (knownKeys.Add(keySelector(source1)))
                    yield return source1;
            }
        }

        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }
    }
}