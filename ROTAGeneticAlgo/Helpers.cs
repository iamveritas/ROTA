using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROTAGeneticAlgo
{
    public static class Helpers
    {
        public static string ToPrintString(this List<BoardState> list)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (BoardState board in list)
            {
                sb.AppendLine(string.Format("{0} status: {1}\n{2}", ++idx, board.VerifyBoard(), board.ToString()));
                //sb.AppendLine(string.Format("{0} status: \n{1}", ++idx, board.ToString()));
            }
            return sb.ToString();
        }
        public static string ToPrintString(this Dictionary<int, List<BoardState>> dict)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (int key in dict.Keys)
            {
                sb.AppendLine(string.Format("{0}, count={1}, key={2}\n{3}", ++idx, dict[key].Count, key, dict[key].ToPrintString()));
            }
            return sb.ToString();
        }
    }
}
