using Codeterpret.Common;
using Codeterpret.Interfaces;
using Codeterpret.SQL;
using System;
using System.Collections.Generic;
using System.Text;
using static Codeterpret.Common.Enums;

namespace Codeterpret.Implementations.Abstract
{
    public abstract class FrontEndCodeBase : IFrontEndCode
    {
        public virtual SettingGroup SettingsDefinition { get; }

        public virtual IEnumerable<ProjectItem> GenerateProject(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName, SettingGroup settings, FileOutputTypes outputType)
        {
            throw new Exception("GenerateProject Method has not yet been implemented");
        }

    }
}
