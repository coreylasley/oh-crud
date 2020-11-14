using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Codeterpret.Common.Enums;

namespace Codeterpret.Common
{
    public enum ItemTypes
    {
        SourceCode = 1,
        Folder = 2
    }

    public class ProjectHiearchy
    {
        public List<ProjectItem> Items { get; set; }

        public ProjectHiearchy(string projectName)
        {
            Items = new List<ProjectItem>();
            Items.Add(new ProjectItem { Name = projectName, ItemType = ItemTypes.Folder, Code = "", Items = new List<ProjectItem>() });
        }

        /// <summary>
        /// Adds a new item to a Project hiearchy
        /// </summary>
        /// <param name="path">i.e. \Project\Models\test.txt</param>
        /// <param name="itemType"></param>
        /// <param name="code"></param>
        public void Add(string path, ItemTypes itemType, string code)
        {
            Items[0] = AddItem(path, itemType, code, Items[0]);
        }

        private ProjectItem AddItem(string path, ItemTypes itemType, string code, ProjectItem level)
        {
            if (path.StartsWith("\\"))
            {
                path = path.Substring(1, path.Length - 1);
            }

            string[] pathParts = path.Split('\\');

            // If we have more than one path part, we are still in a folder....
            if (pathParts.Length > 1)
            {
                string thisPart = pathParts[0];
                bool foundIt = false;
                int index = -1;

                // Loop through the items in at this level, and see if we have a match
                foreach(var i in level.Items)
                {
                    index++;
                    if (i.Name == thisPart)
                    {
                        foundIt = true;
                        break;
                    }
                }

                path = "";

                // build a new path, excluding this part of the path
                for (int x = 1; x < pathParts.Length; x++)
                {
                    path += "\\" + pathParts[x];
                }

                // If we found a match at this level...
                if (!foundIt)
                {
                    // Add it
                    //level.Items.Add(new ProjectItem { Name = thisPart, ItemType = ItemTypes.Folder, Items = new List<ProjectItem>() });
                    level.Items.Add(AddItem(path, itemType, code, new ProjectItem { Name = thisPart, ItemType = ItemTypes.Folder, Items = new List<ProjectItem>() }));
                    index++;
                }
                else
                {
                    // recursivly call this method at the next level
                    level.Items[index] = AddItem(path, itemType, code, level.Items[index]);
                }


            }
            else if (pathParts.Length > 0) // we are at the last path part 
            {
                if (itemType == ItemTypes.SourceCode)
                {
                    level.Items.Add(new ProjectItem { Name = pathParts[0], ItemType = ItemTypes.SourceCode, Code = code });
                }
                else
                {
                    level.Items.Add(new ProjectItem { Name = pathParts[0], ItemType = ItemTypes.Folder, Items = new List<ProjectItem>() });
                }
            }

            level.Items = level.Items.OrderBy(x => x.Name).ToList();

            return level;
        }
    }

    public class ProjectItem
    {
        public ItemTypes ItemType { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public List<ProjectItem> Items { get; set; }
    }
}
