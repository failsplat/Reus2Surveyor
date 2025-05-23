using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reus2Surveyor
{
    public partial class StatCollector
    {
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class UnpackToBiomesAttribute : System.Attribute
        {
            public string Prefix { get; } = "";
            public string Suffix { get; } = "";
            public object DefaultValue { get; } = "";
            public string? NumberFormat { get; } = null;

            public UnpackToBiomesAttribute(string prefix = "", string suffix = "", object defaultValue = null, string numberFormat = null)
            {
                this.Prefix = prefix;
                this.Suffix = suffix;
                this.DefaultValue = defaultValue;
                this.NumberFormat = numberFormat;
            }
        }
    }
}
