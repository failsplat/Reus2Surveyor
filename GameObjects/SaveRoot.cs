using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor.GameObjects
{
    public class SaveRoot
    {
        public int saveVersion { get; set; }
        public Id<int> compositioRoot { get; set; }
        public List<JToken> referenceTokens { get; set; }
    }
}
