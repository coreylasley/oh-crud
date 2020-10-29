using Codeterpret.Common;
using Codeterpret.Implementations.BackEnd;
using Codeterpret.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Codeterpret.Implementations
{
    public class Project
    {
        public IBackEndCode CodeGenerator { get; }

        public Project(Enums.BackEndProjectTypes projectType)
        {
            switch (projectType)
            {
                case Enums.BackEndProjectTypes.CSharpNETCore31:
                    CodeGenerator = new CSharp();
                    break;
            }
        }
    }
}
