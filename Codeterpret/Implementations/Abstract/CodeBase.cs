using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codeterpret.Common;
using Codeterpret.Interfaces;
using Codeterpret.SQL;
using static Codeterpret.Common.Common;

namespace Codeterpret.Implementations
{
    public abstract class CodeBase : ICode
    {
        /// <summary>
        /// Comma delimited string of substrings that help to determine if a line of code would not be a Property definition
        /// </summary>
        public string PropertyDefinitionsShouldNotContain { get; set; }

        /// <summary>
        /// Comma delimited string of ORM names that are supported in code generation
        /// </summary>
        public string ORMs { get; set; }

        /// <summary>
        /// Generates a SQL Script from a block of Code. NOTE: GenerateSQLTables() must be implemented.
        /// </summary>
        /// <param name="Code">Can be the contents of an entire code file</param>
        /// <param name="dbType">The type of SQL script to be generated</param>
        /// <returns></returns>
        public string GenerateSQLScript(string Code, DatabaseTypes dbType)
        {
            List<SQLTable> sts = GenerateSQLTables(Code);

            return GenerateSQLScript(sts, dbType);
        }

        /// <summary>
        /// Generates a SQL Script from a List of SQLTable Objects
        /// </summary>
        /// <param name="sqlTableList">List of SQLTable Objects</param>
        /// <param name="dbType">The type of SQL script to be generated</param>
        /// <returns></returns>
        public string GenerateSQLScript(List<SQLTable> sqlTableList, DatabaseTypes dbType)
        {
            StringBuilder sb = new StringBuilder();

            //sb.AppendLine("/**** WARNING: Because this script was generated from C# Classes which do not include data type precision (length, nullability, foreign key relationships, constraint names,)\r\n column data is only a best guess and should be manually checked for accuracy and/or intention of use *****/\r\n ");

            List<CreateTableBlock> ctbs = new List<CreateTableBlock>();
            foreach (SQLTable st in sqlTableList)
            {
                ctbs.Add(st.GenerateSQLScript(DatabaseTypes.SQLServer, dbType));
            }

            ctbs = CreateTableBlock.SortByDependency(ctbs);

            foreach (CreateTableBlock ctb in ctbs)
            {
                sb.AppendLine(ctb.SQL);
            }

            if (dbType == DatabaseTypes.SQLServer)
            {
                foreach (SQLTable st in sqlTableList)
                {
                    string fk = st.GenerateForeignKeyScript(DatabaseTypes.SQLServer);
                    if (fk != "") sb.AppendLine(fk);
                }
            }

            return sb.ToString();
        }

        public virtual List<SQLTable> GenerateSQLTables(string code, bool AddIDColumnIfMissing = true)
        {
            throw new Exception("GenerateSQLTables Method has not yet been implemented");            
        }

        public virtual List<string> GenerateServiceMethods(List<SQLTable> tables, DatabaseTypes fromDBType, string ORM, bool groupByTable = false, bool AsInterface = false)
        {
            throw new Exception("GenerateServiceMethods Method has not yet been implemented");
        }

        public virtual List<string> GenerateControllerMethods(List<SQLTable> tables, DatabaseTypes fromDBType, string serviceName, bool groupByTable = false)
        {
            throw new Exception("GenerateControllerMethods Method has not yet been implemented");
        }

        public virtual List<string> GenerateModels(List<SQLTable> tables, DatabaseTypes fromDBType, GenerateSettings settings = null, bool IncludeRelevantImports = false)
        {
            throw new Exception("GenerateModels Method has not yet been implemented");
        }

        public virtual IEnumerable<ProjectItem> GenerateProject(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName, string orm, bool seperateFilesPerTable = false)
        {
            throw new Exception("GenerateProject Method has not yet been implemented");
        }
    }
}
