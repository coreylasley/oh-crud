using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeterpret.SQL
{
    public class GenerateSettings
    {

        /// <summary>
        /// Applies to: C#, TypeScript
        /// </summary>
        public bool IncludeEmptyConstructor { get; set; }
        
        /// <summary>
        /// Applies to: C#, TypeScript
        /// </summary>
        public bool IncludeFullConstructor { get; set; }

        /// <summary>
        /// Applies to: C#
        /// </summary>
        public bool IncludeSerializeDecorator { get; set; }

        /// <summary>
        /// Applies to: C#
        /// </summary>
        public string Namespace { get; set; }

        public GenerateSettings() { }
    }
}
