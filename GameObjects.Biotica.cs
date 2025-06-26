using System.Collections.Generic;

namespace Reus2Surveyor
{
    public class BioticumSlot
    {
        // Primary Data
        public readonly int? bioticumId, patchId, locationOnPatch, slotLevel;
        public readonly int? futureSlotId;

        public readonly bool? isInvasiveSlot;
        public readonly List<string> slotbonusDefinitions;
        public readonly List<Dictionary<string, object>> archivedBiotica;
        public readonly List<string> archivedBioticaDefs;
        public readonly string name;

        public BioticumSlot(Dictionary<string, object> refDict)
        {
            this.bioticumId = DictHelper.TryGetInt(refDict, ["bioticum", "id"]);
            this.futureSlotId = DictHelper.TryGetInt(refDict, ["futureSlot", "id"]);
            this.patchId = DictHelper.TryGetInt(refDict, ["patch", "id"]);

            // 0 = Foreground
            // 1 = Background
            // 2 = Mountain 
            this.locationOnPatch = DictHelper.TryGetInt(refDict, ["locationOnPatch", "value"]);

            this.slotbonusDefinitions = DictHelper.TryGetStringList(refDict, ["slotbonusDefinitions", "itemData"], "itemData");

            this.slotLevel = DictHelper.TryGetInt(refDict, "slotLevel");

            this.archivedBiotica = DictHelper.TryGetDictList(refDict, ["archivedBiotica", "itemData"], "value");
            this.archivedBioticaDefs = DictHelper.TryGetStringList(refDict, ["archivedBiotica", "itemData"], ["value", "bioticum", "value"]);

            this.isInvasiveSlot = DictHelper.TryGetBool(refDict, "isInvasiveSlot");

            // "BioticumSlot (<patch> - <position>)
            this.name = DictHelper.TryGetString(refDict, "name");
        }
    }

    public class NatureBioticum
    {
        // Active bioticum only!
        // Legacy biotica are archived as part of the slots.

        // Primary Data
        public readonly List<int?> aspectSlotsIds;
        public readonly int? bioticumId;
        public readonly string? definition;
        public readonly bool? receivedRiverBonus, anomalyBonusActive;
        public readonly int? slotId;
        public readonly List<string?> evolvedBiotica;

        // Secondary Data
        public bool IsOnMountain { get; private set; }
        public bool HasMicro { get; private set; }
        public string BioticumName { get; private set; }

        public NatureBioticum(Dictionary<string, object> refDict)
        {
            this.aspectSlotsIds = DictHelper.TryGetIntList(refDict, ["aspectSlots", "itemData"], "id");

            // Older saves don't have this, so it can't be used to tell if a bioticum is active
            if (refDict.ContainsKey("bioticumId"))
            {
                this.bioticumId = DictHelper.TryGetInt(refDict, "bioticumID");
            }
            else
            {

            }

            this.definition = DictHelper.TryGetString(refDict, ["definition", "value"]);

            this.receivedRiverBonus = DictHelper.TryGetBool(refDict, "receivedRiverBonus");
            this.anomalyBonusActive = DictHelper.TryGetBool(refDict, "anomalyBonusActve"); // [sic]

            this.slotId = DictHelper.TryGetInt(refDict, ["parent", "id"]);
            this.evolvedBiotica = DictHelper.TryGetStringList(refDict, ["evolvedBiotica", "itemData"], "value");
        }

        public void CheckSlotProperties(Dictionary<int, BioticumSlot> slotDict)
        {
            if (this.slotId is null || this.bioticumId is null) return;
            BioticumSlot thisSlot = slotDict[(int)this.slotId];
            this.IsOnMountain = thisSlot.locationOnPatch == 2;
        }
        // TODO: (Maybe) check that the aspect slot(s) have placed micros

        public void CheckName(Glossaries g)
        {
            this.BioticumName = g.BioticumNameFromHash(this.definition);
        }

        //public bool IsActive() { return this.bioticumId is null ? false : this.bioticumId > 0; }
    }
}
