using System.Collections.Generic;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class IncludedEntity
    {
        public string LogicalName { get; set; }

        public List<IncludedField> IncludedFields { get; set; } = new List<IncludedField>();
    }

    public class IncludedField
    {
        public string LogicalName { get; set; }
        public string OptionSetName { get; set; }
    }
}