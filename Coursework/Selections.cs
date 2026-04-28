using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    internal class Selections
    {
        public List<Individual> RouletteSelection(List<Individual> population)
        {
            List<Individual> result = new List<Individual>();
            double sum = 0;
            List<double> cumProb = new();
            for (int i = 0; i < population.Count; i++)
            {
                sum += population[i].Fitness;
            }
            double cumulative = 0;
            for (int i = 0; i < population.Count; i++)
            {
                cumulative += population[i].Fitness / sum;
                cumProb.Add(cumulative);
            }

            do
            {
                double rand = Individual.random.NextSingle();
                int indx = cumProb.Count - 1;
                for (int i = 0; i < cumProb.Count; i++)
                {
                    if (rand <= cumProb[i]) { indx = i; break; }
                    else
                    {
                        continue;
                    }
                }
                result.Add(population[indx]);
            } while (result.Count != population.Count / 2);

            return result;
        }

        public List<Individual> TournamentSelection(List<Individual> population)
        {
            List<Individual> result = new List<Individual>();
            int tournamentSize = 2;
            int populationSize = population.Count/2;

            for ( int i = 0; i < populationSize; i++)
            {
                List<Individual> currentTournament = new List<Individual>();
                for ( int j = 0; j < tournamentSize; j++)
                {
                    int randomIdx = Individual.random.Next(population.Count);
                    currentTournament.Add(population[randomIdx]);
                }

                Individual best = currentTournament[0];
                for (int j = 1; j < currentTournament.Count; j++)
                {
                    if (currentTournament[j].Fitness > best.Fitness)
                        best = currentTournament[j];
                }

                result.Add(best);
            }

            return result;
        }

        public List<Individual> RankSelection(List<Individual> population)
        {
            List<Individual> result = new List<Individual>();
            
            var sorted = population.OrderByDescending(x =>  x.Fitness).ToList();

            List<double> probabilities = new List<double>();
            for (int i = 0; i < sorted.Count; i++)
            {
                double weight = sorted.Count - i;
                probabilities.Add(weight);
            }

            double sum = probabilities.Sum();
            for (int i = 0; i < probabilities.Count; i++) probabilities[i] /= sum;

            List<double> cumProb = new List<double>();
            double cumulative = 0;
            for (int i = 0; i < probabilities.Count; i++)
            {
                cumulative += probabilities[i];
                cumProb.Add(cumulative);
            }
            do
            {
                double rand = Individual.random.NextSingle();
                int indx = cumProb.Count - 1;
                for (int i = 0; i < cumProb.Count; i++)
                {
                    if (rand <= cumProb[i]) { indx = i; break; }
                }
                result.Add(sorted[indx]);
            } while (result.Count != population.Count / 2);

            return result;
        }
    }
}
