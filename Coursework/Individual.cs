using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    public delegate List<int> OrderChoice();

    internal class Individual
    {
        public static Random random = new Random();
        public List<int> Order { get; set; }
        public int Late { get; set; }
        public double Fitness { get; set; }
        public List<List<int>> StartTime { get; set; } = new();
        public List<List<int>> EndTime { get; set; } = new();

        public Individual(OrderChoice order = null)
        {
            order ??= RandomOrder;

            Order = order();
            Late = LateCalculate();
            Fitness = 1.0 / (Late + 1);
        }

        public override string ToString()
        {
            string st = "";
            st += "\nOrder: ";
            st += String.Join(" -> ", Order);
            st += "\nLate: ";
            st += String.Join(" , ", Late);
            st += "\nFitness: ";
            st += string.Join(" , ", Fitness);

            return st;
        }

        static public List<int> GreedyOrder()
        {
            List<int> availableTasks = Enumerable.Range(0, Data.deadline.Count).ToList();
            List<int> remainingDeadlines = new List<int>(Data.deadline);
            List<List<int>> remainingTasks = new List<List<int>>(Data.arr);
            List<int> originalTaskNumbers = Enumerable.Range(1, Data.deadline.Count).ToList();

            List<int> order = new List<int>();

            while (availableTasks.Count > 0)
            {
                int selectedIndex = RouletteByDeadline(remainingDeadlines, remainingTasks, originalTaskNumbers);

                order.Add(availableTasks[selectedIndex]);

                remainingDeadlines.RemoveAt(selectedIndex);
                remainingTasks.RemoveAt(selectedIndex);
                availableTasks.RemoveAt(selectedIndex);
                originalTaskNumbers.RemoveAt(selectedIndex);
            }

            return order;
        }
        static private int RouletteByDeadline(List<int> deadlines, List<List<int>> arr, List<int> originalTaskNumbers)
        {
            Random rand = new Random(Environment.TickCount * Guid.NewGuid().GetHashCode());

            double[] weight = new double[deadlines.Count];
            double sum = 0;

            for (int i = 0; i < deadlines.Count; i++)
            {
                weight[i] = Math.Round(1.0 / deadlines[i], 5);
                sum += weight[i];
            }

            double z = Math.Round(rand.NextDouble() * sum, 3);

            double cumulativeSum = 0;
            for (int i = 0; i < deadlines.Count; i++)
            {
                if (z <= cumulativeSum + weight[i])
                {
                    return i;
                }
                cumulativeSum += weight[i];
            }

            return deadlines.Count - 1;
        }

        static public List<int> ControlRandomOrder()
        {
            Individual individual = new();
            List<int> order = individual.Order;

            for (int i = 0; i < order.Count - 1; i++)
            {
                List<int> orderNew = new List<int>(order);
                int temp1, temp2;
                temp1 = orderNew[i];
                temp2 = orderNew[i + 1];
                int r1 = random.Next(0, order.Count / 2);
                int r2 = random.Next(order.Count / 2, order.Count);
                orderNew[i] = orderNew[r1]; orderNew[i + 1] = orderNew[r2]; orderNew[r1] = temp1; orderNew[r2] = temp2;

                Individual individualNew = new();
                InvRedo(orderNew, individualNew);
                if (individual.Late > individualNew.Late) { individual = individualNew; order = orderNew; }
            }

            return order;
        }
        static public void InvRedo(List<int> order, Individual individual)
        {
            individual.Order = order;
            individual.Late = individual.LateCalculate();
            individual.Fitness = 1.0 / (individual.Late + 1);
            return;
        }

        static public List<int> RandomOrder()
        {
            int n = Data.NumJobs;
            int[] ints = new int[n];
            for (int i = 0; i < n; i++) ints[i] = i;
            random.Shuffle(ints);

            return ints.ToList<int>();
        }

        public List<List<int>> SortInOrder()
        {
            List<List<int>> ans = new();
            foreach (int i in Order)
            {
                ans.Add(Data.arr[i]);
            }
            return ans;
        }

        public int LateCalculate()
        {
            int n = Data.NumJobs;
            int m = Data.NumMachines;
            int sum = 0;
            List<List<int>> jobTime = new();
            List<List<int>> sortedArr = SortInOrder();
            List<int> sortedDeadline = new();
            foreach (int o in Order)
            {
                sortedDeadline.Add(Data.deadline[o]);
            }
            for (int i = 0; i < n; i++) jobTime.Add(Enumerable.Repeat(0, m).ToList());

            jobTime[0][0] = sortedArr[0][0];

            //два цикла для 0х строк и потом двойной для заполнения остатка матрицы
            for (int j = 1; j < n; j++)
            {
                jobTime[j][0] = sortedArr[j][0] + jobTime[j - 1][0];
            }
            for (int i = 1; i < m; i++)
            {
                jobTime[0][i] = sortedArr[0][i] + jobTime[0][i - 1];
            }
            for (int j = 1; j < n; j++)
            {
                for (int i = 1; i < m; i++)
                {
                    jobTime[j][i] = Math.Max(jobTime[j][i - 1], jobTime[j - 1][i]) + sortedArr[j][i];
                }
                sum += Math.Max(0, jobTime[j][m-1] - sortedDeadline[j]);
            }

            EndTime = jobTime;
            StartTime = new List<List<int>>();
            for (int j = 0; j < Data.NumJobs; j++)
            {
                var row = new List<int>();
                for (int i = 0; i < Data.NumMachines; i++)
                    row.Add(jobTime[j][i] - sortedArr[j][i]);
                StartTime.Add(row);
            }

            return sum;
        }

        public int GetCmax()
        {
            List<List<int>> jobTime = new();
            List<List<int>> sortedArr = SortInOrder();
            for (int i = 0; i < Data.NumJobs; i++)
                jobTime.Add(Enumerable.Repeat(0, Data.NumMachines).ToList());

            jobTime[0][0] = sortedArr[0][0];
            for (int j = 1; j < Data.NumJobs; j++)
                jobTime[j][0] = sortedArr[j][0] + jobTime[j - 1][0];
            for (int i = 1; i < Data.NumMachines; i++)
                jobTime[0][i] = sortedArr[0][i] + jobTime[0][i - 1];
            for (int j = 1; j < Data.NumJobs; j++)
                for (int i = 1; i < Data.NumMachines; i++)
                    jobTime[j][i] = Math.Max(jobTime[j][i - 1], jobTime[j - 1][i]) + sortedArr[j][i];

            return jobTime[Data.NumJobs - 1][Data.NumMachines - 1];
        }

        static public List<int> HillClimbingOrder()
        {
            Individual current = new Individual(RandomOrder);

            bool improved = true;
            while (improved)
            {
                improved = false;

                for (int i = 0; i < current.Order.Count - 1; i++)
                {
                    for (int j = i + 1; j < current.Order.Count; j++)
                    {
                        List<int> newOrder = new List<int>(current.Order);
                        int temp = newOrder[i];
                        newOrder[i] = newOrder[j];
                        newOrder[j] = temp;

                        Individual neighbour = new Individual();
                        InvRedo(newOrder, neighbour);

                        if (neighbour.Late < current.Late)
                        {
                            current = neighbour;
                            improved = true;
                        }
                    }
                }
            }

            return current.Order;
        }
    }
}
