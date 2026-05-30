using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    // Avoid direct access to the deser fields deep than this
    public class GameSession
    {
        public RootRandom rootRandom { get; init; }
        public StartParameters startParameters { get; init; }
        public string sessionID { get; init; }
        public bool isFinished { get; init; }
        public bool freePlay { get; init; }
        public ItemData<List<Value<string>>> encounteredDefinitions { get; init; }
        public bool planetIsLost { get; init; }
        public SessionSummary sessionSummary { get; init; }

        //
        public List<TurningPointPerformance> EraPerformances 
        { 
            get => this.sessionSummary.TurningPointPerformances;
        }
        public List<CivSummary> CivSummaries
        {
            get => this.sessionSummary.CivSummaries;
        }
        public HashSet<string> EncounteredDefinitions
        {
            get => [.. this.encounteredDefinitions.itemData.Select(v => v.value)];
        }
        public List<SessionSummary.TopBioticumSummary> TopBioticaSummaries
        {
            get => this.sessionSummary.TopBioticumSummaries;
        }
        public StartParameters StartParameters { get => this.startParameters; }
        public int CoolBiomeCount { get => this.sessionSummary.coolBiomes; }
        public StartParameters.ChallengeID ChallengeInfo { get => this.StartParameters.challengeID; }
    }

    public class StartParameters
    {
        // Wow that's a nested type
        // Digging through keys is probably easier for things like this
        // This cliss needs a lot of shortcut/helper properties
        public GiantRoster giantRoster { get; init; }
        public Value<string> scenarioDefinition { get; init; }
        public Value<string> selectedCharacter { get; init; }
        public Value<int> finalEra { get; init; }
        public Value<int> playMode { get; init; }
        public bool isTimedBasedChallenge { get; init; }
        public bool noLimit { get; init; }
        public bool? giantsRandomized { get; init; }
        public bool startingSpiritRandomized { get; init; }
        public Value<int> draftMode { get; init; }
        public Value<int> horizonMode { get; init; }
        public Value<int> maxDraftsPerBiome { get; init; }
        public Value<int> rerollsPerEra { get; init; }
        public Value<int>? eventIntensity { get; init; }
        public ChallengeID challengeID { get; init; }
        public bool pacifismMode { get; init; }
        public ItemData<List<Value<N2ItemValue<int, bool>>>> isRoleRandomized { get; init; }
        public Value<int> sessionDifficulty { get; init; }

        public class ChallengeID
        {
            public int version { get; init; }
            public int challengeIndex { get; init; }
            public Value<int> timedChallengeType { get; init; }
            public long challengeDate { get; init; }
            public int ChallengeType { get => this.timedChallengeType.value; }
        }

        public int Difficulty { get => this.sessionDifficulty.value; }
        public string SelectedCharacter
        {
            get => this.selectedCharacter.value;
        }
        public List<string> SelectedGiantDefinitions
        {
            get => [.. this.giantRoster.itemData.Select(v => (v.value.Item1.value, v.value.Item2.value)).OrderBy(t => t.Item1).Select(t => t.Item2)];
        }

        public ChallengeID ChallengeInfo { get => this.challengeID; }
    }

    public class SessionSummary
    {
        public ItemData<List<Value<GiantType>>> giantRoster { get; init; }
        public ScoreCard scoreCard { get; init; }
        public ItemData<List<Value<EraSummary>>> eraSummaries { get; init; }
        public bool advanced { get; init; }
        public int coolBiomes { get; init; }
        public HumanitySummary humanitySummary2 { get; init; }
        public ItemData<List<Value<TopBioticumSummary>>>? topBiotica { get; init; }

        public List<TurningPointPerformance> TurningPointPerformances { get => [..this.scoreCard.turningPointPerformances.itemData.Select(i => i.value)]; }
        public List<CivSummary> CivSummaries { get => this.humanitySummary2.CivSummaries; }
        public List<TopBioticumSummary> TopBioticumSummaries { get => [..this.topBiotica?.itemData.Select(i => i.value) ?? []]; }

        public class ScoreCard
        {
            public ItemData<List<Value<TurningPointPerformance>>> turningPointPerformances { get; init; }
        }

        public class GiantType
        {
            public Value<string> giantType { get; init; }
        }

        public class EraSummary
        {
            public double score { get; init; }
            public Value<int> era { get; init; }
            public Value<int> rank { get; init; }
        }

        public class HumanitySummary
        {
            public ItemData<List<Value<CivSummary>>> civs { get; init; }
            public List<CivSummary> CivSummaries { get => [..this.civs.itemData.Select(i => i.value)]; }
        }

        public class TopBioticumSummary
        {
            public Value<string> bioticumType { get; init; }
            public double food { get; init; }
            public double valuables { get; init; }
            public double curio { get; init; }
            public double mystery { get; init; }
            public ItemData<List<Value<string>>> aspects { get; init; }

            public string BioticumType { get => this.bioticumType.value; }
            public double TotalValue { get => this.food + this.valuables + this.curio + (this.mystery * 5); }
            public List<string> Aspects { get => [..this.aspects.itemData.Select(a => a.value)]; }
        }
    }

    public class RootRandom
    {
        public int seedState { get; init; }
        public double pulls { get; init; }
        public int baseSeedState { get; init; }
    }

    public class GiantRoster
    {
        // Item1 position
        // Item2 definition
        public List<Value<N2ItemValue<int, string>>> itemData { get; init; }
    }

    public class TurningPointPerformance
    {
        public Value<string> turningPoint { get; init; }
        public Value<string> requestingCharacter { get; init; }
        public int starRating { get; init; }
        public ItemData<List<Value<ScoreElement>>> scoreElements { get; init; }

        public class ScoreElement
        {
            public int score { get; init; }
        }

        public int TotalScore
        {
            get
            {
                int total = 0;
                foreach (ScoreElement se in this.scoreElements.itemData.Select(i => i.value))
                {
                    total += se.score;
                }
                return total;
            }
        }

        public string Definition { get => this.turningPoint.value; }

    }

    public class CivSummary
    {
        public string name { get; init; }
        public int prosperity { get; init; }
        public int population { get; init; }
        public int wealth { get; init; }
        public int innovation { get; init; }
        public Value<string> character { get; init; }
        public Value<string> homeBiome { get; init; }
    }
}
