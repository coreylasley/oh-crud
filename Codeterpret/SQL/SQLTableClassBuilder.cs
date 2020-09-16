using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Codeterpret.SQL
{
    /// <summary>
    /// Parses a database generated table creation script and generates language specific model classes representing the tables
    /// </summary>
    public class SQLTableClassBuilder : Common.Common
    {

        public List<SQLTable> SQLTables { get; set; }
        public DatabaseTypes DatabaseType { get; }

        /// <summary>
        /// Parses the Table Schema into SQLTable objects. Auto-Detects the Database Type of SQL Script. 
        /// </summary>
        /// <param name="SQL"></param>
        public SQLTableClassBuilder(string SQL)
        {
            if (SQL.Contains("GO") && SQL.Contains("](") && SQL.Contains("CREATE TABLE ["))
            {
                DatabaseType = DatabaseTypes.SQLServer;
                ParseScript(SQL, DatabaseTypes.SQLServer);
            }
            else if (SQL.Contains("CREATE TABLE IF NOT EXISTS") && SQL.Contains("-- Table "))
            {
                DatabaseType = DatabaseTypes.MySQL;
                ParseScript(SQL, DatabaseTypes.MySQL);
            }
            else
            {
                DatabaseType = DatabaseTypes.NaturalLanguage;
                ParseNaturalDescription(SQL);
            }
        }

        /// <summary>
        /// Parses the Table Schema into SQLTable objects.
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="dbType"></param>
        public SQLTableClassBuilder(string SQL, DatabaseTypes dbType)
        {
            DatabaseType = dbType;
            ParseScript(SQL, dbType);
        }

        /// <summary>
        /// Parses the Table Schema into a list of SQLTable objects.
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="dbType"></param>
        public void ParseScript(string SQL, DatabaseTypes dbType)
        {
            StringBuilder runningBlock = new StringBuilder();
            List<SQLBlock> blocks = new List<SQLBlock>();
            string[] lines = SQL.Split('\n');

            string blockTerminator = "";
            string createTableDelimiter = "";

            // Determine what to look for when parsing the script to determine where create table blocks are
            switch (dbType)
            {
                case DatabaseTypes.SQLServer:
                    blockTerminator = "GO";
                    createTableDelimiter = "CREATE TABLE";
                    break;
                case DatabaseTypes.MySQL:
                    blockTerminator = "-- -----------------------------------------------------";
                    createTableDelimiter = "CREATE TABLE IF NOT EXISTS";
                    break;
            }

            // Loop through each line of the script
            foreach (string l in lines)
            {
                // If the line is a block terminator
                if (l.Trim() == blockTerminator || (l.Contains(blockTerminator) && dbType == DatabaseTypes.MySQL))
                {
                    // create a new block and add the running block script to it
                    blocks.Add(new SQLBlock(runningBlock.ToString()));

                    // reset our running block
                    runningBlock = new StringBuilder();
                }
                else
                {
                    // add the line to our running block
                    runningBlock.AppendLine(l);
                }
            }

            // Init our List of SQLTable
            SQLTables = new List<SQLTable>();

            // Create & Init a List of ForeignKey
            List<ForeignKey> fks = new List<ForeignKey>();

            // Loop through each block
            foreach (SQLBlock sb in blocks)
            {
                // If the block contains our create table delimiter
                if (sb.Text.Contains(createTableDelimiter))
                {
                    // Create and add a new SQLTable to our List
                    SQLTables.Add(new SQLTable(sb, dbType));
                }

                // If the block contains a Foreign Key Reference
                if (sb.Text.Contains("FOREIGN KEY") && sb.Text.Contains("REFERENCES"))
                {
                    // Create and add a new ForeignKey to our List
                    fks.Add(new ForeignKey(sb, dbType));
                }
            }

            // Loop through all of the ForeignKey's that we created
            foreach (ForeignKey fk in fks)
            {
                // Find the matching table
                foreach (SQLTable st in SQLTables.Where(x => x.Name == fk.Table1))
                {
                    // Add the ForeignKey as a SQLColumn 
                    st.SQLColumns.Add(new SQLColumn { Name = localVariable(fk.Table2), ForeignKeyType = fk.Table2, ForeignKey = fk, Comment = fk.ConstraintName });
                }
            }


        }

        /// <summary>
        /// Parses the Natural Language Table Schema Description into a list of SQLTable objects.
        /// </summary>
        /// <param name="nd"></param>
        public void ParseNaturalDescription(string nd)
        {
            /*  EXAMPLE
                ===========================
                t Person
                c ID, an, pk
                c Name varchar 50
                c Email 100 varchar, nn
                c Gender, fk to Genders MorF, nn

                t Genders
                c MorF, pk, auto 
             */

            string[] lines = nd.Split('\n');
            string[] sections;
            string[] parts;

            SQLTable st = null;
            SQLColumn sc = null;

            SQLTables = new List<SQLTable>();

            // Loop through each line 
            foreach (string l in lines)
            {
                if (l.Trim() != "")
                {
                    // If we are looking at a table line
                    if (l.ToLower().Trim().StartsWith("table ") || l.ToLower().Trim().StartsWith("t "))
                    {
                        sections = l.Split(' ');
                        if (sections.Length > 1)
                        {
                            // If we had an active table, time to add it to our list
                            if (st != null) SQLTables.Add(st);
                            // start a new table
                            st = new SQLTable { Name = sections[1].Trim() };
                        }
                    }

                    // If we are looking at a column line
                    if (l.ToLower().Trim().StartsWith("column ") || l.ToLower().Trim().StartsWith("c "))
                    {
                        sections = l.Split(',');

                        // Get the column name
                        parts = sections[0].Split(' ');
                        if (parts.Length > 1)
                        {
                            sc = new SQLColumn { Name = parts[1], SQLType = "", IsNullable = true };

                            // Get the type if specified
                            if (parts.Length > 2)
                            {
                                int v = 0;
                                int.TryParse(parts[2], out v);
                                if (v == 0)
                                {
                                    sc.SQLType = parts[2];
                                }
                                else // It is a number, so maybe its the size?
                                {
                                    sc.Size = $"({v})";
                                }
                            }

                            // Get the size if specified
                            if (parts.Length > 3)
                            {
                                int v = 0;
                                int.TryParse(parts[3], out v);
                                if (v != 0)
                                {
                                    sc.Size = $"({v})";
                                }
                                else // if its not a number..
                                {
                                    // And we already got our size, but havent gotten our type yet
                                    if (sc.Size != "" && sc.SQLType == "") sc.SQLType = parts[3];
                                }
                            }


                            // Loop through the rest of the sections in the column definition
                            for (int x = 1; x < sections.Length; x++)
                            {
                                string s = sections[x].Trim().ToLower();

                                // Section defines Autonumber/Identity
                                if (s == "an" || s == "auto" || s == "autonumber" || s == "auto number" || s == "i" || s == "identity")
                                {
                                    sc.IsIdentity = true;
                                    sc.IsNullable = false;
                                    sc.IsUnique = true;
                                }

                                // Section defines Primary Key
                                if (s == "pk" || s == "primary" || s == "primarykey" || s == "primary key")
                                {
                                    sc.IsPrimaryKey = true;
                                    sc.ConstraintName = $"PK_{st.Name}";
                                    sc.ClusterType = ClusterTypes.Clustered;
                                }

                                // Section defines NOT NULL
                                if (s == "nn" || s == "notnull" || s == "not null" || s == "notnullable" || s == "not nullable")
                                {
                                    sc.IsNullable = false;
                                }

                                // Section defines Foriegn Key
                                if (s.StartsWith("r to") || s.StartsWith("relates to") || s.StartsWith("fk to") || s.StartsWith("foriegn key to") || s.StartsWith("foriegnkey to"))
                                {
                                    parts = sections[x].Trim().Split(' ');
                                    // The last two parts should be TableName and Column
                                    ForeignKey fk = new ForeignKey { Table1 = st.Name, Column1 = sc.Name, Table2 = parts[parts.Length - 2], Column2 = parts[parts.Length - 1], ConstraintName = $"FK_{st.Name}_{sc.Name}_{parts[parts.Length - 2]}_{parts[parts.Length - 1]}" };
                                    sc.ForeignKey = fk;

                                }
                            }

                        }

                        if (sc.SQLType == "") sc.SQLType = "INT";
                        st.SQLColumns.Add(sc);
                    }
                }

            }

            // If we had an active table, time to add it to our list
            if (st != null) SQLTables.Add(st);


        }

        /// <summary>
        /// Saves each Model Class to its own file within the specified directory
        /// </summary>
        /// <param name="langType"></param>
        /// <param name="path"></param>
        /// <param name="overwriteExisting"></param>
        /// <returns></returns>
        /*
        public Dictionary<string, bool> SaveSQLScripts(DatabaseTypes dbType, LanguageTypes langType, string path, bool overwriteExisting, GenerateSettings settings)
        {
            Dictionary<string, bool> ret = new Dictionary<string, bool>();

            string ext = "";

            if (!path.EndsWith(@"\")) path += @"\";

            switch(langType)
            {
                case LanguageTypes.CSharp:
                    ext = ".cs";
                    break;
                case LanguageTypes.CSharpGraphQLTypes:
                    ext = ".cs";
                    break;
                case LanguageTypes.TypeScript:
                    ext = ".ts";
                    break;
            }

            foreach(SQLTable st in SQLTables)
            {

                string fileName = path + st.Name + (langType == LanguageTypes.CSharpGraphQLTypes ? "Type" : "") + ext;

                if (!File.Exists(fileName) || (File.Exists(fileName) && overwriteExisting))
                {
                    try
                    {
                        File.WriteAllText(fileName, st.GenerateModelClass(dbType, langType, settings, true));
                        ret.Add(fileName, true);
                    }
                    catch
                    {
                        ret.Add(fileName, false);
                    }
                }
                else
                {
                    ret.Add(fileName, false);
                }
            }

            return ret;
        }
        */

        /// <summary>
        /// Loads a SQL Script file from disk
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string LoadSQLScript(string fileName)
        {
            string ret = "";

            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                ret = streamReader.ReadToEnd();
            }

            return ret;
        }


    }

}
