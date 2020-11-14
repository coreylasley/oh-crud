using Codeterpret.Common;
using Codeterpret.SQL;
using System;
using System.Collections.Generic;
using System.Text;
using static Codeterpret.Common.Enums;

namespace Codeterpret.Interfaces
{
    public interface IFrontEndCode
    {
        SettingGroup SettingsDefinition { get; }

        /// <summary>
        /// Generates directories and files representing a project structure (Models, Interfaces, Services, Controllers) in the implemented language
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        IEnumerable<ProjectItem> GenerateProject(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName, SettingGroup settings, FileOutputTypes outputType);
    }
}
