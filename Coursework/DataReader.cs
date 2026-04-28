using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    public class ProblemInstance
    {
        public int NumJobs { get; set; }
        public int NumMachines { get; set; }
        public long Seed { get; set; }
        public int UpperBound { get; set; }
        public int LowerBound { get; set; }
        public List<List<int>> ProcessingTimes { get; set; } = new();

        public override string ToString() =>
            $"Jobs={NumJobs}, Machines={NumMachines}, UB={UpperBound}, LB={LowerBound}";
    }

    public static class DataReader
    {
        // Считывает все задачи из файла и возвращает список экземпляров.
        public static List<ProblemInstance> ReadFromFile(string filePath)
        {
            var instances = new List<ProblemInstance>();
            var lines = File.ReadAllLines(filePath)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 0)
                            .ToList();

            int i = 0;
            while (i < lines.Count)
            {
                if (!lines[i].StartsWith("number of jobs", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    continue;
                }

                // Следующая строка — числа: jobs, machines, seed, UB, LB
                i++;
                if (i >= lines.Count) break;

                var headerNumbers = ParseInts(lines[i]);
                if (headerNumbers.Count < 5)
                {
                    i++;
                    continue;
                }

                var instance = new ProblemInstance
                {
                    NumJobs = headerNumbers[0],
                    NumMachines = headerNumbers[1],
                    Seed = headerNumbers[2],
                    UpperBound = headerNumbers[3],
                    LowerBound = headerNumbers[4]
                };

                i++;
                if (i >= lines.Count) break;
                if (lines[i].StartsWith("processing times", StringComparison.OrdinalIgnoreCase))
                    i++;

                // Считываем NumMachines строк — каждая строка это одна машина
                for (int m = 0; m < instance.NumMachines && i < lines.Count; m++, i++)
                {
                    // Выходим из for, если встретили следующий заголовок
                    if (lines[i].StartsWith("number of jobs", StringComparison.OrdinalIgnoreCase))
                        break;

                    var row = ParseInts(lines[i]);
                    instance.ProcessingTimes.Add(row);
                }

                instances.Add(instance);
            }

            return instances;
        }

        // Загружает одну задачу в Data.
        public static void LoadIntoData(ProblemInstance instance)
        {
            int n = instance.NumJobs;
            int m = instance.NumMachines;

            Data.arr = new List<List<int>>();
            for (int job = 0; job < n; job++)
            {
                var row = new List<int>();
                for (int machine = 0; machine < m; machine++)
                {
                    row.Add(instance.ProcessingTimes[machine][job]);
                }
                Data.arr.Add(row);
            }

            Data.deadline = Enumerable.Range(1, n)
                .Select(k => instance.UpperBound * k / n)
                .ToList();

            Data.NumJobs = n;
            Data.NumMachines = m;
        }

        private static List<int> ParseInts(string line)
        {
            return line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(t => int.TryParse(t, out int v) ? v : (int?)null)
                       .Where(v => v.HasValue)
                       .Select(v => v!.Value)
                       .ToList();
        }
    }
}
