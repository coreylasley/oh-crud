using System.Linq;
using static Codeterpret.Common.Common;

namespace Codeterpret.SQL
{
    public class SQLColumn : Common.Common
    {
        public string Name { get; set; }
        public string SQLType { get; set; }
        public string ForeignKeyType { get; set; }
        public ForeignKey ForeignKey { get; set; }
        public string Comment { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public string ConstraintName { get; set; }
        public string Size { get; set; }
        public ClusterTypes ClusterType { get; set; }

        public string ClusterTypeName { get {

                string ret = "";

                switch(ClusterType)
                {
                    case ClusterTypes.Clustered:
                        ret = "CLUSTERED";
                        break;
                    case ClusterTypes.NonClustered:
                        ret = "NONCLUSTERED";
                        break;
                }

                return ret;
            }
        }

        /// <summary>
        /// Translates the SQL Type to the C# Type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public string CSharpType(DatabaseTypes dbType)
        {

            string ret = "";

            if (dbType == DatabaseTypes.SQLServer)
            {
                switch (SQLType.ToLower().Trim())
                {
                    case "int": ret = "int"; break;
                    case "tinyint": ret = "int"; break;
                    case "datetime": ret = "DateTime"; break;
                    case "varchar": ret = "string"; break;
                    case "nvarchar": ret = "string"; break;
                    case "bit": ret = "bool"; break;
                    case "text": ret = "string"; break;
                    case "ntext": ret = "string"; break;
                    case "char": ret = "string"; break;
                    case "nchar": ret = "string"; break;
                    case "decimal": ret = "decimal"; break;
                    case "uniqueidentifier": ret = "string"; break;
                    case "image": ret = "string"; break;
                    case "": ret = ForeignKeyType; break;
                    default: ret = "string"; break;
                }
            }

            if (dbType == DatabaseTypes.MySQL)
            {
                switch (SQLType.ToLower().Trim())
                {
                    case "int": ret = "int"; break;
                    case "tinyint": ret = "int"; break;
                    case "bigint": ret = "long"; break;
                    case "datetime": ret = "DateTime"; break;
                    case "varchar": ret = "string"; break;
                    case "bit": ret = "bool"; break;
                    case "text": ret = "string"; break;
                    case "longtext": ret = "string"; break;
                    case "char": ret = "string"; break;
                    case "decimal": ret = "decimal"; break;
                    case "uniqueidentifier": ret = "string"; break;
                    case "longblob": ret = "string"; break;
                    case "": ret = ForeignKeyType; break;
                    default: ret = "string"; break;
                }
            }

            if (IsNullable) ret += "?";

            return ret;
        }

        /// <summary>
        /// Translates the SQL Type to the TypeScript Type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public string TypeScriptType(DatabaseTypes dbType)
        {
            string ret = "";

            if (dbType == DatabaseTypes.SQLServer)
            {
                switch (SQLType.ToLower().Trim())
                {
                    case "int": ret = "number"; break;
                    case "tinyint": ret = "number"; break;
                    case "datetime": ret = "string"; break;
                    case "varchar": ret = "string"; break;
                    case "nvarchar": ret = "string"; break;
                    case "bit": ret = "boolean"; break;
                    case "text": ret = "string"; break;
                    case "ntext": ret = "string"; break;
                    case "char": ret = "string"; break;
                    case "nchar": ret = "string"; break;
                    case "decimal": ret = "number"; break;
                    case "uniqueidentifier": ret = "string"; break;
                    case "image": ret = "string"; break;
                    case "": ret = ForeignKeyType; break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Translates the SQL Type to the JavaScript Type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public string JavaScriptType(DatabaseTypes dbType)
        {
            return "var";
        }

        /// <summary>
        /// Changes a common SQL Server Types string to MySQL Type string if applicable
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static string SQLServerTypeToMySQL(string typeName)
        {
            string ret = typeName;

            switch(typeName.ToLower().Trim())
            {
                case "tinyint": ret = "int"; break;
                case "smalldatetime": ret = "datetime"; break;
                case "time(7)": ret = "time(6)"; break;
                case "nvarchar": ret = "varchar"; break;
                case "ntext": ret = "text"; break;
                case "nchar": ret = "char"; break;
                case "varchar(max)": ret = "text"; break;
                case "nvarchar(max)": ret = "text"; break;
                case "varbinary(max)": ret = "blob"; break;
                case "nvarbinary(max)": ret = "blob"; break;
                case "uniqueidentifier": ret = "char(32)"; break;
                case "image": ret = "blob"; break;
            }

            return ret;
        }

        public SQLColumn() {
            Name = "";
            SQLType = "";
            Comment = "";
            ForeignKeyType = "";
            ForeignKey = null;
            IsNullable = false;
            IsIdentity = false;
            IsPrimaryKey = false;
            Size = "";
            ConstraintName = "";
            ClusterType = SQLServerConstraint.ClusterTypes.Unknown;
        }
             

        /// <summary>
        /// Parses and Extracts a SQLColumn Object from a line if applicable
        /// </summary>
        /// <param name="SQLLine"></param>
        /// <param name="dbType"></param>
        public SQLColumn(string SQLLine, DatabaseTypes dbType)
        {
            Name = "";
            SQLType = "";
            Comment = "";
            string cleanLine = "";
            string ocleanLine = "";
            string[] parts = null;

            // Based on the Database type, we will need to parse the line differently...
            switch (dbType)
            {
                case DatabaseTypes.SQLServer:

                    if (SQLLine.Contains("IDENTITY("))
                    {
                        IsIdentity = true;
                        Comment = Comment.CommaAppend("This is an identity column");
                    }

                    // Based on SQL Server Generated Table Script patterns, cleanup the line (I assign into a variable so I can better debug it)
                    cleanLine = SQLLine.Trim();
                    ocleanLine = cleanLine;

                    // Look for the first [...] which should contain the column name
                    if (cleanLine.IndexOf("[") == 0)
                    {
                        int close = cleanLine.IndexOf("]");
                        if (close > 0)
                        {
                            // Change SPACE characters into DASH characters only within this section
                            cleanLine = cleanLine.ReplaceBetween(' ', '-', 0, close);
                        }
                    }

                    if (cleanLine != ocleanLine) Comment = Comment.CommaAppend("Name contains spaces in the database");
                                            
                    cleanLine = cleanLine.Replace("] [", " ").Replace("[", " ").Replace("]", " ").Replace("  ", " ").Trim();
                   
                    // Split the line into an array 
                    parts = cleanLine.Split(' ');
                    // We should have more than 2 elements in the array
                    if (parts.Count() > 2)
                    {
                        // The first is the Column Name, and the second is the Type
                        Name = parts[0].Replace("-","");
                        SQLType = parts[1];
                        if (parts[2].Contains("("))
                        {
                            Size = parts[2];
                            // If the Size ends with a comma, then the spit several lines back split the precision into the next array element
                            if (Size.EndsWith(","))
                            {
                                Size += parts[3];
                            }
                        }

                        // The last part should be the nullable definer
                        if (parts[parts.Count()-1].Trim().ToUpper().Replace(",","") == "NULL")
                        {
                            IsNullable = true;
                            if (parts[parts.Count() - 2].Trim().ToUpper() == "NOT")
                                IsNullable = false;
                        }
                    }
                    break;

                case DatabaseTypes.MySQL:
                    if (SQLLine.Contains("AUTO_INCREMENT"))
                    {
                        IsIdentity = true;
                        Comment = Comment.CommaAppend("This is an identity column");
                    }
                    if (SQLLine.Contains(" UNIQUE "))
                    {
                        Comment = Comment.CommaAppend("This is an unique value column");
                        IsUnique = true;
                    }

                    // Based on MySQL Generated Table Script patterns, cleanup the line (I assign into a variable so I can better debug it)
                    cleanLine = SQLLine.Trim().Replace("`", "");
                    // Split the line into an array 
                    parts = cleanLine.Split(' ');
                    // We should have more than 2 elements in the array
                    if (parts.Count() > 2)
                    {
                        // The first element is the Column Name
                        Name = parts[0];
                        // The second element contains the Type, but we need to split it from it dimension
                        string[] parts2 = parts[1].Split('(');
                        if (parts2.Count() > 0)
                        {
                            // the first element contains the Type Name
                            SQLType = parts2[0].Trim();
                            if (parts2.Count() > 1)
                            {
                                Size = "(" + parts2[1].Trim();
                            }
                        }

                        // The last part should be the nullable definer
                        if (parts[parts.Count() - 1].Trim().ToUpper().Replace(",", "") == "NULL")
                        {
                            IsNullable = true;
                            if (parts[parts.Count() - 2].Trim().ToUpper() == "NOT")
                                IsNullable = false;
                        }
                    }
                    break;
            }
            
        }
        
        
    }

}
