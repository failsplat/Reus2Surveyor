using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
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
        public List<TurningPointPerformance> EraPerformance 
        { 
            get => [.. this.sessionSummary.scoreCard.turningPointPerformances.itemData.Select(v => v.value)];
        }
        public List<CivSummary> CivSummaries
        {
            get => [.. this.sessionSummary.humanitySummary2.civs.itemData.Select(v => v.value)];
        }
        public HashSet<string> EncounteredDefinitions
        {
            get => [.. this.encounteredDefinitions.itemData.Select(v => v.value)];
        }
        public string SelectedCharacter
        {
            get => this.startParameters.selectedCharacter.value;
        }
        public List<string> SelectedGiantDefinitions
        {
            get => [.. this.startParameters.giantRoster.itemData.Select(v => (v.value.Item1.value, v.value.Item2.value)).OrderBy(t => t.Item1).Select(t => t.Item2)];
        }
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
        }
    }

    public class SessionSummary
    {
        public ItemData<List<Value<GiantType>>> giantRoster { get; init; }
        public ScoreCard scoreCard { get; init; }
        public ItemData<List<Value<EraSummary>>> eraSummaries { get; init; }
        public bool advanced { get; init; }
        public int coolBiomes { get; init; }
        public HumanitySummary humanitySummary2 { get; init; }
        public ItemData<List<Value<TopBioticumSummary>>> topBiotica { get; init; }

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
            public Value<int> era;
            public Value<int> rank;
        }

        public class HumanitySummary
        {
            public ItemData<List<Value<CivSummary>>> civs { get; init; }
        }

        public class TopBioticumSummary
        {
            public Value<string> bioticumType { get; init; }
            public double food { get; init; }
            public double valuables { get; init; }
            public double curio { get; init; }
            public double mystery { get; init; }
            public ItemData<List<Value<string>>> aspects { get; init; }
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
