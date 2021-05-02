using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EarlyXrm.EarlyBoundGenerator
{
    [ExcludeFromCodeCoverage]
    public class EarlyBoundConfig
    {
        public string ConnectionString;
        public string Namespace;
        public bool UseDisplayNames;
        public bool DebugMode;
        public bool Instrument;
        public bool AddSetters;
        public bool GenerateConstants;
        public bool NestNonGlobalEnums;
        public string Out;
        public string[] Solutions;
        public Dictionary<string, string[]> Include = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> Exclude = new Dictionary<string, string[]>();
    }
}