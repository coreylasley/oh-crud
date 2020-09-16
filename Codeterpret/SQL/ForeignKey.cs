using System.Linq;

namespace Codeterpret.SQL
{
    public class ForeignKey : Common.Common
    {
        public string Table1 { get; set; }
        public string Table2 { get; set; }
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string ConstraintName { get; set; }


        public ForeignKey()
        { }

        /// <summary>
        /// Parses and Extracts a ForeignKey Object from a SQLBlock
        /// </summary>
        /// <param name="block"></param>
        /// <param name="dbType"></param>
        public ForeignKey(SQLBlock block, DatabaseTypes dbType)
        {
            // Split the block into an array of lines
            string[] lines = block.Text.Trim().Split('\n');

            // If we are looking at a SQL Server block
            if (dbType == DatabaseTypes.SQLServer)
            {
                // There should only be 2 lines in the block
                if (lines.Length == 2)
                {
                    string cleanLine1 = lines[0].Replace("ALTER TABLE", "").Replace("WITH CHECK ADD  CONSTRAINT ", "").Replace("FOREIGN KEY", "").Replace("  ", " ").Trim();
                    string[] firstParts = cleanLine1.Split(' ');
                    if (firstParts.Length == 3)
                    {
                        ConstraintName = firstParts[1].Replace("[", "").Replace("]", "").Trim();
                        Column1 = firstParts[2].Replace("([", "").Replace("])", "").Trim();
                        string[] tn1Parts = firstParts[0].Replace("].[", " ").Replace("[", "").Replace("]", "").Trim().Split(' ');
                        if (tn1Parts.Length == 2)
                        {
                            Table1 = tn1Parts[1];
                        }
                    }

                    string[] secondParts = lines[1].Replace("REFERENCES", "").Trim().Split(' ');
                    if (secondParts.Length == 2)
                    {
                        Column2 = secondParts[1].Replace("([", "").Replace("])", "").Trim();
                        string[] tn2Parts = secondParts[0].Replace("].[", " ").Replace("[", "").Replace("]", "").Trim().Split(' ');
                        if (tn2Parts.Length == 2)
                        {
                            Table2 = tn2Parts[1];
                        }

                    }
                }
            }

            // If we are looking at a MySQL block
            if (dbType == DatabaseTypes.MySQL)
            {
                // MySQL Foreign Key's are defined within the CREATE TABLE block 

                // Loop through each line in the block
                for (int x = 0; x < lines.Count(); x++)
                {
                    // Extract the Table Name from the CREATE TABLE line
                    if (lines[x].Contains("CREATE TABLE IF NOT EXISTS"))
                    {
                        string[] dparts = lines[x].Replace("CREATE TABLE IF NOT EXISTS", "").Replace("(", "").Replace("`.`", " ").Replace("`", "").Trim().Split(' ');
                        if (dparts.Length == 2)
                        {
                            Table1 = dparts[1];
                        }
                    }

                    // If we found our FOREIGN KEY line...
                    if (lines[x].Trim().Contains("FOREIGN KEY"))
                    {
                        // The line before it contains our key name which we will use for a comment
                        ConstraintName = lines[x - 1].Replace("CONSTRAINT", "").Replace("`", "").Trim();
                        // Parse the Column Name from the current line
                        Column1 = lines[x].Replace("FOREIGN KEY", "").Replace("(`", "").Replace("`)", "").Trim();
                        // The line after will contain the second Table and second Column names, so clean up the line and split into an a array
                        string[] l3 = lines[x + 1].Replace("REFERENCES", "").Replace("`.`", " ").Replace("`", "").Replace("(", "").Replace(")", "").Trim().Split(' ');

                        // If we have 3 elements in the array...
                        if (l3.Count() == 3)
                        {
                            Table2 = l3[1];
                            Column2 = l3[2];
                        }

                        break;
                    }

                }
            }


        }

    }

}
