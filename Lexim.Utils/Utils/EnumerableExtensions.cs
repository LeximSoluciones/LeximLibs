using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexim.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T, T)> CrossProduct<T>(this IEnumerable<T> l1, IEnumerable<T> l2) =>
            from e1 in l1
            from e2 in l2
            select (e1, e2);

        public static IEnumerable<T> With<T>(this IEnumerable<T> source, params Action<T>[] actions)
        {
            foreach (var i in source)
            {
                foreach (var a in actions)
                {
                    a(i);
                }
                yield return i;
            }
        }

        public static IEnumerable<T> Index<T>(this IEnumerable<T> source, Action<T, int> indexSetter)
        {
            return
                source.Select((item, index) => item.With(x => indexSetter(x, index + 1)));
        }

        public static string StringJoin(this IEnumerable<string> source, string separator) => string.Join(separator, source);

        public static IEnumerable<List<T>> InBatchesOf<T>(this IEnumerable<T> source, int max)
        {
            var batch = new List<T>(max);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == max)
                {
                    yield return batch;
                    batch = new List<T>(max);
                }
            }
            if (batch.Any())
            {
                yield return batch;
            }
        }
    }
}