using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace MonoGame
{
    public struct Gene
    {
        public int diceId;
        public int faceId;
    };

    public struct Genome : IComparable
    {
        public Gene[] genes;
        public double fitness;

        private static Random g_random = null;

        public Genome(int nb)
        {
            if(g_random == null)
                g_random = new Random();

            genes = new Gene[nb];
            fitness = 0.0;

            CreateRandomGenes();
        }

        private void CreateRandomGenes()
        {
            for(int i = 0; i < genes.Length; i++)
            {
                genes[i].diceId = i;
                genes[i].faceId = g_random.Next(0, 6);
            }

            for(int i = 0; i < genes.Length; i++)
            {
                int swapIndex = g_random.Next(0, genes.Length);

                Gene t = genes[i];
                genes[i] = genes[swapIndex];
                genes[swapIndex] = t;
            }
        }

        int IComparable.CompareTo(object obj)
        {
            Genome gen2 = (Genome)obj;

            if(this.fitness > gen2.fitness)
                return(1);
            if(this.fitness < gen2.fitness)
                return(-1);
            else
                return(0);
        }
    }

    public class BoardGA
    {
        private int        m_popSize;
        private double     m_crossoverRate;
        private double     m_mutationRate;
        private int        m_boardWidth;
        private int        m_chromoLength;
        private Genome[]   m_population;
        private double     m_totalFitness;
        private double     m_bestFitness;
        private int        m_bestIndividual;
        private string     m_bestConfiguration;
        private int        m_bestRawFitness;
        private int        m_lastGenerationMaxScore;
        private int        m_generation;
        private int        m_indexToWork;
        private Random     m_random;
        private Board      m_board;
        private SpriteFont m_vcrFont;
        public      BoardGA(int popSize, double crossRate, double mutRate, int boardWidth, ContentManager content)
        {
            m_popSize = popSize;
            m_crossoverRate = crossRate;
            m_mutationRate = mutRate;
            m_boardWidth = boardWidth;
            m_chromoLength = boardWidth * boardWidth;

            m_population = new Genome[m_popSize];
            m_totalFitness = 0.0;
            m_bestFitness = 0.0;
            m_bestIndividual = 0;
            m_bestConfiguration = "ABCDEFGHIJKLMNOP";
            m_bestRawFitness = 0;
            m_generation = 0;
            m_indexToWork = 0;
            m_lastGenerationMaxScore = 0;

            m_random = new Random();

            m_board = new Board(boardWidth, m_bestConfiguration, content);
            m_vcrFont = content.Load<SpriteFont>("VCR");

            CreateInitialPopulation();
        }

        public void Run(int frames)
        {
            for(int i = 0; i < frames; i++)
            {
                string config = GetConfiguration(ref m_population[m_indexToWork]);

                List<SolutionWord> tmp = null;
                int rawFitness = m_board.TestConfiguration(config, ref tmp);
                m_population[m_indexToWork].fitness = rawFitness;

                m_bestRawFitness = Math.Max(m_bestRawFitness, rawFitness);

                m_indexToWork++;
                if(m_indexToWork >= m_popSize)
                {
                    m_lastGenerationMaxScore = m_bestRawFitness;
                    m_bestRawFitness = 0;
                    m_indexToWork = 0;
                    Epoch();
                }
            }
        }

        public void Render(SpriteBatch spriteBatch, float screenWidth, float screenHeight)
        {
            m_board.SetActiveConfiguration(m_bestConfiguration);

            spriteBatch.DrawString(m_vcrFont, "Generation: " + m_generation, new Vector2(15.0f, 15.0f), Color.CornflowerBlue);
            spriteBatch.DrawString(m_vcrFont, "Score: " + m_lastGenerationMaxScore.ToString(), new Vector2(15.0f, 65.0f), Color.CornflowerBlue);
            m_board.Render(spriteBatch, screenWidth, screenHeight);
        }

        private void   CreateInitialPopulation()
        {
            for(int i = 0; i < m_popSize; i++)
            {
                m_population[i] = new Genome(m_chromoLength);
            }
        }

        private char   GetGeneChracter(ref Gene gene)
        {
            if(m_boardWidth == 4)
            {
                char[] letters = null;

                switch(gene.diceId)
                {
                case 0:
                    letters = new char[]{'R', 'I', 'F', 'O', 'B', 'X'};
                    break;
                case 1:
                    letters = new char[]{'I', 'F', 'E', 'H', 'E', 'Y'};
                    break;
                case 2:
                    letters = new char[]{'D', 'E', 'N', 'O', 'W', 'S'};
                    break;
                case 3:
                    letters = new char[]{'U', 'T', 'O', 'K', 'N', 'D'};
                    break;
                case 4:
                    letters = new char[]{'H', 'M', 'S', 'R', 'A', 'O'};
                    break;
                case 5:
                    letters = new char[]{'L', 'U', 'P', 'E', 'T', 'S'};
                    break;
                case 6:
                    letters = new char[]{'A', 'C', 'I', 'T', 'O', 'A'};
                    break;
                case 7:
                    letters = new char[]{'Y', 'L', 'G', 'K', 'U', 'E'};
                    break;
                case 8:
                    letters = new char[]{'#', 'B', 'M', 'J', 'O', 'A'};
                    break;
                case 9:
                    letters = new char[]{'E', 'H', 'I', 'S', 'P', 'N'};
                    break;
                case 10:
                    letters = new char[]{'V', 'E', 'T', 'I', 'G', 'N'};
                    break;
                case 11:
                    letters = new char[]{'B', 'A', 'L', 'I', 'Y', 'T'};
                    break;
                case 12:
                    letters = new char[]{'E', 'Z', 'A', 'V', 'N', 'D'};
                    break;
                case 13:
                    letters = new char[]{'R', 'A', 'L', 'E', 'S', 'C'};
                    break;
                case 14:
                    letters = new char[]{'U', 'W', 'I', 'L', 'R', 'G'};
                    break;
                case 15:
                    letters = new char[]{'P', 'A', 'C', 'E', 'M', 'D'};
                    break;
                }

                if(letters != null)
                {
                    if(gene.faceId < letters.Length)
                    {
                        return letters[gene.faceId];
                    }
                }
            }

            return '0';
        }

        private string GetConfiguration(ref Genome genome)
        {
            string result = new string("");

            for(int i = 0; i < genome.genes.Length; i++)
            {
                result += GetGeneChracter(ref genome.genes[i]);
            }

            return result;
        }

        private void   UpdateFitnessProperties()
        {
            m_totalFitness = 0.0;
            m_bestFitness = 0.0;
            m_bestIndividual = 0;

            double minFitness = 0.0;
            for(int i = 0; i < m_popSize; i++)
            {
                minFitness = Math.Min(minFitness, m_population[i].fitness);
            }
            for(int i = 0; i < m_popSize; i++)
            {
                m_population[i].fitness -= minFitness;
            }

            for(int i = 0; i < m_popSize; i++)
            {
                m_totalFitness += m_population[i].fitness;

                if(m_population[i].fitness > m_bestFitness)
                {
                    m_bestFitness = m_population[i].fitness;
                    m_bestIndividual = i;
                }
            }

            m_bestConfiguration = GetConfiguration(ref m_population[m_bestIndividual]);
        }
        private double     GetAverageFitness(ref Genome[] pop)
        {
            double averageFitness = 0.0;

            for(int i = 0; i < pop.Length; i++)
            {
                averageFitness += pop[i].fitness;
            }
            averageFitness /= (double)pop.Length;

            return averageFitness;
        }

        private void       FitnessScaleSigma(ref Genome[] pop)
        {
            double runningTotal = 0.0;

            double averageFitness = GetAverageFitness(ref pop);

            for(int gen = 0; gen < pop.Length; gen++)
            {
                runningTotal += (pop[gen].fitness - averageFitness) *
                                (pop[gen].fitness - averageFitness);
            }

            double variance = runningTotal / (double)m_popSize;
            double sigma = Math.Sqrt(variance);

            for(int gen = 0; gen < pop.Length; gen++)
            {
                double oldFitness = pop[gen].fitness;
                pop[gen].fitness = (oldFitness - averageFitness) / 
                                   (2.0 * sigma);
            }
        }

        private void       GrabNBest(int          nBest,
                                     int          numCopies,
                                     ref Genome[] newPop,
                                     ref int      index)
        {
            Genome[] tempPopulation = new Genome[m_popSize];
            Array.Copy(m_population, tempPopulation, m_popSize);
            Array.Sort(tempPopulation);

            for(int i = 0; i < numCopies; i++)
            {
                for(int j = 0; j < nBest; j++)
                {
                    newPop[index++] = tempPopulation[m_popSize - 1 - j];
                }
            }
        }

        private ref Genome RouletteWheelSelection()
        {
            double slice = m_random.NextDouble() * m_totalFitness;
            double accumulated = 0.0;
            int chosenIndex = 0;

            for(int i = 0; i < m_popSize; i++)
            {
                accumulated += m_population[i].fitness;
                if(accumulated >= slice)
                {
                    chosenIndex = i;
                    i = m_popSize + 1;
                }
            }

            return ref m_population[chosenIndex];
        }

        private void       CrossoverPBX(ref Gene[] mum,
                                        ref Gene[] dad,
                                        ref Gene[] baby1,
                                        ref Gene[] baby2)
        {
            if(m_random.NextDouble() > m_crossoverRate || mum == dad)
            {
                baby1 = mum;
                baby2 = dad;
                return;
            }

            Array.Fill(baby1, new Gene() { diceId = -1, faceId = 0 });
            Array.Fill(baby2, new Gene() { diceId = -1, faceId = 0 });

            ArrayList positions = new ArrayList();
            ArrayList dice = new ArrayList();

            int numDice = 0;
            int pos = m_random.Next(0, mum.Length - 1);

            while(pos < mum.Length)
            {
                positions.Add(pos);
                dice.Add(mum[pos]);
                pos += m_random.Next(1, mum.Length - pos + 1);

                numDice++;
            } 

            for(int i = 0; i < numDice; i++)
            {
                baby1[(int)positions[i]] = (Gene)dice[i];
            }

            for(int i = 0; i < dad.Length; i++)
            {
                bool good = true;
                for(int j = 0; j < baby1.Length; j++)
                {
                    if(baby1[j].diceId == dad[i].diceId)
                    {
                        good = false;
                        j = baby1.Length + 1;
                    }
                }

                if(good)
                {
                    for(int j = 0; j < baby1.Length; j++)
                    {
                        if(baby1[j].diceId == -1)
                        {
                            baby1[j] = dad[i];
                            j = baby1.Length + 1;
                        }
                    }
                }
            }

            dice.Clear();

            for(int i = 0; i < numDice; i++)
            {
                dice.Add(dad[(int)positions[i]]);
            }

            for(int i = 0; i < numDice; i++)
            {
                baby2[(int)positions[i]] = (Gene)dice[i];
            }

            for(int i = 0; i < mum.Length; i++)
            {
                bool good = true;
                for(int j = 0; j < baby2.Length; j++)
                {
                    if(baby2[j].diceId == mum[i].diceId)
                    {
                        good = false;
                        j = baby2.Length + 1;
                    }
                }

                if(good)
                {
                    for(int j = 0; j < baby2.Length; j++)
                    {
                        if(baby2[j].diceId == -1)
                        {
                            baby2[j] = mum[i];
                            j = baby2.Length + 1;
                        }
                    }
                }
            }
        }

        private void       MutateIM(ref Gene[] chromo)
        {
            if(m_random.NextDouble() > m_mutationRate)
                return;
            
            int curPos = m_random.Next(0, chromo.Length);
            int otherPos = m_random.Next(0, chromo.Length);

            while(curPos == otherPos)
                otherPos = m_random.Next(0, chromo.Length);
            
            Gene t = chromo[curPos];
            chromo[curPos] = chromo[otherPos];
            chromo[otherPos] = t;
        }

        private void       MutateFace(ref Gene[] chromo)
        {
            if(m_random.NextDouble() > m_mutationRate)
                return;
            
            int curPos = m_random.Next(0, chromo.Length);
            chromo[curPos].faceId = m_random.Next(0, 6);
        }

        private void       MutateFunction(ref Gene[] chromo)
        {
            int mutateFunction = m_random.Next(0, 2);
            switch(mutateFunction)
            {
            case 0:
                MutateIM(ref chromo);
                break;
            case 1:
                MutateFace(ref chromo);
                break;
            }
        }

        private void       Epoch()
        {
            Genome[] newPopulation = new Genome[m_popSize];
            int index = 0;

            UpdateFitnessProperties();
            FitnessScaleSigma(ref m_population);
            UpdateFitnessProperties();

            // Doesn't really work when the fitness is scaled
            // GrabNBest(3, Defines.NUM_BEST_TO_ADD, ref newPopulation, ref index);

            while(index < m_popSize)
            {
                Genome mum = RouletteWheelSelection();
                Genome dad = RouletteWheelSelection();

                Genome baby1, baby2;
                baby1 = new Genome(m_chromoLength);
                baby2 = new Genome(m_chromoLength);

                CrossoverPBX(ref mum.genes, ref dad.genes, ref baby1.genes, ref baby2.genes);

                MutateFunction(ref baby1.genes);
                MutateFunction(ref baby2.genes);

                newPopulation[index++] = baby1;
                newPopulation[index++] = baby2;
            }

            m_population = newPopulation;

            m_generation++;
        }
    }
}