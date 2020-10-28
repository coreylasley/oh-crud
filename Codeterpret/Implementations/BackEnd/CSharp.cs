using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Codeterpret.SQL;
using System.IO.Compression;
using static Codeterpret.Common.Enums;
using Codeterpret.Common;
using Codeterpret.Implementations.Abstract;

namespace Codeterpret.Implementations.BackEnd
{

    public class CSharp : BackEndCodeBase
    {
       
        public CSharp()
        {
            PropertyDefinitionsShouldNotContain = "enum,struct,event,const";
            ORMs = "dapper,ado";
        }

        private string controllerTryCatchBlock = "try\n\t\t{\n\t\t\t<CODE>\n\t\t}\n\t\tcatch\n\t\t{\n\t\t\treturn StatusCode((int)HttpStatusCode.InternalServerError);\n\t\t}";
        private string controllerOkOrBadRequest = "<CALL>\n\t\t\tif (<CONDITION>)\n\t\t\t{\n\t\t\t\treturn Ok(\"<RETURN>\");\n\t\t\t}\n\t\t\telse\n\t\t\t{\n\t\t\t\treturn BadRequest(\"<MESSAGE>\");\n\t\t\t}";
        private string controllerNoContentBadRequest = "if (<CONDITION>)\n\t\t\t{\n\t\t\t\treturn StatusCode((int)HttpStatusCode.NoContent);\n\t\t\t}\n\t\t\telse\n\t\t\t{\n\t\t\t\treturn BadRequest(\"<MESSAGE>\");\n\t\t\t}";

        public override IEnumerable<ProjectItem> GenerateProject(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName, string orm, SettingGroup group)
        {
            List<ProjectItem> ret = new List<ProjectItem>();
            ret.Add(new ProjectItem { Name = projectName, ItemType = ItemTypes.Folder, Items = new List<ProjectItem>() });

            List<string> interfaceMethods = new List<string>();
            List<string> serviceMethods = new List<string>();
            List<string> controllerMethods = new List<string>();
            List<string> models = new List<string>();
            string code = "";

            bool seperateFilesPerTable = false;

            // If all the table code exists in a single Service and a single Controller....
            if (!seperateFilesPerTable)
            {
                interfaceMethods = GenerateServiceMethods(tables, fromDBType, "", false, true);
                serviceMethods = GenerateServiceMethods(tables, fromDBType, "dapper", false, false);
                controllerMethods = GenerateControllerMethods(tables, fromDBType, "dataService", "DataController", "IDataService", false);
                models = GenerateModels(tables, fromDBType, projectName);

                // --- CONTROLLERS ----------------
                code = "";
                foreach (string s in controllerMethods)
                {
                    code += s + "\n\n";
                }

                ret[0].Items.Add(new ProjectItem { Name = "Controllers", ItemType = ItemTypes.Folder, Items = new List<ProjectItem>() });
                ret[0].Items[0].Items.Add(new ProjectItem { Name = "DataController.cs", ItemType = ItemTypes.SourceCode, Code = GenerateControllerClass(projectName, "Crud", "controller", code) });


                code = "";
                // --- INTERFACES -----------------
                foreach (string s in interfaceMethods)
                {
                    code += s + "\n\n";
                }
                //WriteFile(rootPath + "Interfaces\\IDataService.cs", code);
                ret[0].Items.Add(new ProjectItem { Name = "Interfaces", ItemType = ItemTypes.Folder, Items = new List<ProjectItem>() });
                ret[0].Items[1].Items.Add(new ProjectItem { Name = "IDataService.cs", ItemType = ItemTypes.SourceCode, Code = GenerateInterfaceClass(projectName, "IDataService", code) });


                code = "";
                // --- MODELS ---------------------
                ret[0].Items.Add(new ProjectItem { Name = "Models", ItemType = ItemTypes.Folder, Items = new List<ProjectItem>() });
                for (int x = 0; x < models.Count; x++)
                {
                    if (tables[x].IncludeThisTable)
                        ret[0].Items[2].Items.Add(new ProjectItem { Name = tables[x].Name + ".cs", ItemType = ItemTypes.SourceCode, Code = models[x] });
                }


                // --- SERVICES --------------------
                code = "";
                foreach (string s in serviceMethods)
                {
                    code += s + "\n\n";
                }

                ret[0].Items.Add(new ProjectItem { Name = "Services", ItemType = ItemTypes.Folder, Items = new List<ProjectItem>() });
                ret[0].Items[3].Items.Add(new ProjectItem { Name = "DataService.cs", ItemType = ItemTypes.SourceCode, Code = GenerateServiceClass(projectName, "DataService", "IDataService", code) });

               
                // --- MISC PROJECT FILES ---------------
                ret[0].Items.Add(new ProjectItem { Name = projectName + ".csproj", ItemType = ItemTypes.SourceCode, Code = GenerateProjectCSPROJ() });
                ret[0].Items.Add(new ProjectItem { Name = "Program.cs", ItemType = ItemTypes.SourceCode, Code = GenerateProgramCS(projectName) });
                ret[0].Items.Add(new ProjectItem { Name = "Startup.cs", ItemType = ItemTypes.SourceCode, Code = "" });
                ret[0].Items.Add(new ProjectItem { Name = "Dockerfile", ItemType = ItemTypes.SourceCode, Code = GenerateDockerFile(projectName) });
                ret[0].Items.Add(new ProjectItem { Name = "README.md", ItemType = ItemTypes.SourceCode, Code = "" });

            }
            else // If we want each table represented in its own Service and Controller
            {
                interfaceMethods = GenerateServiceMethods(tables, fromDBType, "", true, true);
                serviceMethods = GenerateServiceMethods(tables, fromDBType, orm, true, false);
                controllerMethods = GenerateControllerMethods(tables, fromDBType, "", "", "", true);

                Dictionary<string, string> injections = new Dictionary<string, string>();
                Dictionary<string, string> startupInjections = new Dictionary<string, string>();

                for (int x = 0; x < tables.Count; x++)
                {
                    startupInjections.Add($"I{tables[x].Name}Service", $"{tables[x].Name}Service");

                    // Write the Interface class
                    //WriteFile(rootPath + $"Interfaces\\I{tables[x].Name}.cs", GenerateInterfaceClassFile(interfaceMethods[x], projectName, tables[x].Name));

                    // Write the Service class
                    //WriteFile(rootPath + $"Services\\{tables[x].Name}Service.cs", GenerateServiceClassFile(serviceMethods[x], projectName, tables[x].Name, new Dictionary<string, string>(), fromDBType, orm));

                    // Write the Controller class
                    injections = new Dictionary<string, string>();
                    injections.Add($"I{tables[x].Name}Service", $"{tables[x].Name}Service");
                    //WriteFile(rootPath + $"Controllers\\{tables[x].Name}Controller.cs", GenerateControllerClassFile(controllerMethods[x], projectName, tables[x].Name, injections));
                }

                // Write Startup class
                //WriteFile(rootPath + $"Startup.cs", GenerateStartupClassFile(projectName, "", startupInjections, fromDBType));

            }

            /*
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = archive.CreateEntry("foo.txt");

                    using (var entryStream = demoFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write("Bar!");
                    }
                }

                using (var fileStream = new FileStream(@"C:\Temp\test.zip", FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
            */

            return ret;

        }

        public override List<SQLTable> GenerateSQLTables(string code, bool addIDColumnIfMissing = true)
        {
            List<SQLTable> ret = new List<SQLTable>();
            bool IsInClass = false;

            string[] lines = code.Split('\n');
            string line; 
            int levelDepth = 0;

            SQLTable currentTable = new SQLTable();

            for(int x = 0; x < lines.Length; x++)
            {
                line = lines[x].Trim();
                // If we are not already in a class, and the current line appears to be a class declaration...
                if (!IsInClass && line.Contains(" class "))
                {
                    // If the next line contains an opening bracket...
                    if (x + 1 < lines.Length && lines[x+1].Trim().StartsWith("{"))
                    {
                        IsInClass = true;

                        // Split the parts of the line
                        string[] cdParts = line.Split(' ');
                        // Loop through each part
                        for (int y = 0; y < cdParts.Length; y++)
                        {
                            // If this part is "class" and there is at least one more after
                            if (cdParts[y].Trim() == "class" && y + 1 < cdParts.Length)
                            {
                                // That is likely the class name
                                currentTable.Name = cdParts[y + 1];
                                break;
                            }
                        }

                    }
                }
                else
                {
                    // If we are currently within a class block
                    if (IsInClass)
                    {
                        int obCount = line.ContainsHowMany("{");
                        int cbCount = line.ContainsHowMany("}");
                        int bDiff = levelDepth + obCount - cbCount;

                        // Determine if our depth level needs to be adjusted
                        if (levelDepth != bDiff) levelDepth = bDiff;

                        if (levelDepth == 0 && line == "}")
                        {
                            IsInClass = false;

                            if (currentTable.SQLColumns.Count > 0)
                            {
                                if (currentTable.SQLColumns.Where(z => z.Name.ToUpper() == "ID").Count() == 0)
                                {
                                    SQLColumn sc = new SQLColumn();
                                    sc.Name = "ID";
                                    sc.SQLType = "INT";
                                    sc.IsIdentity = true;
                                    sc.IsPrimaryKey = true;
                                    sc.IsNullable = false;
                                    sc.ConstraintName = "PK__" + currentTable.Name;
                                    currentTable.SQLColumns.Insert(0, sc);
                                }
                                else
                                {
                                    foreach (SQLColumn sc in currentTable.SQLColumns.Where(z => z.Name.ToUpper() == "ID"))
                                    {
                                        sc.IsIdentity = true;
                                        sc.IsPrimaryKey = true;
                                        sc.IsNullable = false;
                                        sc.ConstraintName = "PK__" + currentTable.Name;
                                    }
                                }

                                ret.Add(currentTable);
                                currentTable = new SQLTable();
                            }
                        }

                        // If we are only one level deep, we should be in the area where properties are defined
                        if (levelDepth == 1)
                        {
                            // If the first word of the line contains "public", and does not look like a constructor we might be looking at a property
                            if (line.StartsWith("public ") && !line.Contains(currentTable.Name + "("))
                            {
                                bool goodToGo = true;
                                // Loop through each of our PropertyDefinitionsShouldNotContain words...
                                foreach (string badWord in PropertyDefinitionsShouldNotContain.Split(','))
                                {
                                    // If we found that in the line, than it isnt a property def
                                    if (line.Contains(badWord))
                                    {
                                        goodToGo = false;
                                        break;
                                    }
                                }

                                // If we are still good to go, then this is most likely a property def
                                if (goodToGo)
                                {
                                    string[] pdefParts = line.Split(' ');
                                    if (pdefParts.Length >= 3)
                                    {
                                        bool nullable = false;
                                        string propName = pdefParts[2];
                                        string propType = pdefParts[1];

                                        if (propType.Contains("Nullable") || propType.Contains("?")) nullable = true;

                                        if (!propType.ToLower().Contains("void") && !propName.Contains("("))
                                        {
                                            currentTable.SQLColumns.Add(new SQLColumn() { Name = propName, SQLType = propType, IsNullable = nullable });
                                        }
                                    }
                                }

                            }
                        }                        

                        
                    }
                }
            }

            // Sweep through the tables to see if any Foreign Keys can be detected and created
            foreach(SQLTable st in ret)
            {
                for (int x = 0; x < st.SQLColumns.Count; x++)
                {
                    if (st.SQLColumns[x].SQLType.Contains("<"))
                    {
                        string[] parts = st.SQLColumns[x].SQLType.Split('<');
                        string realType = parts[1].Replace(">", "");
                                                
                        foreach(SQLTable st2 in ret)
                        {
                            if (st2.Name == realType)
                            {
                                ForeignKey fk = new ForeignKey();
                                fk.Table1 = st.Name;
                                fk.Table2 = st2.Name;
                                fk.Column1 = st2.Name + "ID";
                                fk.Column2 = "ID";
                                fk.ConstraintName = $"FK__{st.Name}_{st2.Name}";

                                st.SQLColumns[x].ForeignKey = fk;
                                st.SQLColumns[x].SQLType = "INT";
                                st.SQLColumns[x].Name = st2.Name + "ID";

                                break;
                            }
                        }                      
                                                
                    }
                }
            }

            // Final sweep through the tables to clean up the Type names
            foreach (SQLTable st in ret)
            {
                foreach(SQLColumn sc in st.SQLColumns)
                {
                    sc.SQLType = CleanType(sc.SQLType);
                }
            }

            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="fromDBType"></param>
        /// <param name="ORM"></param>
        /// <param name="groupByTable"></param>
        /// <param name="AsInterface"></param>
        /// <returns></returns>
        public override List<string> GenerateServiceMethods(List<SQLTable> tables, DatabaseTypes fromDBType, string ORM, bool groupByTable = false, bool AsInterface = false)
        {
            List<string> methods = new List<string>();
            foreach (SQLTable t in tables)
            {
                if (!groupByTable)
                {
                    if (t.GenerateCreate) methods.Add(GenerateServiceMethod(t, fromDBType, CRUDTypes.Create, ORM, AsInterface));
                    if (t.GenerateRead) methods.Add(GenerateServiceMethod(t, fromDBType, CRUDTypes.Read, ORM, AsInterface));
                    if (t.GenerateUpdate) methods.Add(GenerateServiceMethod(t, fromDBType, CRUDTypes.Update, ORM, AsInterface));
                    if (t.GenerateDelete) methods.Add(GenerateServiceMethod(t, fromDBType, CRUDTypes.Delete, ORM, AsInterface));
                }
                else
                {
                    string code = "";
                    if (t.GenerateCreate) code += GenerateServiceMethod(t, fromDBType, CRUDTypes.Create, ORM, AsInterface) + "\n\n";
                    if (t.GenerateRead) code += GenerateServiceMethod(t, fromDBType, CRUDTypes.Read, ORM, AsInterface) + "\n\n";
                    if (t.GenerateUpdate) code += GenerateServiceMethod(t, fromDBType, CRUDTypes.Update, ORM, AsInterface) + "\n\n";
                    if (t.GenerateDelete) code += GenerateServiceMethod(t, fromDBType, CRUDTypes.Delete, ORM, AsInterface);

                    methods.Add(code.Trim());
                }
            }
            return methods;           
        }

        public override List<string> GenerateControllerMethods(List<SQLTable> tables, DatabaseTypes fromDBType, string serviceName, string controllerName, string interfaceName, bool groupByTable = false)
        {
            List<string> methods = new List<string>();
            string sn = "";
            string cn = "";
            string inf = "";

            if (controllerName == "")
                cn = "DataController";
            else
                cn = controllerName;

            if (interfaceName == "")
                inf = "IDataService";
            else
                inf = interfaceName;

            methods.Add(GenerateControllerMethod(null, fromDBType, CRUDTypes.Constructor, serviceName, cn, inf));

            foreach (SQLTable t in tables)
            {
                if (serviceName == "")
                    sn = t.Name + "Service";
                else
                    sn = serviceName;                

                if (!groupByTable)
                {
                    
                    if (t.GenerateCreate) methods.Add(GenerateControllerMethod(t, fromDBType, CRUDTypes.Create, sn, cn, inf));
                    if (t.GenerateRead) methods.Add(GenerateControllerMethod(t, fromDBType, CRUDTypes.Read, sn, cn, inf));
                    if (t.GenerateUpdate) methods.Add(GenerateControllerMethod(t, fromDBType, CRUDTypes.Update, sn, cn, inf));
                    if (t.GenerateDelete) methods.Add(GenerateControllerMethod(t, fromDBType, CRUDTypes.Delete, sn, cn, inf));
                }
                else
                {
                    string code = "";
                    
                    if (t.GenerateCreate) code += GenerateControllerMethod(t, fromDBType, CRUDTypes.Create, sn, cn, inf) + "\n\n";
                    if (t.GenerateRead) code += GenerateControllerMethod(t, fromDBType, CRUDTypes.Read, sn, cn, inf) + "\n\n";
                    if (t.GenerateUpdate) code += GenerateControllerMethod(t, fromDBType, CRUDTypes.Update, sn, cn, inf) + "\n\n";
                    if (t.GenerateDelete) code += GenerateControllerMethod(t, fromDBType, CRUDTypes.Delete, sn, cn, inf); 

                    methods.Add(code.Trim());
                }
            }
            return methods;
        }

        public override List<string> GenerateModels(List<SQLTable> tables, DatabaseTypes fromDBType, string projectName, GenerateSettings settings = null, bool IncludeRelevantImports = false)
        {
            List<string> models = new List<string>();
            foreach(SQLTable t in tables)
            {
                models.Add(GenerateModel(t, fromDBType, projectName, settings, IncludeRelevantImports));
            }
            return models;
        }




        /// <summary>
        /// Generates a comment block for a method
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="paramNames"></param>
        /// <param name="returns"></param>
        /// <returns></returns>
        private string metaComment(string summary, List<string> paramNames, string returns)
        {
            string ret = $"\t\t/// <summary>\n\t\t/// {summary}\n\t\t/// </summary>\n";
            foreach (string pn in paramNames)
            {
                ret += $"\t\t/// <param name=\"{pn}\"></param>\n";
            }
            ret += $"\t\t/// <returns>{returns}</returns>\n";

            return ret;
        }


        private string GenerateServiceMethod(SQLTable table, DatabaseTypes fromDBType, CRUDTypes crudType, string ORM, bool asInterface = false)
        {
            string method = "";

            string methodName = "";
            string by = "";
            string byParams = "";
            string Params = "";
            string accessible = !asInterface ? "public async" : "";
            string methodEnd = !asInterface ? "" : ";";
            string returnType = "";

            List<string> paramNamesA = new List<string>();
            List<string> paramNamesB = new List<string>();

            // Get the primary key colums to be used in method names, and parameters
            foreach (var c in table.SQLColumns)
            {
                if (c.IsIdentity || c.IsUnique)
                {
                    if (by != "")
                    {
                        by += "And";
                        byParams += ", ";
                    }
                    else
                    {
                        by = "By";
                    }

                    by += c.Name;
                    byParams += $"{c.CSharpType(fromDBType)} {c.Name}";
                    paramNamesA.Add(c.Name);
                }
                else
                {
                    if (Params != "") Params += ", ";

                    Params += $"{c.CSharpType(fromDBType)} {c.Name}";
                    paramNamesB.Add(c.Name);
                }
            }

            switch (crudType)
            {
                case CRUDTypes.Create:
                    methodName = $"{(asInterface ? metaComment("Adds a " + table.Name + " record", paramNamesB, table.Name) : "")}\t\t{accessible} Task<{table.Name}> Add{table.Name}({Params}){methodEnd}";
                    returnType = table.Name;
                    break;
                case CRUDTypes.Read:
                    methodName = $"{(asInterface ? metaComment("Gets a " + table.Name + " record", paramNamesA, table.Name) : "")}\t\t{accessible} Task<{table.Name}> Get{table.Name}{by}({byParams}){methodEnd}";
                    returnType = table.Name;
                    break;
                case CRUDTypes.Update:
                    methodName = $"{(asInterface ? metaComment("Updates a " + table.Name + " record", new List<string> { table.Name }, "bool representing success") : "")}\t\t{accessible} Task<bool> Update{table.Name}({table.Name} entity){methodEnd}";
                    returnType = "bool";
                    break;
                case CRUDTypes.Delete:
                    methodName = $"{(asInterface ? metaComment("Deletes a " + table.Name + " record", paramNamesA, "bool representing success") : "")}\t\t{accessible} Task<bool> Delete{table.Name}{by}({byParams}){methodEnd}";
                    returnType = "bool";
                    break;
            }

            if (asInterface)
            {
                return methodName;
            }

            switch (ORM.Trim().ToLower())
            {
                case "dapper":
                    method = $"{methodName}\n\t\t{{\n\t\t\t{returnType} entity = {(returnType == "bool" ? "false" : "null" )};\n{GenerateDapperMethodBody(table, fromDBType, crudType)}\n\t\t\treturn entity;\n\t\t}}";
                    break;
                case "ado":
                    method = $"{methodName}\n\t\t{{\n{GenerateADOMethodBody(table, fromDBType, crudType)}\n}}";
                    break;
            }

            return method.ToString();
        }

        private string GenerateControllerMethod(SQLTable table, DatabaseTypes fromDBType, CRUDTypes crudType, string serviceName, string controllerName, string interfaceName)
        {
            string method = "";

            string methodName = "";
            string by = "";
            string byParams = "";
            string Params = "";
            string byParamsCall = "";
            string ParamsCall = "";
            string call = "";
            string Route = "";
            string byRoute = "";
            string accessible = "public async Task<IActionResult>";
            string methodEnd = "";

            if (crudType == CRUDTypes.Constructor)
            {
                
                return $"       {interfaceName}  _{serviceName};\n"
                                 + "\n"
                                 + $"    public {controllerName}({interfaceName} {serviceName})\n"
                                 + "    {\n"
                                 + $"        _{serviceName} = {serviceName};\n"
                                 + "    }";
                
            }

            // Get the primary key colums to be used in method names, and parameters
            foreach (var c in table.SQLColumns)
            {
                if (c.IsIdentity || c.IsUnique)
                {
                    if (by != "")
                    {
                        by += "And";
                        byParams += ", ";
                        byParamsCall += ", ";
                    }
                    else
                    {
                        by = "By";
                    }

                    by += c.Name;
                    byParams += $"{c.CSharpType(fromDBType)} {c.Name}";
                    byParamsCall += $"{c.Name}";
                    byRoute += $"{{{c.Name}}}/";
                }
                else
                {
                    if (Params != "") Params += ", ";
                    if (ParamsCall != "") ParamsCall += ", ";

                    Params += $"{c.CSharpType(fromDBType)} {c.Name}";
                    Route += $"{{{c.Name}}}/";
                }
            }

            string methodNameCall = "";
            string condition = "";
            string callToService = "";

            switch (crudType)
            {
                case CRUDTypes.Constructor:
                    method = $"   I{serviceName}  _{serviceName};\n"
                                 + "\n"
                                 + $"    public {controllerName}(I{serviceName} {serviceName})\n"
                                 + "    {\n"
                                 + $"        _{serviceName} = {serviceName};\n"
                                 + "    }";
                    break;
                case CRUDTypes.Create:
                    methodName = $"[HttpPost(\"{table.Name}/{Route}\")]\n\t{accessible} Add{table.Name}({Params}){methodEnd}";
                    methodNameCall = $"Add{table.Name}";
                    call = ParamsCall;
                    method = $"\t{methodName}\n\t{{\n\t\tvar ret = await _{serviceName}.{methodNameCall}({call});\n\t\treturn Ok(ret);\n\t}}";
                    break;
                case CRUDTypes.Read:
                    methodName = $"[HttpGet(\"{table.Name}/{byRoute}\")]\n\t{accessible} Get{table.Name}{by}({byParams}){methodEnd}";
                    methodNameCall = $"Read{table.Name}{by}";
                    call = byParamsCall;
                    callToService = $"var ret = await _{serviceName}.{methodNameCall}({call});";
                    condition = $"ret != null";
                    method = $"\t{methodName}\n\t{{\n\t\t{controllerTryCatchBlock.Replace("<CODE>", controllerOkOrBadRequest.Replace("<CONDITION>", condition).Replace("<MESSAGE>", table.Name + " Not Found")).Replace("<RETURN>", "ret").Replace("<CALL>", callToService)}\n\t}}";
                    break;
                case CRUDTypes.Update:
                    methodName = $"[HttpPut(\"{table.Name}/{Route}\")]\n\t{accessible} Update{table.Name}({table.Name} entity){methodEnd}";
                    methodNameCall = $"Update{table.Name}";
                    call = ParamsCall;
                    condition = $"await _{serviceName}.{methodNameCall}({call})";
                    method = $"\t{methodName}\n\t{{\n\t\t{controllerTryCatchBlock.Replace("<CODE>", controllerNoContentBadRequest.Replace("<CONDITION>", condition).Replace("<MESSAGE>", table.Name + " Not Updated"))}\n\t}}";
                    break;
                case CRUDTypes.Delete:
                    methodName = $"[HttpDelete(\"{table.Name}/{byRoute}\")]\n\t{accessible} Delete{table.Name}{by}({byParams}){methodEnd}";
                    methodNameCall = $"Delete{table.Name}{by}";
                    call = byParamsCall;
                    condition = $"await _{serviceName}.{methodNameCall}({call})";
                    method = $"\t{methodName}\n\t{{\n\t\t{controllerTryCatchBlock.Replace("<CODE>", controllerNoContentBadRequest.Replace("<CONDITION>", condition).Replace("<MESSAGE>", table.Name + " Not Deleted"))}\n\t}}";
                    break;
            }

           

            return method;
        }


        private string GetConnectionType(DatabaseTypes fromDBType)
        {
            string myConnection = "";
            switch (fromDBType)
            {
                case DatabaseTypes.SQLServer:
                    myConnection = "SqlConnection";
                    break;
                case DatabaseTypes.MySQL:
                    myConnection = "MySqlConnection";
                    break;
            }

            return myConnection;
        }

        private string SQLWrap(string name, DatabaseTypes fromDBType)
        {
            string o = "", c = "";
            switch(fromDBType)
            {
                case DatabaseTypes.SQLServer:
                    o = "[";
                    c = "]";
                    break;
                case DatabaseTypes.MySQL:
                    o = "`";
                    c = "`";
                    break;
            }

            return o + name + c;
        }

       

        private string GenerateDapperMethodBody(SQLTable table, DatabaseTypes fromDBType, CRUDTypes crudType)
        {
            string con = GetConnectionType(fromDBType);
            string body = $"\t\t\tusing ({con} conn = new {con}(_connectionString))\n\t\t\t{{\n\t\t\t\t";

            List<string> columns = new List<string>();
            List<string> variables = new List<string>();
            List<string> ovariables = new List<string>();
            List<string> where = new List<string>();
            string returnObj = "";
            string sql = "";
            string execute = "";           

            foreach(var c in table.SQLColumns)
            {
                if (crudType == CRUDTypes.Create)
                {
                    if (!(c.IsIdentity || c.IsUnique))
                    {
                        columns.Add(c.Name);
                        variables.Add($"@{c.Name}");
                        ovariables.Add($"{c.Name} = {c.Name}");
                    }                    
                }

                if (crudType == CRUDTypes.Read)
                {
                    if (!(c.IsIdentity || c.IsUnique))
                    {
                        columns.Add($"{c.Name}");
                    }
                    else
                    {
                        where.Add($"{c.Name} = @{c.Name}");
                        ovariables.Add($"{c.Name} = {c.Name}");
                    }
                }

                if (crudType == CRUDTypes.Update)
                {
                    if (!(c.IsIdentity || c.IsUnique))
                    {
                        columns.Add($"{c.Name} = @{c.Name}");
                    }
                    else
                    {
                        where.Add($"{c.Name} = @{c.Name}");
                    }

                    ovariables.Add($"{c.Name} = {c.Name}");
                }

                if (crudType == CRUDTypes.Delete)
                {
                    if (c.IsIdentity || c.IsUnique)
                    {
                        where.Add($"{c.Name} = @{c.Name}");
                        ovariables.Add($"{c.Name} = {c.Name}");
                    }
                }
            }

            switch (crudType)
            {
                case CRUDTypes.Create:
                    returnObj = table.Name; 
                    sql = $"string sql = \"INSERT {table.Name} ({columns.ToCommaList()}) VALUES ({variables.ToCommaList()})\";";
                    execute = $"entity = await conn.ExecuteAsync(sql, new {{{ovariables.ToCommaList()}}});";
                    break;
                case CRUDTypes.Read:
                    returnObj = table.Name;
                    sql = $"string sql = \"SELECT {columns.ToCommaList()} FROM {table.Name} WHERE {where.ToCommaList()}\";";
                    execute = $"entity = await conn.QuerySingleAsync<{returnObj}>(sql, new {{{ovariables.ToCommaList()}}});";
                    break;
                case CRUDTypes.Update:
                    returnObj = $"bool";
                    sql = $"string sql = \"UPDATE {table.Name} SET {columns.ToCommaList()} WHERE {where.ToCommaList()}\";";
                    execute = $"entity = await conn.ExecuteAsync(sql, new {{{ovariables.ToCommaList()}}});";
                    break;
                case CRUDTypes.Delete:
                    returnObj = $"bool";
                    sql = $"string sql = \"DELETE {table.Name} WHERE {where.ToCommaList()}\";";
                    execute = $"entity = await conn.ExecuteAsync(sql, new {{{ovariables.ToCommaList()}}});";
                    break;
            }

            body += $"{sql}\n\t\t\t\tconn.Open();\n\t\t\t\t{execute}\n\t\t\t}}";            
            
            return body;
        }
               

        private string GenerateADOMethodBody(SQLTable table, DatabaseTypes fromDBType, CRUDTypes crudType)
        {
            string body = "";

            return body;
        }

        private string GenerateModel(SQLTable table, DatabaseTypes fromDBType, string projectName, GenerateSettings settings = null, bool IncludeRelevantImports = false)
        {
            if (settings == null) settings = new GenerateSettings();

            string props = "";
            string comment = "";
            string emptyConstructor = "";
            string fullConstructor = "";

            StringBuilder sbMainClassBlock = new StringBuilder();
            StringBuilder sbClassProperties = new StringBuilder();
            StringBuilder sbConstructorParameters = new StringBuilder();
            StringBuilder sbConstructorAssignments = new StringBuilder();

            if (settings.IncludeSerializeDecorator) sbMainClassBlock.AppendLine("[Serializable]");
            sbMainClassBlock.AppendLine($"public class {table.Name}\n{{");
            props = "{ get; set; }";
            emptyConstructor = $"\n\tpublic {table.Name}() {{}}\n";
            fullConstructor = $"\n\tpublic {table.Name}([[PARAMETERS]])\n\t{{\n[[ASSIGNMENTS]]\t}}";

            // Loop through each of the columns...
            foreach (SQLColumn sc in table.SQLColumns)
            {
                comment = sc.Comment;
                if (comment != "") comment = @"// " + comment;

                sbClassProperties.AppendLine($"\tpublic {sc.CSharpType(fromDBType)} {sc.Name} {props} {comment}");
                if (sbConstructorParameters.Length > 0) sbConstructorParameters.Append(", ");
                sbConstructorParameters.Append(sc.CSharpType(fromDBType) + " " + localVariable(sc.Name));
                sbConstructorAssignments.Append($"\t\t{sc.Name} = {localVariable(sc.Name)};\n");
            }

            string ec = (settings.IncludeEmptyConstructor == true ? emptyConstructor : "");
            string fc = (settings.IncludeFullConstructor == true ? fullConstructor.Replace("[[PARAMETERS]]", sbConstructorParameters.ToString()).Replace("[[ASSIGNMENTS]]", sbConstructorAssignments.ToString()) : "");

            sbMainClassBlock.AppendLine(sbClassProperties.ToString() + ec + fc + "}");

            string ret = $"namespace {projectName}.Models\n{{\n{sbMainClassBlock.ToString().Indent('\t', 1)}\n}}";

            //if (settings.Namespace != "")
            // {
            //    ret = ret.IncludeNamespace(settings.Namespace);
            //}

            if (settings.IncludeSerializeDecorator) ret = "using System.Xml.Serialization;\n\n" + ret;

            return ret;
        }

        private string GenerateControllerClass(string projectName, string controllerName, string route, string code)
        {
            string ret = $"using System;\nusing System.Collections.Generic;\nusing System.Net;\nusing System.Linq;\nusing System.Threading.Tasks;\nusing Microsoft.AspNetCore.Mvc;\nusing Microsoft.Extensions.Logging;\nusing {projectName}.Interfaces;\nusing {projectName}.Services;\nusing {projectName}.Models;\n\nnamespace {projectName}.Controllers\n{{\n    [ApiController]\n    [Route(\"[{route}]\")]\n    public class {controllerName}Controller : ControllerBase\n    {{\n{code.Indent('\t', 1)}\n    }}\n}}";

            return ret;
        }

        private string GenerateInterfaceClass(string projectName, string interfaceName, string code)
        {
            string ret = "using System;\n"
                         + "using System.Collections.Generic;\n"
                         + "using System.Linq;\n"
                         + "using System.Threading.Tasks;\n"
                         + $"using {projectName}.Models;\n"
                         + "\n"
                         + $"namespace {projectName}.Interfaces\n"
                         + "{\n"
                         + $"    public interface {interfaceName}\n"
                         + "    {\n"
                         + $"{code.Indent('\t', 1)}"
                         + "    }\n"
                         + "}";

            return ret;

        }

        private string GenerateServiceClass(string projectName, string serviceName, string interfaceName, string code)
        {
            string ret = "using System;\n"
                         + "using System.Collections.Generic;\n"
                         + "using System.Linq;\n"
                         + "using System.Threading.Tasks;\n"
                         + "using System.Data.SqlClient;\n"
                         + "using Dapper;\n"
                         + $"using {projectName}.Models;\n"
                         + "\n"
                         + $"namespace {projectName}.Services\n"
                         + "{\n"
                         + $"    public class {serviceName} : {interfaceName}\n"
                         + "    {\n"
                         + "        private string _connectionString;\n\n"
                         + $"{code.Indent('\t', 1)}"
                         + "    }\n"
                         + "}";

            return ret;
        }


        private string GenerateStartupClassFile(string baseNamespace, string className, Dictionary<string, string> injectServices, DatabaseTypes fromDBType)
        {
            string ret = "using System;\n" +                         
                         "using System.Collections.Generic;\n" +
                         "using System.Linq;\n" +
                         "using System.Threading.Tasks;\n" +
                         "using Microsoft.AspNetCore.Builder;\n" +
                         "using Microsoft.AspNetCore.Hosting;\n" +
                         "using Microsoft.AspNetCore.HttpsPolicy;\n" +
                         "using Microsoft.AspNetCore.Mvc;\n" +
                         "using Microsoft.Extensions.Configuration;\n" +
                         "using Microsoft.Extensions.DependencyInjection;\n" +
                         "using Microsoft.Extensions.Hosting;\n" +
                         "using Microsoft.Extensions.Logging;\n" +
                        $"using {baseNamespace}.Interfaces;\n" +
                        $"using {baseNamespace}.Services;\n\n";

            string injects = "";
            foreach(var i in injectServices)
            {
                injects += $"\t\t\tservices.AddSingleton<{i.Key}, {i.Value}>();\n";
            }

            ret += $"namespace {baseNamespace}\n{{\n\tpublic class Startup\n\t{{\n\t\tpublic Startup(IConfiguration configuration)\n\t\t{{\n\t\t\tConfiguration = configuration;\n\t\t}}\n\n\t\tpublic IConfiguration Configuration {{ get; }}\n\n\t\tpublic void ConfigureServices(IServiceCollection services)\n\t\t{{\n\t\t\tservices.AddControllers();\n\n{injects}\n\t\t}}\n\n";

            ret += $"\t\tpublic void Configure(IApplicationBuilder app, IWebHostEnvironment env)\n\t\t{{\n\t\t\tif (env.IsDevelopment())\n\t\t\t{{\n\t\t\t\tapp.UseDeveloperExceptionPage();\n\t\t\t}}\n\t\t\tapp.UseHttpsRedirection();\n\t\t\tapp.UseRouting();\n\t\t\tapp.UseAuthorization();\n\t\t\tapp.UseEndpoints(endpoints =>\n\t\t\t{{\n\t\t\t\tendpoints.MapControllers();\n\t\t\t}});\n\t\t}}\n";

            ret += $"\t}}\n}}";

            return ret;
        }

        private string GenerateInterfaceClassFile(string methodCode, string baseNamespace, string className)
        {
            string specialRefs = $"using {baseNamespace}.Models;\n";

            string ret = $"using System;\nusing System.Collections.Generic;\nusing System.Threading.Tasks;\n{specialRefs}\n\nnamespace {baseNamespace}.Interfaces\n{{\n\tpublic interface I{className}\n\t{{\n";
                                    
            ret += $"{Indent(methodCode, 1)}\n\t}}\n\n}}";

            return ret;
        }

        private string GenerateServiceClassFile(string methodCode, string baseNamespace, string className, Dictionary<string, string> injectServices, DatabaseTypes fromDBType, string orm)
        {
            List<string> Params = new List<string>();
            string Assignments = "";
            string specialRefs = "";

            switch (fromDBType)
            {
                case DatabaseTypes.MySQL:
                    specialRefs = "using MySql.Data.MySqlClient;\n";
                    break;
                case DatabaseTypes.SQLServer:
                    specialRefs = "using System.Data.SqlClient;\n";
                    break;
            }

            switch (orm.Trim().ToLower())
            {
                case "dapper":
                    specialRefs += "using Dapper;\n";
                    break;
            }

            specialRefs += $"using {baseNamespace}.Interfaces;\nusing {baseNamespace}.Models;\n";

            string ret = $"using System;\n" +
                $"using System.Collections.Generic;\n" +
                $"using System.Threading.Tasks;\n" +
                $"{specialRefs}\n\n" +
                $"namespace {baseNamespace}.Services\n" +
                $"{{\n\tpublic class {className}Service\n\t{{\n";

            ret += "\t\tprivate readonly string _connectionString;\n";
            foreach(var i in injectServices)
            {
                ret += $"\t\tprivate readonly {i.Key} {i.Value};\n";
                
                Params.Add($"{i.Key} _{i.Value}");
                Assignments += $"\t\t\t{i.Value} = _{i.Value};\n";
            }

            ret += $"\t\tpublic {className}Service({Params.ToCommaList()})\n\t\t{{\n{Assignments}\n\t\t}}\n\n";

            ret += $"{Indent(methodCode, 1)}\n\t}}\n\n}}";            

            return ret;
        }

        private string GenerateControllerClassFile(string methodCode, string baseNamespace, string className, Dictionary<string, string> injectServices)
        {
            List<string> Params = new List<string>();
            string Assignments = "";
            string specialRefs = $"using {baseNamespace}.Services;\nusing {baseNamespace}.Models;\n";

            string ret = $"using System;\n" +
                $"using System.Collections.Generic;\n" +
                $"using System.Threading.Tasks;\n" +
                $"using Microsoft.AspNetCore.Mvc;\n" +
                $"{specialRefs}\n\n" +
                $"namespace {baseNamespace}.Controllers : Controller\n" +
                $"{{\n\tpublic class {className}Controller\n\t{{\n";

            foreach (var i in injectServices)
            {
                ret += $"\t\tprivate readonly {i.Key} {i.Value};\n";

                Params.Add($"{i.Key} _{i.Value}");
                Assignments += $"\t\t\t{i.Value} = _{i.Value};\n";
            }

            ret += $"\t\tpublic {className}Controller({Params.ToCommaList()})\n\t\t{{\n{Assignments}\n\t\t}}\n\n";

            ret += $"{Indent(methodCode, 1)}\n\t}}\n\n}}";

            return ret;
        }

        private string GenerateDockerFile(string projectName)
        {
            string code = "FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base\n"
                 + "WORKDIR /app\n"
                 + "EXPOSE 80\n"
                 + "EXPOSE 443\n\n"                 
                 + "FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build\n"
                 + "WORKDIR /src\n"
                 + $"COPY [\"{projectName}/{projectName}.csproj\", \"{projectName}/\"]\n"
                 + $"RUN dotnet restore \"{projectName}/{projectName}.csproj\"\n"
                 + "COPY . .\n"
                 + $"WORKDIR \"/src/{projectName}\"\n"
                 + $"RUN dotnet build \"{projectName}.csproj\" -c Release -o /app/build\n\n"
                 + "FROM build AS publish\n"
                 + $"RUN dotnet publish \"{projectName}.csproj\" -c Release -o /app/publish\n"
                 + "FROM base AS final\n"
                 + "WORKDIR /app\n"
                 + "COPY --from=publish /app/publish .\n"
                 + $"ENTRYPOINT [\"dotnet\", \"{projectName}.dll\"]\n";

            return code;
        }

        private string GenerateProgramCS(string projectName)
        {
            string code = "using System;\n"
                 + "using System.Collections.Generic;\n"
                 + "using System.Linq;\n"
                 + "using System.Threading.Tasks;\n"
                 + "using Microsoft.AspNetCore.Hosting;\n"
                 + "using Microsoft.Extensions.Configuration;\n"
                 + "using Microsoft.Extensions.Hosting;\n"
                 + "using Microsoft.Extensions.Logging;\n"
                 + "\n\n"
                 + $"namespace {projectName}\n"
                 + "{\n"
                 + "    public class Program\n"
                 + "    {\n"
                 + "        public static void Main(string[] args)\n"
                 + "        {\n"
                 + "            CreateHostBuilder(args).Build().Run();\n"
                 + "        }\n\n"
                 + "        public static IHostBuilder CreateHostBuilder(string[] args) =>\n"
                 + "            Host.CreateDefaultBuilder(args)\n"
                 + "                .ConfigureWebHostDefaults(webBuilder =>\n"
                 + "                {\n"
                 + "                    webBuilder.UseStartup<Startup>();\n"
                 + "                });\n"
                 + "    }\n"
                 + "}";

            return code;
        }

        private string GenerateProjectCSPROJ()
        {
            string code = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">\n\n"
                 + "  <PropertyGroup>\n"
                 + "    <TargetFramework>netcoreapp3.1</TargetFramework>\n"
                 + $"    <UserSecretsId>{Guid.NewGuid()}</UserSecretsId>\n"
                 + "    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>\n"
                 + "  </PropertyGroup>\n\n"
                 + "  <ItemGroup>\n"
                 + "    <PackageReference Include=\"Dapper\" Version=\"2.0.35\" />\n"
                 + "    <PackageReference Include=\"Microsoft.VisualStudio.Azure.Containers.Tools.Targets\" Version=\"1.10.9\" />\n"
                 + "    <PackageReference Include=\"System.Data.SqlClient\" Version=\"4.8.2\" />\n"
                 + "  </ItemGroup>\n\n"
                 + "</Project>";

            return code;
        }

        private string Indent(string code, int tabs)
        {
            string ret = "";
            string[] lines = code.Split('\n');
            foreach(string l in lines)
            {
                for (int x = 0; x < tabs; x++)
                {
                    ret += "\t";
                }

                ret += l + "\n";
            }

            return ret;
        }

        private void CreateDir(string path)
        {            
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private bool WriteFile(string fileName, string text, bool overwrite = true)
        {
            bool success = false;

            try
            {
                if (File.Exists(fileName))
                {
                    if (overwrite)
                        File.Delete(fileName);
                    else
                        return false;
                }

                File.WriteAllText(fileName, text);
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            

            return success;
        }
               

        private string CleanType(string originalType)
        {
            string ot = originalType.Trim().ToUpper();
            if (ot.Contains("INT")) ot = "INT";
            if (ot.Contains("STRING")) ot = "VARCHAR(MAX)";
            if (ot.Contains("DATETIME")) ot = "DATETIME";
            if (ot.Contains("BOOL")) ot = "BIT";
            if (ot.Contains("DECIMAL")) ot = "DECIMAL";
            if (ot.Contains("GUID")) ot = "GUID";

            return ot;

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
