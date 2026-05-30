using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class GenericBuff
    {
        public Value<string> definition { get; init; }
        public Id<int> owner { get; init; }
        public bool isOwner { get; init; }
        public string name { get; init; }
        public Parent parent { get; init; }

        public string Definition { get => this.definition.value; }
        public int Owner { get => this.owner.id; }
    }
}
