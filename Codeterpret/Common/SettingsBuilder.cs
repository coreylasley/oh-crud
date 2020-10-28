using System;
using System.Collections.Generic;
using System.Text;
using Codeterpret.Common;

namespace Codeterpret.Common
{
    public class SettingsBuilder
    {
        public static SettingGroup GetSettings(Enums.BackEndProjectTypes projectType)
        {
            SettingGroup ret = null;

            switch(projectType)
            {
                case Enums.BackEndProjectTypes.CSharpNETCore31Dapper:
                     ret = CSharpNETCore31Dapper();
                break;
            }

            return ret;
        }

        /// <summary>
        /// Extra Settings for C# .NET Core 3.1 w/ Dapper projects
        /// </summary>
        /// <returns></returns>
        private static SettingGroup CSharpNETCore31Dapper()
        {
            SettingGroup settings = new SettingGroup();

            List<SettingOption> so = new List<SettingOption>();
            so.Add(new SettingOption { Value = "1", Label = "Angular" });
            so.Add(new SettingOption { Value = "2", Label = "Other" });
            settings.Settings.Add(new Setting { Type = InputTypes.Select, Key = "ExpectedProjectType", Label = "Expected Front-End Project Type", Options = so, Display = true });

            so = new List<SettingOption>();
            so.Add(new SettingOption { Value = "1", Label = "Everything in same Class" });
            //so.Add(new SettingOption { Value = "2", Label = "In Class by Table" });
            //so.Add(new SettingOption { Value = "3", Label = "In Class by CRUD Operation (i.e. CreateService)" });
            settings.Settings.Add(new Setting { Type = InputTypes.Select, Key = "ServiceClassOrganization", Label = "Service Class Organization", Options = so, Display = true });

            return settings;
        }
    }
}
