using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor
{
    public class PlacedMicro // BioticumAspect
    {
        public string? definition;
        public int? parent; // points to a BioticumAspectSlot aka MicroSlot

        public PlacedMicro(Dictionary<string, object> refDict)
        {
            this.definition = DictHelper.TryGetString(refDict, ["definition", "value"]);
			this.parent = DictHelper.TryGetInt(refDict, ["parent", "id"]);
		}
	}

    public class MicroSlot // BioticumAspectSlot
    {
        public int? aspect;
        public int? parent; // points to a NatureBioticum

        public MicroSlot(Dictionary<string, object> refDict)
        {
            this.aspect = DictHelper.TryGetInt(refDict, ["aspect", "id"]);
			this.parent = DictHelper.TryGetInt(refDict, ["parent", "id"]);
		}
	}
}
