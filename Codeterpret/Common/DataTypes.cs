using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeterpret.Common
{
    public class DataTypeCollection
    {
        public List<DataType> DataTypes { get; set; }

        public DataTypeCollection()
        {
            DataTypes.Add(new DataType("DATE", "DATE", "DATE", "DATE"));
            DataTypes.Add(new DataType("DATETIME", "DATETIME", "DATETIME", "TIMESTAMP(3)"));
        }
    }

    public class DataType
    {
        public string BaseType { get; set; }
        public string SQLServer { get; set; }
        public string MySQL { get; set; }
        public string PostgreSQL { get; set; }

        public DataType(string baseType, string sqlServer, string mySQL, string postgreSQL)
        {
            BaseType = baseType;
            SQLServer = sqlServer;
            MySQL = mySQL;
            PostgreSQL = postgreSQL;
        }
    }
}
