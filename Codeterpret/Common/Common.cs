using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeterpret.Common
{
    public class Common
    {
        public enum DatabaseTypes
        {            
            SQLServer,
            MySQL,
            PostgreSQL,
            NaturalLanguage
        }

        public enum LanguageTypes
        {
            CSharp,
            CSharpGraphQLTypes,
            TypeScript            
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
            Delete
        }

        protected string localVariable(string name)
        {
            if (name == "ID") name = "Id";

            string ret = name;

            if (name.Length > 0)
            {
                name = ret.Substring(0, 1).ToLower() + ret.Substring(1, ret.Length - 1);
                if (name != ret)
                    ret = name;
                else
                    ret = "_" + ret;
            }

            return ret;
        }

    }
}
