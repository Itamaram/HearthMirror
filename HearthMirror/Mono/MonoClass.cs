﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HearthMirror.Mono
{
    [DebuggerDisplay("{FullName}")]
    public class MonoClass
    {
        //Hack to prevent leak
        private const int MAX_FIELDS_HACK = 1000;

        private readonly uint _pClass;
        private readonly ProcessView _view;

        public MonoClass(ProcessView view, uint pClass)
        {
            _view = view;
            _pClass = pClass;
        }

        public string Name => _view.ReadCString(_view.ReadUint(_pClass + Offsets.MonoClass_name));

        public string NameSpace => _view.ReadCString(_view.ReadUint(_pClass + Offsets.MonoClass_name_space));

        public string FullName
        {
            get
            {
                var name = Name;
                var ns = NameSpace;
                var nestedIn = NestedIn;
                while (nestedIn != null)
                {
                    name = nestedIn.Name + "+" + name;
                    ns = nestedIn.NameSpace;
                    nestedIn = nestedIn.NestedIn;
                }
                return ns.Length == 0 ? name : ns + "." + name;
            }
        }

        public uint VTable
        {
            get
            {
                var rti = _view.ReadUint(_pClass + Offsets.MonoClass_runtime_info);
                return _view.ReadUint(rti + Offsets.MonoClassRuntimeInfo_domain_vtables);
            }
        }

        public bool IsValueType => 0 != (_view.ReadUint(_pClass + Offsets.MonoClass_bitfields) & 4);

        public bool IsEnum => 0 != (_view.ReadUint(_pClass + Offsets.MonoClass_bitfields) & 0x8);

        public int Size => _view.ReadInt(_pClass + Offsets.MonoClass_sizes);

        public MonoClass Parent
        {
            get
            {
                var pParent = _view.ReadUint(_pClass + Offsets.MonoClass_parent);
                return pParent == 0 ? null : new MonoClass(_view, pParent);
            }
        }

        public MonoClass NestedIn
        {
            get
            {
                var pNestedIn = _view.ReadUint(_pClass + Offsets.MonoClass_nested_in);
                return pNestedIn == 0 ? null : new MonoClass(_view, pNestedIn);
            }
        }

        public MonoType ByvalArg => new MonoType(_view, _pClass + Offsets.MonoClass_byval_arg);

        public int NumFields
        {
            get
            {
                switch (FullName)
                {
                    case "System.Collections.Generic.List`1":
                        return 6;
                    case "System.Collections.Generic.Dictionary`2":
                        return 13;
                    case "System.Collections.Generic.Dictionary`2+Entry":
                        return 4;
                    default:
                        return Math.Min(MAX_FIELDS_HACK, _view.ReadInt(_pClass + Offsets.MonoClass_field_count));
                }
            }
        }

        public IEnumerable<MonoClassField> GetFields()
        {
            var pFields = _view.ReadUint(_pClass + Offsets.MonoClass_fields);

            return Enumerable.Range(0, NumFields)
                .Select(i => new MonoClassField(_view, pFields + (uint)i * Offsets.MonoClassField_sizeof))
                .Concat(Parent != null ? Parent.GetFields() : Enumerable.Empty<MonoClassField>());
        }
        
        public dynamic this[string key] => GetField(key)?.StaticValue;

        // todo remove ordefault
        public MonoClassField GetField(string key) => GetFields().FirstOrDefault(f => f.Name == key); 

#if DEBUG
        public Dictionary<string, object> DebugFields => GetFields()
            .ToDictionary(f => f.Name, f => f.StaticValue);
#endif
    }
}
