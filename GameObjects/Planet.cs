using DocumentFormat.OpenXml.Office.Drawing;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Reus2Surveyor.GameObjects
{
    // This is not deserialized directly
    // Instead, it is assembled by reading SaveRoot.referenceTokens 
    public class Planet
    {
        public Dictionary<int, BioticumSlot> BioticumSlots = [];


        public Planet(SaveRoot sr, string path)
        {
            int i = -1;
            foreach (JToken jo in sr.referenceTokens) 
            {
                i++;
                Dummy dummy = jo.ToObject<Dummy>();
                if (dummy.name?.StartsWith("BioticumSlot") ?? false)
                {
                    BioticumSlot slot = jo.ToObject<BioticumSlot>();
                    this.BioticumSlots.Add(i, slot);
                    continue;
                }
            }
        }
    }
}
