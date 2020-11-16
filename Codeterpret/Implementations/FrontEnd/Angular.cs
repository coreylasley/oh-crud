using Codeterpret.Common;
using Codeterpret.Implementations.Abstract;
using Codeterpret.SQL;
using System;
using System.Collections.Generic;
using System.Text;
using static Codeterpret.Common.Enums;

namespace Codeterpret.Implementations.FrontEnd
{
    public class Angular : FrontEndCodeBase
    {

        private CodeColoring TSPalette = new CodeColoring(CodeColoring.ColorPalettes.TypeScript_VSDark);

        private string publicClass;

        public Angular()
        {
            publicClass = TSPalette.Color("interface", CodeColoring.ColorTypes.PrimitiveType);
        }


        public override IEnumerable<ProjectItem> GenerateProject(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName, SettingGroup group, FileOutputTypes outputType)
        {
            ProjectHiearchy prj = new ProjectHiearchy(projectName);

            List<string> models = new List<string>();
            string code = "";

            bool seperateFilesPerTable = false;                        
          
            // If all the table code exists in a single Service and a single Controller....
            if (!seperateFilesPerTable)
            {
                models = GenerateModels(tables, fromDBType, projectName);                
                                
                // --------------------------------
                // --- MODELS ---------------------
                // --------------------------------
                for (int x = 0; x < models.Count; x++)
                {
                    if (tables[x].IncludeThisTable)
                        prj.Add($@"\Client\app\src\Models\{tables[x].Name}.ts", ItemTypes.SourceCode, outputType == FileOutputTypes.HTML ?
                            TSPalette.RenderWithColor(models[x]) : 
                            TSPalette.RenderWithNoColor(models[x])
                            );
                }

            }
           
            return prj.Items;

        }



        private List<string> GenerateModels(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName)
        {
            List<string> models = new List<string>();
            foreach (SQLTable t in tables)
            {
                if (t.IncludeThisTable)
                    models.Add(GenerateModel(t, fromDBType, projectName));
            }
            return models;
        }

        private string GenerateModel(SQLTable table, DatabaseTypes fromDBType, string projectName)
        {
            
            string publicStr = TSPalette.Color("public", CodeColoring.ColorTypes.PrimitiveType);

            StringBuilder ret = new StringBuilder();

            ret.AppendLine($"{publicClass} {TSPalette.Color(table.Name, CodeColoring.ColorTypes.ClassName)} {{\n");
            
            // Loop through each of the columns...
            foreach (SQLColumn sc in table.SQLColumns)
            {
                ret.AppendLine($"\t{publicStr} {sc.Name}{(sc.IsNullable ? "?" : "")}{sc.TypeScriptType(fromDBType)};\n");                
            }
                        
            return ret.ToString();
        }

    }
}
