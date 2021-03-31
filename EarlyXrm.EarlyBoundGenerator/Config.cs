using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class EarlyBoundConfig
    {
        public string ConnectionString;
        public string Namespace;
        public bool UseDisplayNames;
        public bool DebugMode;
        public bool Instrument;
        public string[] Solutions;
        public Dictionary<string, string[]> Include = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> Exclude = new Dictionary<string, string[]>();
    }
}