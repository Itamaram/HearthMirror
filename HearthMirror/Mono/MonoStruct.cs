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
}