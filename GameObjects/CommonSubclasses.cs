using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class Id<T>
    {
        public T? id { get; init; }
    }

    public class Value<T>
    {
        public T? value { get; init; }
    }

    public class ValueItemDataList<T>
    {
        public List<Value<T>> itemData { get; init; } = [];
    }

    public class IdItemDataList<T>
    {
        public List<Id<T>> itemData { get; init; } = [];
    }

    public class Parent
    {
        public int? id { get; init; }
    }
}
