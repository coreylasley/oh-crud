using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;


namespace Codeterpret.SQL
{
    /// <summary>
    /// Parses and stores a Table structure from a SQL create script block. Also used to generate a language specific Model Class
    /// </summary>
    public class SQLTable : Common.Enums
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public List<SQLColumn> SQLColumns { get; }

        // Properties used to flag table for code generation requirements
        public bool GenerateCreate { get; set; }
        public bool GenerateRead { get; set; }
        public bool GenerateUpdate { get; set; }
        public bool GenerateDelete { get; set; }

        public bool IncludeThisTable { 
        
            get
            {
                return GenerateCreate || GenerateRead || GenerateUpdate || GenerateDelete;
            }
        
        }


        /// <summary>
        /// Return all Foreign Keys associated to this SQLTable
        /// </summary>
        public List<ForeignKey> ForeignKeys {

            get
            {
                List<ForeignKey> ret = new List<ForeignKey>();

                foreach(SQLColumn sc in SQLColumns.Where(x => x.ForeignKey != null))
                {
                    ret.Add(sc.ForeignKey);
                }

                return ret;
            }
        }

        public SQLTable() {
            SQLColumns = new List<SQLColumn>();

        }

        /// <summary>
        /// Parses and Extracts a SQLTable Object from a SQLBlock
        /// </summary>
        /// <param name="block">Block of SQL Code</param>
        /// <param name="dbType">Supported Database Type</param>
        public SQLTable(SQLBlock block, DatabaseTypes dbType)
        {
            Name = "";
            SQLColumns = new List<SQLColumn>();

            // Split the block into an array of strings (representing each line)
            string[] lines = block.Text.Split('\n');
            bool allDone = false;
            int index = -1;
            
            // Loop through each line in the array
            foreach (string l in lines)
            {
                index++;

                // Determine if we should escape out of the loop (save some time)
                if ((l.Contains("CONSTRAINT ") || l.Contains("PRIMARY KEY NONCLUSTERED") && dbType == DatabaseTypes.SQLServer) ||
                    (l.IndexOf("  `") != 0 && Name != "" && dbType == DatabaseTypes.MySQL))
                {
                    switch (dbType)
                    {
                        case DatabaseTypes.SQLServer:
                            // Grab the remaining lines and put them back together
                            StringBuilder remainingLines = new StringBuilder();
                            for (int x = index; x < lines.Length; x++)
                            {
                                remainingLines.AppendLine(lines[x]);
                            }
                            // Extract the Constraints from this last part of the block
                            List<SQLServerConstraint> sscs = ExtractSQLServerKeys(remainingLines.ToString());

                            // Loop through each of the Constraints...
                            foreach (SQLServerConstraint ssc in sscs)
                            {
                                // Loop through each of the SQL Columns
                                foreach (SQLColumn sc in SQLColumns)
                                {
                                    // If we have a match on the Column Name
                                    if (sc.Name == ssc.ColumnName)
                                    {
                                        // Set the additional constraint infor on the Column
                                        sc.IsPrimaryKey = ssc.IsPrimary;
                                        sc.IsUnique = ssc.IsUnique;
                                        sc.ConstraintName = ssc.ConstraintName;
                                        sc.ClusterType = ssc.ClusterType;

                                        break;
                                    }
                                }
                            }
                            break;

                        case DatabaseTypes.MySQL:
                            if (l.Contains("PRIMARY KEY"))
                            {
                                string[] pks = l.ReplaceEach("PRIMARY KEY|(|)|`", "").Split(',');
                                foreach(string p in pks)
                                {
                                    // Loop through each of the SQL Columns
                                    foreach (SQLColumn sc in SQLColumns)
                                    {
                                        // If we have a match on the Column Name
                                        if (sc.Name == p)
                                        {                                            
                                            sc.IsPrimaryKey = true;                                            
                                            break;
                                        }
                                    }
                                }
                            }

                            break;
                    }

                    break;
                }                  

                // Based on the Database Type we will need to analyze and parse the line differently...
                switch (dbType)
                {
                    case DatabaseTypes.SQLServer:

                        // If the line is a CREATE TABLE line...
                        if (l.Contains("CREATE TABLE "))
                        {
                            // Split the line into an array with the SPACE character as the delimiter
                            string[] dparts = l.Split(' ');

                            // Only if we have at least 3 items in the array
                            if (dparts.Length >= 3)
                            {
                                // The table name should be in item 2 
                                string tableName = dparts[2];
                                
                                // Because it likely contains a schema prefix, lets break this up into an array
                                string[] tparts = tableName.Replace("].[", " ").ReplaceEach("[|]|(", "").Split(' ');

                                // If we have 2 items...
                                if (tparts.Length == 2)
                                {
                                    // The first item will represent the schema
                                    Schema = tparts[0].Trim();
                                    // The second item will represent the actual table name
                                    Name = tparts[1].Trim();
                                }
                                else // If we only have 1 item, there must not be a schema named
                                {
                                    Schema = "";
                                    Name = tparts[0].Trim();
                                }
                            }
                        }
                        else // If this is not a CREATE TABLE line
                        {
                            // Create a new SQLColumn
                            SQLColumn sc = new SQLColumn(l, dbType);

                            // If we were successful in creating the SQLColumn, then add it to our List
                            if (sc.Name != null && sc.Name != "" && sc.SQLType.Trim() != "") SQLColumns.Add(sc);
                        }
                        break;

                    case DatabaseTypes.MySQL:
                        if (l.Contains("CREATE TABLE IF NOT EXISTS"))
                        {
                            string[] dparts = l.Replace("CREATE TABLE IF NOT EXISTS", "").Replace("(", "").Replace("`.`", " ").Replace("`", "").Trim().Split(' ');
                            if (dparts.Length == 2)
                            {
                                Schema = dparts[0];
                                Name = dparts[1];
                            }
                            else
                            {
                                Schema = "";
                                Name = dparts[0];
                            }
                        }
                        else
                        {
                            SQLColumn sc = new SQLColumn(l, dbType);
                            if (sc.Name != null && sc.Name != "" && sc.SQLType.Trim() != "") SQLColumns.Add(sc);
                        }


                        break;
                }
            }
        }

        /// <summary>
        /// Generates Foreign Key Creation Script (NOTE: Currently only applicable to SQL Server)
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public string GenerateForeignKeyScript(DatabaseTypes dbType)
        {
            StringBuilder ret = new StringBuilder();
            switch(dbType)
            {
                case DatabaseTypes.SQLServer:
                    foreach(SQLColumn sc in SQLColumns.Where(x => x.ForeignKey != null))
                    {
                        ret.AppendLine($"ALTER TABLE {Schema.WrapIfNotEmpty("[", "].")}[{Name}]  WITH CHECK ADD  CONSTRAINT [{sc.ForeignKey.ConstraintName}] FOREIGN KEY([{sc.ForeignKey.Column1}])");
                        ret.AppendLine($"REFERENCES {Schema.WrapIfNotEmpty("[", "].")}[{sc.ForeignKey.Table2}] ([{sc.ForeignKey.Column2}])\r\nGO");
                        ret.AppendLine($"ALTER TABLE {Schema.WrapIfNotEmpty("[", "].")}[{Name}] CHECK CONSTRAINT [{sc.ForeignKey.ConstraintName}]\r\nGO");
                    }

                    break;
                default:
                    // Currently this only applies to SQL Server
                    break;
            }

            return ret.ToString();
        }

        /// <summary>
        /// Generates a SQL Create Script  
        /// </summary>
        /// <param name="fromDBType">Original Database Type</param>
        /// <param name="toDBType">New Database Type</param>
        /// <returns></returns>
        public CreateTableBlock GenerateSQLScript(DatabaseTypes fromDBType, DatabaseTypes toDBType)
        {
            StringBuilder sb = new StringBuilder();
            CreateTableBlock ret = new CreateTableBlock();

            ret.Name = Name;

            List<SQLColumn> realColumns = SQLColumns.Where(x => !x.Name.StartsWith("_") && x.SQLType != "").ToList();
            string PrimaryKeys = "";

            switch (toDBType)
            {
                case DatabaseTypes.MySQL:
                    sb.AppendLine("-- ----------------------------------------------------------------------------");
                    sb.AppendLine("-- Table " + Name);
                    sb.AppendLine("-- ----------------------------------------------------------------------------");
                    sb.AppendLine($"CREATE TABLE IF NOT EXISTS {Schema.WrapIfNotEmpty("`","`.")}`{Name}` (");

                    for (int x = 0; x < realColumns.Count; x++)
                    {
                        SQLColumn sc = realColumns[x];
                        if (sc.Size == null) sc.Size = "";

                        bool addComma = true;

                        // If we are on the last Column, do not add a comma...
                        if (x + 1 == realColumns.Count) addComma = false;
                        // Unless we have Primary or Foreign Keys to define...
                        if (PrimaryKeys != "" || ForeignKeys.Count > 0) addComma = true;

                        // If we have an IDENTITY column, we need to format the line a bit differently
                        if (!sc.Size.ToUpper().Contains("IDENTITY"))
                        {
                            sb.AppendLine("  `" + (sc.Name + "` " + SQLColumn.SQLServerTypeToMySQL(SQLColumn.SQLServerTypeToMySQL(sc.SQLType) + sc.Size.PadIfNotStartsWith("(", " ")) + (sc.IsUnique == true ? " UNIQUE" : "") + (sc.IsNullable == true ? " NULL" : " NOT NULL") + (!addComma ? "" : ",")).Replace("  ", " "));
                        }
                        else // Otherwise format as normal
                        {
                            sb.AppendLine("  `" + (sc.Name + "` " + SQLColumn.SQLServerTypeToMySQL(sc.SQLType) + (sc.IsUnique == true ? " UNIQUE" : "") + (sc.IsNullable == true ? " NULL" : " NOT NULL") + " AUTO_INCREMENT" +  (!addComma ? "" : ",")).Replace("  ", " "));

                        }

                        // If the Column is flagged as a Primary Key add it, or if it contains an IDENTITY type this must also be a Primary Key in MySQL
                        if (sc.IsPrimaryKey || sc.Size.ToUpper().Contains("IDENTITY"))
                            PrimaryKeys = PrimaryKeys.CommaAppend(sc.Name.WrapIfNotEmpty("`"));
                    }

                    // If we have accumulated Primary Keys
                    if (PrimaryKeys != "") sb.AppendLine($"  PRIMARY KEY ({PrimaryKeys}){(ForeignKeys.Count() > 0 ? "," : "")}");

                    for (int x = 0; x < ForeignKeys.Count(); x++)
                    {
                        bool addComma = !(x + 1 == ForeignKeys.Count());
                        ForeignKey fk = ForeignKeys[x];
                        sb.AppendLine($"  CONSTRAINT `{fk.ConstraintName}`\r\n    FOREIGN KEY (`{fk.Column1}`)\r\n    REFERENCES `{fk.Table2}` (`{fk.Column2}`){(!addComma ? "" : ",")}");
                        ret.ReferenceTables.Add(fk.Table2);
                    }

                     sb.AppendLine(");\r\n");

                    break;

                case DatabaseTypes.PostgreSQL:

                    break;

                case DatabaseTypes.SQLServer:
                    sb.AppendLine($"/****** Object:  Table {Schema.WrapIfNotEmpty("[", "].") + Name.WrapIfNotEmpty("[","]")}    Script Date: {DateTime.Now} ******/");
                    sb.AppendLine("SET ANSI_NULLS ON");
                    sb.AppendLine("GO");
                    sb.AppendLine("SET QUOTED_IDENTIFIER ON");
                    sb.AppendLine("GO");
                    sb.AppendLine($"CREATE TABLE {Schema.WrapIfNotEmpty("[", "].") + Name.WrapIfNotEmpty("[", "](")}");
                    
                    for (int x = 0; x < realColumns.Count; x++)
                    {
                        SQLColumn sc = realColumns[x];
                        sb.AppendLine("\t" + sc.Name.WrapIfNotEmpty("[", "] ") + sc.SQLType.WrapIfNotEmpty("[", "]") + sc.Size + (sc.IsNullable == true ? " NULL" : " NOT NULL") + ",");
                    }

                    List<SQLColumn> primaryKeys = SQLColumns.Where(x => x.IsPrimaryKey).ToList(); 
                    List<SQLColumn> withConstraints = realColumns.Where(x => x.ConstraintName != null && x.ConstraintName.Trim() != "" && !x.IsPrimaryKey).ToList();

                    if (primaryKeys.Count > 0)
                    {
                        sb.AppendLine($" CONSTRAINT [{primaryKeys[0].ConstraintName}] PRIMARY KEY {primaryKeys[0].ClusterTypeName}\r\n(");

                        for (int y = 0; y < primaryKeys.Count; y++)
                        {
                           SQLColumn sc = primaryKeys[y];
                           sb.AppendLine("\t" + sc.Name.WrapIfNotEmpty("[", "] ASC" + (y + 1 < primaryKeys.Count ? "," : "")));
                        }

                        sb.AppendLine(")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]" + (withConstraints.Count > 0 ? "," : ""));
                    }                  

                    for (int y = 0; y < withConstraints.Count; y++)
                    {
                        SQLColumn sc = withConstraints[y];
                        sb.AppendLine(" CONSTRAINT [" + sc.ConstraintName + "] " + (sc.IsUnique ? "UNIQUE " : "") + sc.ClusterTypeName + "\r\n(");
                        sb.AppendLine("\t" + sc.Name.WrapIfNotEmpty("[", "] ASC"));
                        sb.AppendLine(")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]" + (y + 1 < withConstraints.Count ? "," : ""));
                    }

                    sb.AppendLine(") ON [PRIMARY]\r\nGO");
                                        

                    break;
            }

            ret.SQL = sb.ToString();

            return ret;
        }

        /// <summary>
        /// Generates a Model Class for a specified language from this SQLTable object
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="langType"></param>
        /// <param name="includeEmptyConstructor"></param>
        /// <param name="includeFullParamConstructor"></param>
        /// <param name="includeSerializableDecorator"></param>
        /// <returns></returns>
        /*
        public string GenerateModelClass(DatabaseTypes dbType, LanguageTypes langType, GenerateSettings settings = null, bool IncludeRelevantImports = false)
        {

            if (settings == null) settings = new GenerateSettings();

            string props = "";
            string comment = "";
            string emptyConstructor = "";
            string fullConstructor = "";

            StringBuilder sbMainClassBlock = new StringBuilder();
            StringBuilder sbClassProperties = new StringBuilder();
            StringBuilder sbConstructorParameters = new StringBuilder();
            StringBuilder sbConstructorAssignments = new StringBuilder();
            switch (langType)
            {
                case LanguageTypes.CSharp:
                    if (settings.IncludeSerializeDecorator) sbMainClassBlock.AppendLine("[Serializable]");
                    sbMainClassBlock.AppendLine("public class " + Name + "()\n   {");
                    props = "{ get; set; }";
                    emptyConstructor = "\n      public " + Name + "() {}\n";
                    fullConstructor = "\n      public " + Name + "([[PARAMETERS]])\n      {\n[[ASSIGNMENTS]]      }";
                    break;
                case LanguageTypes.CSharpGraphQLTypes:
                    if (settings.IncludeSerializeDecorator) sbMainClassBlock.AppendLine("[Serializable]");
                    sbMainClassBlock.AppendLine("public class " + Name + "Type : ObjectGraphType<" + Name + ">\n   {");
                    props = "{ get; set; }";
                    emptyConstructor = "\n      public " + Name + "Type() {}\n";
                    fullConstructor = "\n      public " + Name + "Type()\n      {\n[[ASSIGNMENTS]]      }";
                    break;
                case LanguageTypes.TypeScript:
                    sbMainClassBlock.AppendLine("class " + Name + "()\n   {");
                    props = ";";
                    emptyConstructor = "\n      constructor() {}\n";
                    fullConstructor = "\n      constructor([[PARAMETERS]])\n      {\n[[ASSIGNMENTS]]      }";
                    break;
            }

            // Loop through each of the columns...
            foreach (SQLColumn sc in SQLColumns)
            {
                comment = sc.Comment;
                if (comment != "") comment = @"// " + comment;

                switch (langType)
                {
                    case LanguageTypes.CSharp:
                        sbClassProperties.AppendLine("      public " + sc.CSharpType(dbType) + " " + sc.Name + " " + props + " " + comment);
                        if (sbConstructorParameters.Length > 0) sbConstructorParameters.Append(", ");
                        sbConstructorParameters.Append(sc.CSharpType(dbType) + " " + localVariable(sc.Name));
                        sbConstructorAssignments.Append("         " + sc.Name + " = " + localVariable(sc.Name) + ";\n");
                        break;
                    case LanguageTypes.CSharpGraphQLTypes:
                        sbConstructorAssignments.Append("         Field(x => x." + sc.Name + ")" + ";\n");
                        break;
                    case LanguageTypes.TypeScript:
                        sbClassProperties.AppendLine("      " + sc.Name + ": " + sc.CSharpType(dbType) + "; " + comment);
                        if (sbConstructorParameters.Length > 0) sbConstructorParameters.Append(", ");
                        sbConstructorParameters.Append(localVariable(sc.Name) + ": " + sc.CSharpType(dbType));
                        sbConstructorAssignments.Append("         this." + sc.Name + " = " + localVariable(sc.Name) + ";\n");
                        break;
                }
            }

            string ec = (settings.IncludeEmptyConstructor == true ? emptyConstructor : "");
            string fc = (settings.IncludeFullConstructor == true ? fullConstructor.Replace("[[PARAMETERS]]", sbConstructorParameters.ToString()).Replace("[[ASSIGNMENTS]]", sbConstructorAssignments.ToString()) : "");

            sbMainClassBlock.AppendLine(sbClassProperties.ToString() + ec + fc + "\n   }");

            string ret = sbMainClassBlock.ToString();

            if (settings.Namespace != "" && (langType == LanguageTypes.CSharp || langType == LanguageTypes.CSharpGraphQLTypes))
            {
                ret = ret.IncludeNamespace(settings.Namespace);
            }

            if (langType == LanguageTypes.CSharp || langType == LanguageTypes.CSharpGraphQLTypes)
            {
                if (langType == LanguageTypes.CSharpGraphQLTypes) ret = "using GraphQL.Types;\n\n" + ret;
                if (settings.IncludeSerializeDecorator) ret = "using System.Xml.Serialization;\n\n" + ret;                
            }

            return ret;
        } 
        */

        private List<SQLColumn> GetPrimaryKeyColumns()
        {
            return SQLColumns.Where(x => x.IsPrimaryKey).ToList();
        }

        private List<SQLServerConstraint> ExtractSQLServerKeys(string constraintsBlock)
        {
            List<SQLServerConstraint> ret = new List<SQLServerConstraint>();

            constraintsBlock = constraintsBlock.Replace("(\r\r\n\t", "").Replace("ASC,\r\r\n\t", ",").Trim();

            string[] lines = constraintsBlock.Split('\n');
            string cl = "";
            string currentConstraintName = "";
            bool lastLineWasConstraint = false;

            SQLServerConstraint ssc = new SQLServerConstraint();

            foreach (string l in lines)
            {
                cl = l.Trim();

                if (cl.Contains("ASC") && lastLineWasConstraint)
                {
                    string[] columns = cl.ReplaceEach("[|]|ASC", "").Trim().Split(',');
                    foreach (string c in columns)
                    {
                        ssc.ColumnName = c;
                        ret.Add(ssc);
                        ssc = new SQLServerConstraint();
                    }
                }

                if (cl.StartsWith("CONSTRAINT ") && cl.EndsWith("CLUSTERED"))
                {

                    ssc.ConstraintName = cl.SubstringBetween('[', ']');
                    currentConstraintName = ssc.ConstraintName;
                    if (cl.Contains(" UNIQUE ")) ssc.IsUnique = true;
                    if (cl.Contains(" PRIMARY KEY ")) ssc.IsPrimary = true;
                    ssc.ClusterType = SQLServerConstraint.ClusterTypes.Clustered;
                    if (cl.Contains("NONCLUSTERED")) ssc.ClusterType = SQLServerConstraint.ClusterTypes.NonClustered;

                    lastLineWasConstraint = true;
                }
                else
                {
                    lastLineWasConstraint = false;
                }
            }

            return ret;
        }


    }

}
