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
            public bool NullOnZeroOrBlank = true;

            public UnpackToBiomesAttribute(object defaultValue, string prefix = "", string suffix = "",
                                           string numberFormat = null, bool nullOnZeroOrBlank = true)
            {
                this.Prefix = prefix;
                this.Suffix = suffix;
                this.DefaultValue = defaultValue;
                this.NumberFormat = numberFormat;
                NullOnZeroOrBlank = nullOnZeroOrBlank;
            }
        }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class UnpackToSpiritsAttribute : System.Attribute
        {
            public string Prefix { get; } = "";
            public string Suffix { get; } = "";
            public object DefaultValue { get; } = "";
            public string? NumberFormat { get; } = null;
            public bool NullOnZeroOrBlank = true;

            public UnpackToSpiritsAttribute(object defaultValue, string prefix = "", string suffix = "",
                                           string numberFormat = null, bool nullOnZeroOrBlank = true)
            {
                this.Prefix = prefix;
                this.Suffix = suffix;
                this.DefaultValue = defaultValue;
                this.NumberFormat = numberFormat;
                NullOnZeroOrBlank = nullOnZeroOrBlank;
            }
        }
    }
}
