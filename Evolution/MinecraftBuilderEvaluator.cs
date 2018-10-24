using Newtonsoft.Json.Linq;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RunMission
        #region IPhenomeEvaluator<IBlackBox> Members
.Evolution
{
    public class MinecraftBuilderEvaluator: IPhenomeEvaluator<IBlackBox>
    {
        private ulong _evalCount;
        private bool _stopConditionSatisfied;
        private MalmoClientPool clientPool;
        private List<double> fitnessList = new List<double>(10);
        private int generation = 1;

        public MinecraftBuilderEvaluator()
        {
            clientPool = new MalmoClientPool(2);
        }

        /// <summary>
        /// Gets the total number of evaluations that have been performed.
        /// </summary>
        public ulong EvaluationCount
        {
            get { return _evalCount; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the the evolutionary algorithm/search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        public bool StopConditionSatisfied
        {
            get { return _stopConditionSatisfied; }
        }

        /// <summary>
        /// Evaluate the provided IBlackBox against the random tic-tac-toe player and return its fitness score.
        /// Each network plays 10 games against the random player and two games against the expert player.
        /// Half of the games are played as circle and half are played as x.
        /// 
        /// A win is worth 10 points, a draw is worth 1 point, and a loss is worth 0 points.
        /// </summary>
        public FitnessInfo Evaluate(IBlackBox brain)
        {
            Tuple<JToken, AgentPosition> clientInfo = clientPool.RunAvailableClient(brain);

            double fitness = calculateFitnessWall(clientInfo.Item1, clientInfo.Item2);

            // Update the fitness score of the network
            //fitness += 1;

            // Update the evaluation counter.
            _evalCount++;

            Console.WriteLine("EvalCount: " + _evalCount);
            Console.WriteLine("Fitness: " + fitness);

            // If the networks reaches a fitness of 30, stop evaluation
            if (fitness >= 30)
                _stopConditionSatisfied = true;

            // add fitness to the list before new evaluation
            if (fitnessList.Count < 10)
                fitnessList.Add(fitness);

            // if whole population got evaluated, write generation and max fitness to result file
            String path = "";
            if(System.Environment.UserName == "lema")
                path = @"C:\Users\lema\Documents\GitHub\malmoTestAgentInterface\Evolution\Results\results.csv";
            else
                path = @"C:\Users\Pierre\Documents\malmoTestAgentInterface\Evolution\Results\results.csv";

            if (fitnessList.Count == 10)
            {
                var maxFitness = fitnessList.Max();

                using (StreamWriter outputFile = new StreamWriter(path, true))
                {
                    if (generation == 1)
                    {
                        outputFile.WriteLine("Generation, Fitness");
                    }

                    outputFile.WriteLine(String.Format("{0}, {1}", generation, maxFitness));
                }

                //Console.WriteLine("Maximum fitness = " + maxFitness + " in population number " + populationNumber);
                fitnessList.Clear();
                generation++;
            }

            // Return the fitness score
            return new FitnessInfo(fitness, fitness);
        }

        //Fitness function for calculating fitness of building a wall
        private double calculateFitnessWall(JToken fitnessGrid, AgentPosition agentPosition)
        {
            bool blockOnIndex = false;

            double fitness = 0.0;
            int gridWLH = 9;

            //Overstep boundary of 30 blocks, return fitness of -1.0 to indicate that overstepping is worse than not building anything
            if(Math.Abs(agentPosition.startX - agentPosition.endX) > 30 || Math.Abs(agentPosition.startZ - agentPosition.endZ) > 30)
            {
                return -1.0;
            }

            //The agents current Y position
            double agentYPos = agentPosition.endY;

            //The agent starts at Y position 227.
            int layersBelowGroundLevel = (int)(227 - (agentYPos - ((gridWLH - 1) / 2)));

            //Disregard blocks below ground level by turning them to air.
            int disregardBlocks = 0;
            if (layersBelowGroundLevel > 0)
            {
                if (layersBelowGroundLevel > gridWLH)
                    layersBelowGroundLevel = gridWLH;
                disregardBlocks = (layersBelowGroundLevel * (gridWLH * gridWLH));
                for (int i = 0; i < disregardBlocks; i++)
                {
                    fitnessGrid[i].Replace("air");
                }
            }

            //Keeps track of height of the wall and assigns fitness according to
            //the height the block is placed at
            int currentLayer = 1;
            int currentBlockInLayer = 0;
            int blocksPerLayer = gridWLH * gridWLH;

            //Disregard blocks below groundlevel
            for (int i = disregardBlocks; i < fitnessGrid.Count(); i++)
            {
                //Check if current block is a gold ore and increase fitness if so
                if (fitnessGrid[i].ToString() == "gold_ore")
                {
                    fitness += currentLayer;

                    blockOnIndex = true;
                }

                //Check if there is a block to the right of current block
                if (i - 1 >= 0)
                {
                    if (fitnessGrid[i - 1].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 1.0;
                    }
                }

                //Check if there is a block to the left of current block
                if (i + 1 <= fitnessGrid.Count() - 1)
                {
                    if (fitnessGrid[i + 1].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 1.0;
                    }
                }

                //Check if there is a block below current block
                if (i - (gridWLH * gridWLH) >= 0)
                {
                    if (fitnessGrid[i - (gridWLH * gridWLH)].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 1.0;
                    }
                }

                //Check if there is a block on top of current block
                if (i + (gridWLH * gridWLH) <= fitnessGrid.Count() - 1)
                {
                    if (fitnessGrid[i + (gridWLH * gridWLH)].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 1.0;
                    }
                }

                //Increase current layer count and reset current block in layer position
                //if last block in layer has been checked
                if(currentBlockInLayer == blocksPerLayer - 1)
                {
                    currentLayer++;
                    currentBlockInLayer = 0;
                } else
                {
                    currentBlockInLayer++;
                }

                blockOnIndex = false;
            }
            
            return fitness;
        }


        //Fitness function for calculating fitness of connected structures
        private double calculateFitnessConStruct(JToken fitnessGrid, AgentPosition agentPosition)
        {

            bool blockOnIndex = false;

            double fitness = 0.0;
            int gridWLH = 9;

            //The agents current Y position
            double agentYPos = agentPosition.endY;

            //The agent starts at Y position 227.
            int layersBelowGroundLevel = (int)(227 - (agentYPos - ((gridWLH - 1) / 2)));

            //Disregard blocks below ground level by turning them to air.
            int disregardBlocks = 0;
            if (layersBelowGroundLevel > 0)
            {
                if (layersBelowGroundLevel > gridWLH)
                    layersBelowGroundLevel = gridWLH;
                disregardBlocks = (layersBelowGroundLevel * (gridWLH * gridWLH));
                for (int i = 0; i < disregardBlocks; i++)
                {
                    fitnessGrid[i].Replace("air");
                }
            }

            //Disregard blocks below groundlevel
            for (int i = disregardBlocks; i < fitnessGrid.Count(); i++)
            {
                //Check if current block is a gold ore and increase fitness if so
                if (fitnessGrid[i].ToString() == "gold_ore")
                {
                    fitness += 1.0;

                    blockOnIndex = true;
                }

                //Check if there is a block to the right of current block
                if (i - 1 >= 0)
                {
                    if (fitnessGrid[i - 1].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 1.0;
                    }
                }

                //Check if there is a block to the left of current block
                if (i + 1 <= fitnessGrid.Count() - 1)
                {
                    if (fitnessGrid[i + 1].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 1.0;
                    }
                }

                //Check if there is a block in back of current block
                if (i - gridWLH >= 0)
                {
                    if (fitnessGrid[i - gridWLH].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 1.0;
                    }
                }

                //Check if there is a block in front of current block
                if (i + gridWLH <= fitnessGrid.Count() - 1)
                {
                    if (fitnessGrid[i + gridWLH].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 1.0;
                    }
                }

                //Check if there is a block below current block
                if (i - (gridWLH * gridWLH) >= 0)
                {
                    if (fitnessGrid[i - (gridWLH * gridWLH)].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 2.0;
                    }
                }

                //Check if there is a block on top of current block
                if (i + (gridWLH * gridWLH) <= fitnessGrid.Count() - 1)
                {
                    if (fitnessGrid[i + (gridWLH * gridWLH)].ToString() == "gold_ore" && blockOnIndex)
                    {
                        fitness += 2.0;
                    }
                }

                blockOnIndex = false;
            }

            return fitness;
        }

        /// <summary>
        /// Returns the score for a game. Scoring is 10 for a win, 1 for a draw
        /// and 0 for a loss. Note that scores cannot be smaller than 0 because
        /// NEAT requires the fitness score to be positive.
        /// </summary>
        private int getScore()
        {
            return 0;
        }

        /// <summary>
        /// Reset the internal state of the evaluation scheme if any exists.
        /// Note. The TicTacToe problem domain has no internal state. This method does nothing.
        /// </summary>
        public void Reset()
        {
        }
        #endregion
    }
}
