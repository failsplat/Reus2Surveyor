using System.Collections.Generic;

namespace Reus2Surveyor
{
    public class GenericBuff
    {
        public readonly string definition;
        public readonly int? owner;
        public readonly bool? isActive;
        public readonly string name;

        public GenericBuff(Dictionary<string, object> subDict)
        {
            this.definition = DictHelper.TryGetString(subDict, ["definition", "value"]);
            this.owner = DictHelper.TryGetInt(subDict, ["owner", "id"]);
            this.isActive = DictHelper.TryGetBool(subDict, "isActive");
            this.name = DictHelper.TryGetString(subDict, "name");
        }
    }
}
