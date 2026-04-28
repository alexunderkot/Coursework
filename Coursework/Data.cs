using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    static internal class Data
    {
        public static int NumJobs { get; set; } = 15;
        public static int NumMachines { get; set; } = 5;

        public static List<int> deadline = new List<int>();
        public static List<List<int>> arr = new List<List<int>>();
    }
}
