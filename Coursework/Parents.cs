using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Coursework
{
    internal class Parents
    {
        public static (Individual, Individual) RandomParents(List<Individual> population)
        {
            int i = Individual.random.Next(0, population.Count);
            int j = -1;
            while (j == -1 || j == i) j = Individual.random.Next(0, population.Count); //j != i

            return (population[i], population[j]);
        }

        public static (Individual, Individual) PositiveAssortParents(List<Individual> population)
        {
            int i = Individual.random.Next(0, population.Count);
            Individual firistParent = new();
            Individual.InvRedo(population[i].Order, firistParent);
            int targetLate = firistParent.Late;
            List<Individual> similar = new();
            foreach (Individual j in population)
            {
                if (j == population[i]) continue;

                if (targetLate * 0.2 > Math.Abs(j.Late - firistParent.Late))
                {
                    Individual temp = new();
                    Individual.InvRedo(j.Order, temp);
                    similar.Add(temp);
                }
            }

            Individual secondParent = new();
            if (similar.Count != 0) secondParent = similar[Individual.random.Next(0, similar.Count)];
            else Individual.InvRedo(population[Individual.random.Next(0, population.Count)].Order, secondParent);

            return (firistParent, secondParent);
        }

        public static (Individual, Individual) NegativeAssortParents(List<Individual> population)
        {
            int i = Individual.random.Next(0, population.Count);
            Individual firistParent = new();
            Individual.InvRedo(population[i].Order, firistParent);
            int targetLate = firistParent.Late;
            List<Individual> different = new();
            foreach (Individual j in population)
            {
                if (j == population[i]) continue;

                if (j.Late > targetLate * 1.8 || j.Late < targetLate * 0.2)
                {
                    Individual temp = new();
                    Individual.InvRedo(j.Order, temp);
                    different.Add(temp);
                }
            }
            
            Individual secondParent = new();
            if (different.Count != 0) secondParent = different[Individual.random.Next(0, different.Count)];
            else Individual.InvRedo(population[Individual.random.Next(0,population.Count)].Order, secondParent);

            return (firistParent, secondParent);
        }
    }
}
