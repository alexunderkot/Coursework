using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Coursework
{
    internal class Generation
    {
        public List<Individual> Parents { get; set; } = null;
        public List<Individual> Children { get; set; } = null;

        public Generation(List<Individual> parents, List<Individual> children)
        {
            Parents = parents;
            Children = children;
        }

        public List<Individual> MuNLambda()
        {
            List<Individual> parents = new();
            parents.AddRange(Parents);
            parents.AddRange(Children);

            return parents.OrderBy(p => p.Late).Take(Parents.Count).ToList();
        }

        public List<Individual> NoMuLambda()
        {
            if (Children.Count == 0)
                return Parents;

            List<Individual> result = new List<Individual>();

            while (result.Count < Parents.Count)
            {
                result.AddRange(Children);
            }

            return result.Take(Parents.Count).ToList();
        }

        public List<Individual> OrderCrossover(List<Individual> parents)
        {
            if (parents.Count != 2)
                throw new ArgumentException("Need exactly 2 parents");

            int indx1 = Individual.random.Next(1, parents[0].Order.Count-1);
            int indx2 = indx1;
            while (indx1 == indx2) indx2 = Individual.random.Next(2, parents[0].Order.Count);
            if (indx1 > indx2)
            {
                int temp=indx2;
                indx2 = indx1;
                indx1 = temp;
            }

            Individual child1 = new();
            Individual child2 = new();
            List<int> order = new List<int>();

            order = Enumerable.Skip(parents[0].Order, indx1).Take(indx2 - indx1 + 1).ToList();

            List<int> el = new();
            for (int i = 0; i < parents[1].Order.Count; i++)
            {
                if (order.Contains(parents[1].Order[i])) continue;

                el.Add(parents[1].Order[i]);
            }

            for (int i = 0; i < indx1; i++)
            {
                order.Insert(0, el[i]);
            }
            order.AddRange(el.Skip(indx1));
            Individual.InvRedo(order, child1);

            //first up, second down-----------------------------------------------------------------------------------------------------------
            order = new List<int>();

            order = Enumerable.Skip(parents[1].Order, indx1).Take(indx2-indx1+1).ToList();

            el = new();
            for (int i = 0; i < parents[0].Order.Count; i++)
            {
                if (order.Contains(parents[0].Order[i])) continue;

                el.Add(parents[0].Order[i]);
            }

            for (int i = 0; i < indx1; i++)
            {
                order.Insert(0, el[i]);
            }
            order.AddRange(el.Skip(indx1));
            Individual.InvRedo(order, child2);

            return new List<Individual> { child1, child2 };
        }

        public List<Individual> PartiallyMappedCrossover(List<Individual> parents)
        {
            if (parents.Count != 2)
                throw new ArgumentException("Need exactly 2 parents");

            int n = parents[0].Order.Count;
            List<int> p1 = parents[0].Order;
            List<int> p2 = parents[1].Order;

            int point1 = Individual.random.Next(0, n - 1);
            int point2 = Individual.random.Next(point1 + 1, n);

            List<int> child1 = new List<int>(new int[n]);
            List<int> child2 = new List<int>(new int[n]);

            for (int i = point1; i < point2; i++)
            {
                child1[i] = p2[i];
                child2[i] = p1[i];
            }

            for (int i = 0; i < n; i++)
            {
                if (i < point1 || i >= point2)
                {
                    int candidate = p1[i];
                    int loopCounter = 0;
                    const int MAX_LOOPS = 100;
                    while (child1.Contains(candidate) && loopCounter < MAX_LOOPS)
                    {
                        int indexInChild1 = child1.IndexOf(candidate);

                        candidate = p2[indexInChild1];
                        loopCounter++;
                    }

                    if (loopCounter >= MAX_LOOPS || child1.Contains(candidate))
                    {
                        for (int num = 0; num < n; num++)
                        {
                            if (!child1.Contains(num))
                            {
                                candidate = num;
                                break;
                            }
                        }
                    }

                    child1[i] = candidate;
                }
            }

            for (int i = 0; i < n; i++)
            {
                if (i < point1 || i >= point2)
                {
                    int candidate = p2[i];
                    int loopCounter = 0;
                    const int MAX_LOOPS = 100;

                    while (child2.Contains(candidate) && loopCounter < MAX_LOOPS)
                    {
                        int indexInChild2 = child2.IndexOf(candidate);
                        candidate = p1[indexInChild2];
                        loopCounter++;
                    }

                    if (loopCounter >= MAX_LOOPS || child2.Contains(candidate))
                    {
                        for (int num = 0; num < n; num++)
                        {
                            if (!child2.Contains(num))
                            {
                                candidate = num;
                                break;
                            }
                        }
                    }

                    child2[i] = candidate;
                }
            }

            if (!IsValidPermutation(child1))
                child1 = RepairPermutation(child1);
            if (!IsValidPermutation(child2))
                child2 = RepairPermutation(child2);

            Individual c1 = new Individual();
            Individual.InvRedo(child1, c1);

            Individual c2 = new Individual();
            Individual.InvRedo(child2, c2);

            return new List<Individual> { c1, c2 };
        }

        private bool IsValidPermutation(List<int> order)
        {
            if (order.Count != 15) return false;

            for (int i = 0; i < 15; i++)
            {
                if (order[i] < 0 || order[i] >= 15)
                    return false;
            }

            var distinct = order.Distinct().ToList();
            return distinct.Count == 15;
        }

        private List<int> RepairPermutation(List<int> order)
        {
            List<int> result = new List<int>(order);
            List<int> missing = Enumerable.Range(0, 15).ToList();

            foreach (int num in order)
            {
                if (num >= 0 && num < 15)
                    missing.Remove(num);
            }

            int missingIndex = 0;
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] < 0 || result[i] >= 15 ||
                    result.IndexOf(result[i]) != i) 
                {
                    if (missingIndex < missing.Count)
                    {
                        result[i] = missing[missingIndex];
                        missingIndex++;
                    }
                }
            }

            return result;
        }

        //PostitionbasedCrossover
        public List<Individual> PositionbasedCrossover(List<Individual> parents)
        {
            if (parents.Count != 2)
                throw new ArgumentException("Need exactly 2 parents");

            List<Individual> children = new(new Individual[parents.Count]);

            children[0] = new();
            children[1] = new();

            List<int> firstHalf = RandomOrderSplitter(parents[0].Order);
            List<int> bothHalf = Enumerable.Repeat(-1, firstHalf.Count).ToList();
            for (int j = 0; j < firstHalf.Count; j++)
            {
                if (firstHalf[j] != -1) bothHalf[j] = firstHalf[j];
                else
                {
                    for (int i = 0; i < parents[1].Order.Count; i++)
                    {
                        int order = parents[1].Order[i];
                        if (firstHalf.Contains(order) || bothHalf.Contains(order)) continue;
                        else
                        {
                            bothHalf[j] = order;
                            break;
                        }
                    }
                }
            }
            Individual.InvRedo(bothHalf, children[0]);

            firstHalf = RandomOrderSplitter(parents[1].Order); // 1 -1 3 -1 0         // 3 1 2 4 0
            bothHalf = Enumerable.Repeat(-1, firstHalf.Count).ToList(); // -1 -1 -1 -1 -1
            for (int j = 0; j < firstHalf.Count; j++)
            {
                if (firstHalf[j] != -1) bothHalf[j] = firstHalf[j];
                else
                {
                    for (int i = 0; i < parents[0].Order.Count; i++)
                    {
                        int order = parents[0].Order[i];
                        if (firstHalf.Contains(order) || bothHalf.Contains(order)) continue;
                        else
                        {
                            bothHalf[j] = order;
                            break;
                        }
                    }
                }
            }
            Individual.InvRedo(bothHalf, children[1]);


            return children;
        }
        public List<int> RandomOrderSplitter(List<int> order)
        {
            List<int> result = Enumerable.Repeat(-1, order.Count).ToList();

            for (int i = 0; i < order.Count / 2; i++)
            {
                int indx;
                do
                {
                    indx = Individual.random.Next(0, order.Count);
                }
                while (result[indx] != -1);

                result[indx] = (order[indx]);
            }
            return result;
        }
        //PostitionbasedCrossover
    }
}
