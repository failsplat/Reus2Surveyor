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

    public class ValueList<T>
    {
        public List<Value<T>> itemData { get; init; } = [];
    }
}
