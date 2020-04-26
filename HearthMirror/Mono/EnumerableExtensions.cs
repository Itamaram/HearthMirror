using System;
using System.Collections.Generic;
using System.Linq;

namespace HearthMirror.Mono
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Append<T>(this T head, IEnumerable<T> tail)
        {
            yield return head;

            foreach (var t in tail)
                yield return t;
        }
    }
}