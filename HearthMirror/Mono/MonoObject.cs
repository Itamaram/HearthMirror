namespace HearthMirror.Mono
{
    public class MonoObject : MonoItem
    {
        public MonoObject(ProcessView view, uint pObject)
            : base(view, pObject)
        {
            var vtable = view.ReadUint(pObject);
            Class = new MonoClass(view, view.ReadUint(vtable));
        }

        public override MonoClass Class { get; }

        protected override object GetValue(MonoClassField field) => field.GetValue(this);
    }
}