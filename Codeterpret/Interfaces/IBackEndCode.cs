using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Codeterpret.Common.Enums;
using Codeterpret.SQL;
using Codeterpret.Common;

namespace Codeterpret.Interfaces
{
    public interface IBackEndCode
    {
        /// <summary>
        /// Comma delimited string of substrings that help to determine if a line of code would not be a Property definition
        /// </summary>
        string PropertyDefinitionsShouldNotContain { get; set; }

        /// <summary>
        /// Comma delimited string of ORM names that are supported in code generation
        /// </summary>
        string ORMs { get; set; }

        /// <summary>
        /// Generates a SQL Script from a block of Code. NOTE: GenerateSQLTables() must be implemented.
        /// </summary>
        /// <param name="Code">Can be the contents of an entire code file</param>
        /// <param name="dbType">The type of SQL script to be generated</param>
        /// <returns></returns>
        string GenerateSQLScript(string Code, DatabaseTypes dbType);

        /// <summary>
        /// Generates a SQL Script from a List of SQLTable Objects
        /// </summary>
        /// <param name="sqlTableList">List of SQLTable Objects</param>
        /// <param name="dbType">The type of SQL script to be generated</param>
        /// <returns></returns>
        string GenerateSQLScript(List<SQLTable> sqlTableList, DatabaseTypes dbType);


        List<SQLTable> GenerateSQLTables(string code, bool AddIDColumnIfMissing = true);

        /// <summary>
        /// Generates directories and files representing a project structure (Models, Interfaces, Services, Controllers) in the implemented language
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        IEnumerable<ProjectItem> GenerateProject(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName, string orm, SettingGroup settings);

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
        List<string> GenerateControllerMethods(List<SQLTable> tables, DatabaseTypes fromDBType, string serviceName, string controllerName, string interfaceName, bool groupByTable = false);

        /// <summary>
        /// Generates a List of Models representing Table Objects in the implemented language
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="fromDBType"></param>
        /// <param name="settings"></param>
        /// <param name="IncludeRelevantImports"></param>
        /// <returns></returns>
        List<string> GenerateModels(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName, GenerateSettings settings = null, bool IncludeRelevantImports = false);

        
    }
}
