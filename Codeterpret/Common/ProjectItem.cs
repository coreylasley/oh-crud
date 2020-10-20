using System;
using System.Collections.Generic;
using System.Text;

namespace Codeterpret.Common
{
    public enum ItemTypes
    {
        SourceCode = 1,
        Folder = 2
    }

    public class ProjectItem
    {
        public ItemTypes ItemType { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public List<ProjectItem> Items { get; set; }
    }
}
