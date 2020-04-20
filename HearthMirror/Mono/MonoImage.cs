﻿using System.Collections.Generic;
using System.Linq;
using HearthMirror.Proxy;

namespace HearthMirror.Mono
{
    public class MonoImage
    {
        private readonly IReadOnlyDictionary<string, MonoClass> classes;

        public MonoImage(ProcessView view, uint pImage)
        {
            classes = BuildClassDictionary(view, pImage);
        }

        // todo get name from typeof(T).Name - "Class"
        public T GetClass<T>(string name) where T : class => Generator.GetClass<T>(classes[name]);

        public MonoClass this[string key] => classes[key];

        private static Dictionary<string, MonoClass> BuildClassDictionary(ProcessView view, uint root)
        {
            var ht = root + Offsets.MonoImage_class_cache;
            var size = view.ReadUint(ht + Offsets.MonoInternalHashTable_size);
            var table = view.ReadUint(ht + Offsets.MonoInternalHashTable_table);
            
            return Enumerable.Range(0, (int) size)
                .SelectMany(i => ClassCacheChain(view, view.ReadUint(table + 4 * i)))
                .ToDictionary(mc => mc.FullName);
        }

        private static IEnumerable<MonoClass> ClassCacheChain(ProcessView view, uint start)
        {
            for (var p = start; p != 0; p = view.ReadUint(p + Offsets.MonoClass_next_class_cache))
                yield return new MonoClass(view, p);
        }
    }
}