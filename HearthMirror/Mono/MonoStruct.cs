using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HearthMirror.Mono
{
    public class MonoStruct : MonoItem
    {
        public MonoStruct(ProcessView view, MonoClass mClass, uint pStruct)
            : base(view, pStruct)
        {
            Class = mClass;
        }

        public override MonoClass Class { get; }

        protected override object GetValue(MonoClassField field)
            => field.GetValue(new MonoObject(View, Root - 8));
    }

    [DebuggerDisplay("{Class.FullName}")]
    public abstract class MonoItem
    {
        protected ProcessView View { get; }
        protected uint Root { get; }

        protected MonoItem(ProcessView view, uint root)
        {
            View = view;
            Root = root;
        }

        public abstract MonoClass Class { get; }

        protected abstract object GetValue(MonoClassField field);

        public object this[string key] => GetValue(Class.GetField(key));
    }

    public static class DeserialisationExtensions
    {
        public static object Deserialise(this object obj, Type type)
        {
            if (type.IsPrimitive)
                return obj;

            if (type.IsArray && type.GetElementType().IsPrimitive)
            {
                // todo confirm if this is needed, or if I can pass thru
                var src = (object[])obj;
                var dst = Array.CreateInstance(type.GetElementType(), src.Length);
                Array.Copy(src, dst, src.Length);
                return dst;
            }

            if (type.IsArray)
            {
                var src = (object[])obj;
                var dst = Array.CreateInstance(type.GetElementType(), src.Length);

                for (var i = 0; i < src.Length; i++)
                    dst.SetValue(Deserialise(src[i], type.GetElementType()), i);

                return dst;
            }

            if (type.IsEnum)
                return Enum.ToObject(type, (int)obj);

            var mi = (MonoItem)obj;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var items = (object[])mi["_items"];
                var size = (int)mi["_size"];

                var list = Activator.CreateInstance(type);
                var add = typeof(List<>).GetMethod(nameof(List<object>.Add));

                var gt = type.GenericTypeArguments[0];

                for (var i = 0; i < size; i++)
                    add.Invoke(list, new[] { items[i].Deserialise(gt) });

                return list;
            }

            //todo dictionary

            var result = Activator.CreateInstance(type);

            foreach (var prop in type.GetProperties())
                prop.SetValue(result, Deserialise(mi[prop.Name], prop.PropertyType));

            return result;
        }

        public static T Deserialise<T>(this MonoItem item) => (T)item.Deserialise(typeof(T));
    }
}