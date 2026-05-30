using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class GameplayController
    {
        public ItemData<List<Value<string>>> masteredBiotica { get; init; }
        public HashSet<string> MasteredBiotica { get => [..this.masteredBiotica.itemData.Select(v => v.value)]; }
    }
}
