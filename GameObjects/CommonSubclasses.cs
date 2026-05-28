using System.Diagnostics;

namespace Reus2Surveyor.GameObjects
{
    [DebuggerDisplay("id: {id.ToString()}")]
    public class Id<T>
    {
        public T? id { get; init; }
    }

    [DebuggerDisplay("value: {value.ToString()}")]
    public class Value<T>
    {
        public T? value { get; init; }
    }

    public class ItemData<T>
    {
        public T itemData { get; init; }
    }

    [DebuggerDisplay("parent: {id.ToString()}")]
    public class Parent
    {
        public int? id { get; init; }
    }

    public class Items<T>
    {
        public T items { get; init; }
    }
}
