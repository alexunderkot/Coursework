using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    internal class Mutations
    {
        public void SwapMutation(Individual individual)
        {
            int temp, indx1 = Individual.random.Next(0, individual.Order.Count);
            int indx2=indx1;
            while (indx2==indx1) indx2 = Individual.random.Next(0, individual.Order.Count);
            temp = individual.Order[indx1];
            individual.Order[indx1] = individual.Order[indx2];
            individual.Order[indx2] = temp;
            Individual.InvRedo(individual.Order, individual);
        }

        public void InversionMutation(Individual individual)
        {
            int start = Individual.random.Next(0, individual.Order.Count - 1);
            int end = Individual.random.Next(start + 1, individual.Order.Count);

            while (start < end)
            {
                int temp = individual.Order[start];
                individual.Order[start] = individual.Order[end];
                individual.Order[end] = temp;
                start++;
                end--;
            }

            Individual.InvRedo(individual.Order, individual);
        }

        public void ScrambleMutation(Individual individual)
        {
            individual.Order = individual.Order.OrderBy(x => Individual.random.Next()).ToList();
            Individual.InvRedo(individual.Order, individual);
        }
    }
}
