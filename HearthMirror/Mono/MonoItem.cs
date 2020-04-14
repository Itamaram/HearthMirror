using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HearthMirror.Mono
{
    [DebuggerDisplay("{Class.FullName}")]
    public abstract class MonoItem
    {
        protected ProcessView View { get; }
        public uint Root { get; }

        protected MonoItem(ProcessView view, uint root)
        {
            View = view;
            Root = root;
        }

        public abstract MonoClass Class { get; }

        protected abstract object GetValue(MonoClassField field);

        public object this[string key] => GetField(key);

        public object GetField(string key, params string[] chain)
        {
            return key.Append(chain)
                .Aggregate((object) this, (mi, f) => GetValue(((MonoItem) mi).Class.GetField(f)));
        }

#if DEBUG
        public Dictionary<string, object> DebugFields => Class.GetFieldsRecursively()
            .ToDictionary(f => f.Name, GetValue);
#endif
    }

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