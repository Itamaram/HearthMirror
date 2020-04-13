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

        public dynamic this[string key] => GetValue(Class.GetField(key));

#if DEBUG
        public Dictionary<string, object> DebugFields => Class.GetFieldsRecursively()
            .ToDictionary(f => f.Name, GetValue);
#endif
    }
}