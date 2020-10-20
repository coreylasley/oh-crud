using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Codeterpret.Common.Common;
using Codeterpret.SQL;
using Codeterpret.Common;

namespace Codeterpret.Interfaces
{
    public interface ICode
    {
        string PropertyDefinitionsShouldNotContain { get; set; }
        string ORMs { get; set; }

        string GenerateSQLScript(string Code, DatabaseTypes dbType);
        string GenerateSQLScript(List<SQLTable> sqlTableList, DatabaseTypes dbType);
        List<SQLTable> GenerateSQLTables(string code, bool AddIDColumnIfMissing = true);

        /// <summary>
        /// Generates a List of Service Methods for basic CRUD Operations in the implemented language
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="AsInterface"></param>
        /// <returns></returns>
        List<string> GenerateServiceMethods(List<SQLTable> tables, DatabaseTypes fromDBType, string ORM, bool groupByTable = false, bool AsInterface = false);
        
        /// <summary>
        /// Generates a List of Controller Methods that call Service Methods in the implemented language 
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        List<string> GenerateControllerMethods(List<SQLTable> tables, DatabaseTypes fromDBType, string serviceName, bool groupByTable = false);

        /// <summary>
        /// Generates a List of Models representing Table Objects in the implemented language
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="fromDBType"></param>
        /// <param name="settings"></param>
        /// <param name="IncludeRelevantImports"></param>
        /// <returns></returns>
        List<string> GenerateModels(List<SQLTable> tables, DatabaseTypes fromDBType, GenerateSettings settings = null, bool IncludeRelevantImports = false);

        /// <summary>
        /// Generates directories and files representing a project structure (Models, Interfaces, Services, Controllers) in the implemented language
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        IEnumerable<ProjectItem> GenerateProject(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName, string orm, bool seperateFilesPerTable = false);
    }
}
