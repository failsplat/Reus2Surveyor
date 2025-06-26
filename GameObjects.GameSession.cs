using System.Collections.Generic;
using System.Linq;

namespace Reus2Surveyor
{
    public class GameSession
    {
        // Primary Data
        // sessionSummary
        public readonly List<TurningPointPerformance> turningPointPerformances = [];
        public readonly List<string?> giantRosterDefs = [];
        public readonly List<string?> encounteredDefinitions = [];
        public readonly string? scenarioDefinition;
        public readonly string? selectedCharacterDef;
        public readonly int? finalEra;
        public readonly bool? isTimeBasedChallenge, giantsRandomized, startingSpiritRandomized;
        public readonly int? draftMode, rerollsPerEra, eventIntensity;
        public readonly int? challengeIndex, timedChallengeType, sessionDifficulty, challengeTimestamp;
        // sessionSummary:planetSummary2
        // Biome sectors for planet preview bar
        public readonly List<BiomeSector> biomeSectors = [];
        public int? terribleFate;
        // sessionSummary:humanitySummary2
        public readonly List<CivSummary> civSummaries = [];
        public readonly int? coolBiomes;

        public readonly bool? pacifismMode, planetIsLost;

        public GameSession(Dictionary<string, object> refDict)
        {
            // sessionSummary
            this.giantRosterDefs = DictHelper.TryGetStringList(refDict,
                ["sessionSummary", "startParameters", "giantRoster", "itemData"],
                ["value", "Item2", "value"]);
            this.scenarioDefinition = DictHelper.TryGetString(refDict, ["sessionSummary", "startParameters", "scenarioDefinition", "value"]);
            this.selectedCharacterDef = DictHelper.TryGetString(refDict, ["sessionSummary", "startParameters", "selectedCharacter", "value"]);
            this.finalEra = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "finalEra", "value"]);

            this.isTimeBasedChallenge = DictHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "isTimeBasedChallenge"]);
            this.giantsRandomized = DictHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "giantsRandomized"]);
            this.startingSpiritRandomized = DictHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "startingSpiritRandomized"]);
            this.draftMode = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "draftMode", "value"]);
            this.rerollsPerEra = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "draftMode", "value"]);
            this.eventIntensity = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "draftMode", "value"]);
            this.challengeIndex = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "challengeID", "challengeIndex"]);
            this.timedChallengeType = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "challengeID", "timedChallengeType", "value"]);
            this.challengeTimestamp = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "challengeID", "challengeDate"]);
            this.pacifismMode = DictHelper.TryGetBool(refDict, ["sessionSummary", "startParameters", "pacifismMode"]);
            this.sessionDifficulty = DictHelper.TryGetInt(refDict, ["sessionSummary", "startParameters", "sessionDifficulty", "value"]);
            this.planetIsLost = DictHelper.TryGetBool(refDict, "planetIsLost");
            this.coolBiomes = DictHelper.TryGetInt(refDict, ["sessionSummary", "coolBiomes"]);

            this.encounteredDefinitions = DictHelper.TryGetStringList(refDict, ["encounteredDefinitions", "itemData"], ["value"]);

            List<object> tpDicts = (List<object>)DictHelper.DigValueAtKeys(refDict, ["sessionSummary", "scoreCard", "turningPointPerformances", "itemData"]);
            if (tpDicts is not null)
            {
                foreach (Dictionary<string, object> tpd in tpDicts)
                {
                    this.turningPointPerformances.Add(new TurningPointPerformance(tpd));
                }
            }

            // planetSummary
            List<object> sectorDicts = (List<object>)DictHelper.DigValueAtKeys(refDict, ["sessionSummary", "planetSummary2", "biomeSectors", "itemData"]);
            foreach (Dictionary<string, object> sd in sectorDicts)
            {
                this.biomeSectors.Add(new BiomeSector(sd));
            }
            this.terribleFate = DictHelper.TryGetInt(refDict, ["sessionSummary", "planetSummary2", "terribleFate", "value"]);

            List<object> civDicts = (List<object>)DictHelper.DigValueAtKeys(refDict, ["sessionSummary", "humanitySummary2", "civs", "itemData"]);
            foreach (Dictionary<string, object> cd in civDicts)
            {
                this.civSummaries.Add(new CivSummary(cd));
            }

        }

        public class TurningPointPerformance
        {
            public readonly string? turningPointDef, requestingCharacterDef;
            public readonly int? starRating, scoreTotal;

            public TurningPointPerformance(Dictionary<string, object> subDict)
            {
                this.turningPointDef = DictHelper.TryGetString(subDict, ["value", "turningPoint", "value"]);
                this.requestingCharacterDef = DictHelper.TryGetString(subDict, ["value", "requestingCharacter", "value"]);
                this.starRating = DictHelper.TryGetInt(subDict, ["value", "starRating"]);
                this.scoreTotal = DictHelper.TryGetIntList(subDict, ["value", "scoreElements", "itemData"], ["value", "score"]).Sum();
            }
        }

        public class BiomeSector
        {
            public readonly int? typeDef;
            public readonly float? len;
            public readonly bool? hasCity;

            public BiomeSector(Dictionary<string, object> subDict)
            {
                this.typeDef = DictHelper.TryGetInt(subDict, ["value", "biomeType", "value"]);
                this.len = DictHelper.TryGetFloat(subDict, ["value", "sectorLength"]);
                this.hasCity = DictHelper.TryGetBool(subDict, ["value", "hasCity"]);
            }
        }

        public class CivSummary
        {
            public readonly string? name;
            public readonly int? prosperity, population, wealth, innovation;
            public readonly string? characterDef, homeBiomeDef;
            public readonly List<string?> projectDefs;

            public CivSummary(Dictionary<string, object> subDict)
            {
                this.name = DictHelper.TryGetString(subDict, ["value", "name"]);

                this.prosperity = DictHelper.TryGetInt(subDict, ["value", "prosperity"]);
                this.population = DictHelper.TryGetInt(subDict, ["value", "population"]);
                this.wealth = DictHelper.TryGetInt(subDict, ["value", "wealth"]);
                this.innovation = DictHelper.TryGetInt(subDict, ["value", "innovation"]);

                this.characterDef = DictHelper.TryGetString(subDict, ["value", "character", "value"]);
                this.homeBiomeDef = DictHelper.TryGetString(subDict, ["value", "homeBiome", "value"]);

                this.projectDefs = DictHelper.TryGetStringList(subDict,
                    ["value", "projects", "itemData"],
                    ["value", "projectDefinition", "value"]);
            }
        }
    }
}
