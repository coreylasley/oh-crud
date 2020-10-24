using static Codeterpret.Common.Enums;

namespace Codeterpret.SQL
{
    public class SQLServerConstraint : Common.Enums
    {      

        public string ConstraintName { get; set; }
        public string ColumnName { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsUnique { get; set; }
        public ClusterTypes ClusterType { get; set; }

        public SQLServerConstraint()
        {
            ConstraintName = "";
            ColumnName = "";
            IsPrimary = false;
            IsUnique = false;
            ClusterType = ClusterTypes.Unknown;
        }
    }

}
