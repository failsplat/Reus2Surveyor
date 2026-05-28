using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class PatchCollection
    {
        [JsonProperty(Required = Required.Always)] public ItemData<List<Id<int>>> models { get; init; }
        [JsonProperty(Required = Required.Always)] public string name { get; init; }
        [JsonProperty(Required = Required.Always)] public Parent parent { get; init; }

        public List<int> IdList { get => [.. this.models.itemData.Select(s => s.id)]; }
    }

    public class PatchMap<T> : List<T>
    {
        // Index 0 -> leftmost
        // Increasing index goes to right on planet (clockwise) 
        // TODO: Check if the assumption above is actually true
        // This class makes the list superficially behave like a circular linked list for specific methods

        public PatchMap(IEnumerable<T> values) : base(values)
        { }

        public List<T> IndexSlice(int leftIndex, int rightIndex)
        {
            if (leftIndex <= rightIndex)
            {
                return this.Slice(leftIndex, rightIndex - leftIndex + 1);
            }
            else
            {
                List<T> tailslice = this[leftIndex..this.Count()];
                List<T> headSlice = this[..(rightIndex + 1)];
                return [.. tailslice, .. headSlice];
            }
        }

        public List<T> PatchIndexSlice(T leftID, T rightID)
        {
            int leftIndex = this.IndexOf(leftID);
            int rightIndex = this.IndexOf(rightID);
            if (leftIndex == -1 || rightIndex == -1) return [];
            else return IndexSlice(leftIndex, rightIndex);
        }

    }
}
