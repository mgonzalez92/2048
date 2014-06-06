using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nn2048
{
    class GA
    {
        const int RANGE = 1;

        //Main function
        public static void Compute(ANN[] ann)
        {
            Mate(ann);
            Mutate(ann);
        }

        //Mate and crossover
        static void Mate(ANN[] ann)
        {
            int popSize = ann.Length;
            int bestSize = popSize / 4;
            Random random = new Random();
            int inputs = ann[0].neuronLayers[0].neuronNum;
            int hidden = ann[0].neuronLayers[1].neuronNum;
            int output = ann[0].neuronLayers[2].neuronNum;

            //Fittest half of population
            ANN[] best = new ANN[bestSize];
            //Half to replace
            ANN[] children = new ANN[popSize - bestSize];
            for (int i = 0; i < popSize - bestSize; i++)
            {
                children[i] = new ANN(inputs, hidden, output);
            }

            /**********************/
            /*** Find most fit  ***/
            /**********************/
            for (int i = 0; i < bestSize; i++)
            {
                //For each network
                for (int j = 0; j < popSize; j++)
                {
                    //If this network hasn't already been selected
                    if (!best.Contains(ann[j]))
                    {
                        //Set default best network
                        if (best[i] == null)
                            best[i] = ann[j];
                        //If this network is better, choose that one
                        else if (ann[j].fitness > best[i].fitness)
                            best[i] = ann[j];
                    }
                }
            }

            //Find total fitness of best half
            int totalFitness = 0;
            for (int i = 0; i < bestSize; i++)
            {
                totalFitness += best[i].fitness;
            }

            /******************/
            /*** Crossover  ***/
            /******************/
            for (int i = 0; i < popSize - bestSize; i += 2)
            {
                //Pick two random different parents
                ANN[] parents = new ANN[2];
                for (int j = 0; j < 2; j++)
                {
                    int parentFitness = random.Next(0, totalFitness);
                    int currentFitness = 0;
                    //Search best half for random parent
                    for (int k = 0; k < bestSize; k++)
                    {
                        currentFitness += best[k].fitness;
                        if (currentFitness > parentFitness)
                        {
                            parents[j] = best[k];
                            break;
                        }
                    }
                }

                //Find number of genes (weights)
                int numWeights = (inputs + 1) * hidden + (hidden + 1) * output;
                int cutLength = numWeights / 2;

                //Get two slices of genes
                int division1 = random.Next(0, numWeights - cutLength);
                int division2 = division1 + cutLength;

                //Splice together gene slices
                int ii = 0;
                for (int j = 1; j < 3; j++)
                {
                    for (int k = 0; k < ann[i].neuronLayers[j].neuronNum; k++)
                    {
                        for (int l = 0; l < ann[i].neuronLayers[j].neurons[k].inputNum + 1; l++)
                        {
                            if (ii < division1 || ii >= division2)
                            {
                                children[i].neuronLayers[j].neurons[k].weights[l] = parents[0].neuronLayers[j].neurons[k].weights[l];
                                children[i + 1].neuronLayers[j].neurons[k].weights[l] = parents[1].neuronLayers[j].neurons[k].weights[l];
                            }
                            else
                            {
                                children[i].neuronLayers[j].neurons[k].weights[l] = parents[1].neuronLayers[j].neurons[k].weights[l];
                                children[i + 1].neuronLayers[j].neurons[k].weights[l] = parents[0].neuronLayers[j].neurons[k].weights[l];
                            }
                            ii++;
                        }
                    }
                }
            }
            //Replace ANN with best half and new children
            best.CopyTo(ann, 0);
            children.CopyTo(ann, best.Length);
        }

        //Mutate genes
        static public void Mutate(ANN[] ann)
        {
            Random random = new Random();

            //For each child
            for (int i = ann.Length / 4; i < ann.Length; i++)
            {
                //For each gene
                for (int j = 1; j < 3; j++)
                {
                    for (int k = 0; k < ann[i].neuronLayers[j].neuronNum; k++)
                    {
                        for (int l = 0; l < ann[i].neuronLayers[j].neurons[k].inputNum + 1; l++)
                        {
                            bool mutate = (random.NextDouble() < 0.005);
                            if (mutate)
                                ann[i].neuronLayers[j].neurons[k].weights[l] *= (RANGE * random.NextDouble()) - RANGE;
                        }
                    }
                }
            }
        }
    }
}
