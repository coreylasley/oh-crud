using System.Collections.Generic;
using System.Diagnostics;


namespace Codeterpret.SQL
{
    public class CreateTableBlock
    {
        public string SQL { get; set; }
        public string Name { get; set; }
        public List<string> ReferenceTables { get; set; }

        public CreateTableBlock()
        {
            ReferenceTables = new List<string>();
        }

        /// <summary>
        /// Orders a List of CreateTableBlock objects by Dependency order
        /// </summary>
        /// <param name="originalList"></param>
        /// <returns></returns>
        public static List<CreateTableBlock> SortByDependency(List<CreateTableBlock> originalList)
        {
            List<CreateTableBlock> ret = originalList;


            // Never thought i'd be doing a bubble-like sort again :)

            int moves = 0;

            for (int g = 0; g < ret.Count; g++)
            {
                for (int y = 0; y < ret.Count; y++)
                {
                    foreach (string rt in ret[y].ReferenceTables)
                    {
                        for (int x = 0; x < ret.Count; x++)
                        {
                            if (ret[x].Name == rt && x > y)
                            {
                                Debug.WriteLine($"Moving {ret[y].Name} from pos {y.ToString()} to " + x.ToString());
                                CreateTableBlock move = ret[y];
                                ret.Remove(move);
                                ret.Insert(x, move);
                                moves++;
                            }                            
                        }
                    }
                }
            }

            Debug.WriteLine(moves.ToString() + " moves to order tables by dependency");              
            

            //ret.Reverse();

            return ret;
        }

        /// <summary>
        /// If the index of [isTableName] is lower than the index of [lowerThanTableName] return the index of [isTableName]
        /// </summary>
        /// <param name="ctbl"></param>
        /// <param name="isTableName"></param>
        /// <param name="lowerThanTableName"></param>
        /// <returns></returns>
        private static int IsLower(List<CreateTableBlock> ctbl, string isTableName, string lowerThanTableName)
        {
            int ret = -1;

            int t1p = -1;
            int t2p = -2;

            for(int x = 0; x < ctbl.Count; x++)
            {
                if (ctbl[x].Name == isTableName) t1p = x;
                if (ctbl[x].Name == lowerThanTableName) t2p = x;
                if (t1p > -1 && t2p > -1)
                    break;
            }

            if (t1p < t2p) ret = t1p;

            return ret;
        }
    }

}
