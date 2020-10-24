using System;
using Codeterpret;
using Codeterpret.Implementations.BackEnd;
using Codeterpret.SQL;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string sql = "t Person\n" +
                        "c ID, an, pk\n" +
                        "c Name varchar 50\n" +
                        "c Email varchar 100, nn\n" +
                        "c Gender, fk to Genders MorF\n" +
                        "t Genders\n" +
                        "c MorF, pk, auto\n";

            SQLTableClassBuilder stcb = new SQLTableClassBuilder(sql);

            CSharp cs = new CSharp();
            //cs.GenerateProject(stcb.SQLTables, Codeterpret.Common.Common.DatabaseTypes.SQLServer, @"c:\temp", "codeterpret_test", "dapper", true);

            Console.WriteLine("Done!!");
        }
    }
}
