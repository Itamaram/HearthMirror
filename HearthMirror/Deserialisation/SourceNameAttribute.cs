using System;

namespace HearthMirror.Deserialisation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SourceNameAttribute : Attribute
    {
        public string Name { get; }

        public SourceNameAttribute(string name)
        {
            Name = name;
        }
    }
}