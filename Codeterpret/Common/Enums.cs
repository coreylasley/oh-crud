using System;
using System.Collections.Generic;
using System.ComponentModel;
using Codeterpret;

namespace Codeterpret.Common
{
    public class Enums
    {
        public enum DatabaseTypes
        {            
            SQLServer,
            MySQL,
            PostgreSQL,
            QuickScript
        }

        public enum ClusterTypes
        {
            Unknown,
            Clustered,
            NonClustered
        }

        public enum CRUDTypes
        {
            Create,
            Read,
            Update,
            Delete,
            Constructor
        }

        public enum FileOutputTypes
        {
            Text,
            HTML
        }

        public enum BackEndProjectTypes
        {
            [Description("C# .NET Core 3.1 Web API")]
            CSharpNETCore31 = 1
        }

        public enum FrontEndProjectTypes
        {
            [Description("Angular 10 w/ Material")]
            Angular10Material = 1
        }

        public enum ServiceOrganizationTypes
        {
            AllSameClass = 1,
            ClassPerTable = 2,
            ClassPerCRUDType = 3
        }

        public static List<EnumDetail> GetBackEndProjectTypes()
        {
            List<EnumDetail> detail = new List<EnumDetail>();

            var enums = (BackEndProjectTypes[])Enum.GetValues(typeof(BackEndProjectTypes));

            foreach (var e in enums)
            {                
                detail.Add(new EnumDetail { Description = e.GetDescription(), Value = e });
            }

            return detail;
        }

        public static List<EnumDetail> GetFrontEndProjectTypes()
        {
            List<EnumDetail> detail = new List<EnumDetail>();

            var enums = (FrontEndProjectTypes[])Enum.GetValues(typeof(FrontEndProjectTypes));

            foreach (var e in enums)
            {                
                detail.Add(new EnumDetail { Description = e.GetDescription(), Value = e });
            }

            return detail;
        }

    }

    public class EnumDetail
    {
        public string Description { get; set; }
        public Enum Value { get; set; }
    }

}
