namespace Coursework
{
    internal class Program
    {
        private static int populationSize = 20;
        private static List<Individual> population = new List<Individual>();
        private static int generation = 0;

        static void Main()
        {
            Console.WriteLine("Evolutionary Genetic Algorithm");

            Console.WriteLine("\nData source:");
            Console.WriteLine("1. Read from file");
            Console.WriteLine("2. Ready-made data");
            Console.Write("Choice: ");

            int choice = (Console.ReadLine().Trim() == "1") ? 1 : 2;
            List<ProblemInstance>? instances = null;

            if (choice == 1)
            {
                Console.WriteLine("File path: ");
                string path = Console.ReadLine().Trim();
                if (!File.Exists(path)) { Console.WriteLine("File doesnt exists."); return; }
                instances = DataReader.ReadFromFile(path);
                Console.WriteLine($"Tasks count: {instances.Count}");
                if (instances.Count == 0) { Console.WriteLine("Tasks havent found."); return; }
            }

            OrderChoice initializationMethod = ChooseInitializationMethod();
            if (initializationMethod == null) return;

            var settings = ChooseAlgorithmSettings();
            populationSize = settings.PopulationSize;

            if (instances != null)
            {
                var results = new List<(int idx, int ub, int lb, int cmax, double delta)>();

                for (int i = 0; i < instances.Count; i++)
                {
                    DataReader.LoadIntoData(instances[i]);
                    generation = 0;
                    InitializePopulation(initializationMethod);
                    Console.WriteLine("\nInitial Population:");
                    PrintAllIndividuals();
                    RunEvolutionaryAlgorithm(settings);

                    var best = population.OrderByDescending(x => x.Fitness).First();
                    int cmax = best.GetCmax();
                    double delta = (double)(cmax - instances[i].LowerBound) / instances[i].LowerBound * 100.0;
                    results.Add((i + 1, instances[i].UpperBound, instances[i].LowerBound, cmax, delta));
                    Console.WriteLine($"\nTask {i + 1}: Cmax={cmax}, UB={instances[i].UpperBound}, LB={instances[i].LowerBound}, delta={delta}%");
                    GanttForm.GenerateGantt(best);
                }
                Console.WriteLine("\n" + new string('=', 150));
                Console.WriteLine("EXPEREMENT RESULTS");
                Console.WriteLine("№    UB          LB         Cmax       delta %");
                Console.WriteLine(new string('-', 75));
                foreach (var (i, ub, lb, cmax, delta) in results)
                    Console.WriteLine($"{i}     {ub}       {lb}       {cmax}         {delta}");
                Console.WriteLine(new string('-', 75));
                Console.WriteLine($"Average delta = {results.Average(r => r.delta)}%");
            }
            else
            {
                ReadyMadeDataFill();
                InitializePopulation(initializationMethod);
                Console.WriteLine("\nInitial Population:");
                PrintAllIndividuals();

                RunEvolutionaryAlgorithm(settings);
                ShowFinalResults();
            }
        }

        static OrderChoice ChooseInitializationMethod()
        {
            Console.WriteLine("\nInitial Population Method:");
            Console.WriteLine("1. Random");
            Console.WriteLine("2. Controlled Random");
            Console.WriteLine("3. Greedy Heuristic");
            Console.WriteLine("4. Hill Climbing");
            Console.WriteLine("0. Exit");
            Console.Write("Your choice: ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice == 0)
                return null;

            return choice switch
            {
                1 => Individual.RandomOrder,
                2 => Individual.ControlRandomOrder,
                3 => Individual.GreedyOrder,
                4 => Individual.HillClimbingOrder,
                _ => Individual.RandomOrder
            };
        }

        static void InitializePopulation(OrderChoice method)
        {
            population.Clear();
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(new Individual(method));
            }
        }

        static AlgorithmSettings ChooseAlgorithmSettings()
        {
            var settings = new AlgorithmSettings();

            Console.WriteLine("\nAlgorithm Settings:");

            Console.Write("Population Size: ");
            settings.PopulationSize = int.Parse(Console.ReadLine());

            Console.WriteLine("\nParent Selection:");
            Console.WriteLine("1. Random");
            Console.WriteLine("2. Positive Assortative");
            Console.WriteLine("3. Negative Assortative");
            Console.Write("Your choice: ");
            settings.ParentSelectionMethod = int.Parse(Console.ReadLine());

            Console.WriteLine("\nCrossover Operator:");
            Console.WriteLine("1. Order Crossover (OX)");
            Console.WriteLine("2. Partially Mapped Crossover (PMC)");
            Console.WriteLine("3. Position-based Crossover (PBX)");
            Console.Write("Your choice: ");
            settings.CrossoverMethod = int.Parse(Console.ReadLine());

            Console.WriteLine("\nMutation Operator:");
            Console.WriteLine("1. Swap Mutation");
            Console.WriteLine("2. Inversion Mutation");
            Console.WriteLine("3. Scramble Mutation");
            Console.Write("Your choice: ");
            settings.MutationMethod = int.Parse(Console.ReadLine());

            Console.WriteLine("\nSelection Operator:");
            Console.WriteLine("1. Roulette Selection");
            Console.WriteLine("2. Tournament Selection");
            Console.WriteLine("3. Rank Selection");
            Console.Write("Your choice: ");
            settings.SelectionMethod = int.Parse(Console.ReadLine());

            Console.WriteLine("\nConstraint Handling:");
            Console.WriteLine("1. My Method (repair)");
            Console.WriteLine("2. Penalty Method");
            Console.WriteLine("3. Elimination");
            Console.WriteLine("4. No Constraints");
            Console.Write("Your choice: ");
            settings.ConstraintMethod = int.Parse(Console.ReadLine());

            Console.WriteLine("\nGeneration Strategy:");
            Console.WriteLine("1. Elitist (p+c)");
            Console.WriteLine("2. (p,c)");
            Console.Write("Your choice: ");
            settings.GenerationStrategy = int.Parse(Console.ReadLine());

            Console.Write("\nMax Generations: ");
            settings.MaxGenerations = int.Parse(Console.ReadLine());

            Console.Write("Output Step (generations between prints): ");
            settings.OutputStep = int.Parse(Console.ReadLine());

            return settings;
        }

        static void RunEvolutionaryAlgorithm(AlgorithmSettings settings)
        {
            var constraints = new Constraints();
            var mutations = new Mutations();
            var selections = new Selections();
            var generationHandler = new Generation(population, new List<Individual>());

            Console.WriteLine("\nStarting Algorithm...");

            while (generation < settings.MaxGenerations && !StopConditionMethod(population))
            {
                generation++;

                if (generation == 1 || generation % settings.OutputStep == 0)
                {
                    Console.WriteLine($"\n=== Generation {generation} ===");

                    var bestIndividual = population.OrderByDescending(ind => ind.Fitness).First();
                    Console.WriteLine("\nBest Individual:");
                    Console.WriteLine($"  Encoding (Order): {string.Join(" ", bestIndividual.Order.Select(x => x.ToString().PadLeft(2)))}");
                    Console.WriteLine($"  Late (penalty): {bestIndividual.Late}");
                    Console.WriteLine($"  Fitness: {bestIndividual.Fitness:F6}");

                    Console.WriteLine("\nAll Individuals in Population:");
                    PrintAllIndividuals();
                }

                var parents = SelectParents(population, settings.ParentSelectionMethod);
                var children = ApplyCrossover(parents, settings.CrossoverMethod, generationHandler);
                ApplyMutation(children, settings.MutationMethod, mutations);
                ApplyConstraints(children, settings.ConstraintMethod, constraints);
                EvaluatePopulation(children);
                population = FormNewGeneration(population, children, settings, selections, generationHandler);
            }
        }

        static (Individual, Individual) SelectParents(List<Individual> pop, int method)
        {
            return method switch
            {
                1 => Parents.RandomParents(pop),
                2 => Parents.PositiveAssortParents(pop),
                3 => Parents.NegativeAssortParents(pop),
                _ => Parents.RandomParents(pop)
            };
        }

        static List<Individual> ApplyCrossover((Individual, Individual) parents, int method, Generation genHandler)
        {
            var parentList = new List<Individual> { parents.Item1, parents.Item2 };

            return method switch
            {
                1 => genHandler.OrderCrossover(parentList),
                2 => genHandler.PartiallyMappedCrossover(parentList),
                3 => genHandler.PositionbasedCrossover(parentList),
                _ => genHandler.OrderCrossover(parentList)
            };
        }

        static void ApplyMutation(List<Individual> children, int method, Mutations mutations)
        {
            foreach (var child in children)
            {
                if (Individual.random.NextDouble() < 0.3)
                {
                    switch (method)
                    {
                        case 1: mutations.SwapMutation(child); break;
                        case 2: mutations.InversionMutation(child); break;
                        case 3: mutations.ScrambleMutation(child); break;
                        default: mutations.SwapMutation(child); break;
                    }
                }
            }
        }

        static void ApplyConstraints(List<Individual> children, int method, Constraints constraints)
        {
            switch (method)
            {
                case 1:
                    for (int i = 0; i < children.Count; i++)
                        children[i] = constraints.MyConstraint(children[i]);
                    break;
                case 2:
                    for (int i = 0; i < children.Count; i++)
                        constraints.FineContraint(children[i]);
                    break;
                case 3:
                    var validChildren = new List<Individual>();
                    foreach (var child in children)
                    {
                        var processed = constraints.EliminateContraint(child);
                        if (processed != null)
                            validChildren.Add(processed);
                    }
                    children.Clear();
                    children.AddRange(validChildren);
                    break;
                case 4:
                    break;
            }
        }

        static void EvaluatePopulation(List<Individual> pop)
        {
            foreach (var ind in pop)
            {
                ind.Late = ind.LateCalculate();
                ind.Fitness = 1.0 / (ind.Late + 1);
            }
        }

        static List<Individual> FormNewGeneration(List<Individual> parents, List<Individual> children,
                                                AlgorithmSettings settings, Selections selections,
                                                Generation genHandler)
        {
            genHandler.Parents = parents;
            genHandler.Children = children;

            List<Individual> selectedParents = settings.SelectionMethod switch
            {
                1 => selections.RouletteSelection(parents),
                2 => selections.TournamentSelection(parents),
                3 => selections.RankSelection(parents),
                _ => selections.RouletteSelection(parents)
            };

            return settings.GenerationStrategy switch
            {
                1 => genHandler.MuNLambda(),
                2 => genHandler.NoMuLambda(),
                _ => genHandler.MuNLambda()
            };
        }

        static void ShowFinalResults()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("ALGORITHM FINISHED");
            Console.WriteLine($"Total Generations: {generation}");

            var bestIndividual = population.OrderByDescending(ind => ind.Fitness).First();
            var worstIndividual = population.OrderBy(ind => ind.Fitness).First();

            GanttForm.GenerateGantt(bestIndividual);

            Console.WriteLine("\n=== BEST INDIVIDUAL ===");
            Console.WriteLine($"Encoding (Order): {string.Join(" ", bestIndividual.Order.Select(x => x.ToString().PadLeft(2)))}");
            Console.WriteLine($"Late (total penalty): {bestIndividual.Late}");
            Console.WriteLine($"Fitness: {bestIndividual.Fitness:F6}");

            Console.WriteLine("\n=== WORST INDIVIDUAL ===");
            Console.WriteLine($"Late (total penalty): {worstIndividual.Late}");
            Console.WriteLine($"Fitness: {worstIndividual.Fitness:F6}");

            Console.WriteLine("\n=== POPULATION STATISTICS ===");
            Console.WriteLine($"Population Size: {population.Count}");
            Console.WriteLine($"Average Fitness: {population.Average(ind => ind.Fitness):F6}");
            Console.WriteLine($"Median Fitness: {population.OrderBy(ind => ind.Fitness).Skip(population.Count / 2).First().Fitness:F6}");
            Console.WriteLine($"Fitness Range: {population.Max(ind => ind.Fitness) - population.Min(ind => ind.Fitness):F6}");

            Console.WriteLine("\n=== FINAL POPULATION (ALL INDIVIDUALS) ===");
            PrintAllIndividuals();
        }

        static void PrintAllIndividuals()
        {
            Console.WriteLine("ID | Encoding (Order)                     | Late | Fitness");
            Console.WriteLine(new string('-', 65));

            for (int i = 0; i < population.Count; i++)
            {
                var ind = population[i];
                string orderStr = string.Join(" ", ind.Order.Select(x => x.ToString().PadLeft(2)));
                Console.WriteLine($"{i + 1,2} | {orderStr} | {ind.Late,5} | {ind.Fitness,10:F6}");
            }
        }

        public static bool StopConditionMethod(List<Individual> population)
        {
            if (population.Count == 0) return true;

            double maxFitness = population.Max(x => x.Fitness);
            double minFitness = population.Min(x => x.Fitness);
            double spread = 0.00001;

            return (maxFitness - minFitness < spread);
        }

        public static void ReadyMadeDataFill()
        {
            Data.arr = new List<List<int>> {
        new List<int> {3, 23, 7, 4, 5},
        new List<int> {13, 9, 21, 15, 6},
        new List<int> {7, 11, 9, 1, 1},
        new List<int> {15, 8, 9, 1, 17},
        new List<int> {5, 11, 7, 12, 23},
        new List<int> {5, 22, 23, 12, 5},
        new List<int> {3, 10, 6, 7, 7},
        new List<int> {17, 15, 2, 24, 9},
        new List<int> {13, 4, 8, 19, 4},
        new List<int> {3, 6, 23, 24, 16},
        new List<int> {1, 5, 23, 9, 22},
        new List<int> {3, 5, 16, 12, 20},
        new List<int> {4, 7, 1, 2, 16},
        new List<int> {4, 9, 11, 11, 22},
        new List<int> {19, 10, 16, 16, 12}
    };
            Data.deadline = new List<int> { 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90 };
            Data.NumJobs = 15;
            Data.NumMachines = 5;
        }

        class AlgorithmSettings
        {
            public int PopulationSize { get; set; } = 20;
            public int ParentSelectionMethod { get; set; } = 1;
            public int CrossoverMethod { get; set; } = 1;
            public int MutationMethod { get; set; } = 1;
            public int SelectionMethod { get; set; } = 1;
            public int ConstraintMethod { get; set; } = 1;
            public int GenerationStrategy { get; set; } = 1;
            public int MaxGenerations { get; set; } = 100;
            public int OutputStep { get; set; } = 10;
        }
    }
}