using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    internal class Constraints
    {
        public Individual MyConstraint(Individual individual) //суть в том, что при изменении заданных заранее ОБЗАТЕЛЬНЫХ условий, мы их насильно возвращаем
        {
            Individual result = new();
            bool fl = false;

            if (individual.Order[0] != 0)
            {
                int indx = individual.Order.IndexOf(0);
                individual.Order[indx] = individual.Order[0];
                individual.Order[0] = 0;
                fl = true;
            }
            if (individual.Order[14] != 14)
            {
                int indx = individual.Order.IndexOf(14);
                individual.Order[indx] = individual.Order[14];
                individual.Order[14] = 14;
                fl = true;
            }

            if (fl)
            {
                Individual.InvRedo(individual.Order, result);
                return result;
            }
            else return individual;
        }

        public Individual FineContraint(Individual individual)
        {
            double penaltyFactor = 1.0;

            if (individual.Order[0] != 0)
                penaltyFactor *= 0.8;

            if (individual.Order[14] != 14)
                penaltyFactor *= 0.9;

            individual.Fitness *= penaltyFactor;

            return individual;
        }

        public Individual EliminateContraint(Individual individual)
        {
            if (individual.Order[0] != 0) return null;
            if (individual.Order[14] != 14) return null;
            return individual;
        }
    }
}
