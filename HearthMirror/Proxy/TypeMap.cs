using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HearthMirror.Mono;

namespace HearthMirror.Proxy
{
    public static class TypeMap
    {
        private const string Namespace = "UnityModels";

        private static readonly IReadOnlyDictionary<string, Type> Types;

        static TypeMap()
        {
            // TODO find a better way of doing that. Perhaps rescan on a failure case below?
            var loaded = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
                .Select(a => a.Location).ToList();
            
            var missing = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Where(f => !loaded.Contains(f));

            foreach (var file in missing)
                AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(file));

            Types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.FullName.StartsWith(Namespace))
                .ToDictionary(t => t.FullName.Substring(Namespace.Length + 1));

        }

        public static Type GetInternalType(this MonoItem mi) =>
            Types.TryGetValue(mi.Class.FullName, out var type)
                ? type
                : throw new Exception($"External type {mi.Class.FullName} was not mapped internally");
    }
}