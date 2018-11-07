using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunMission.Evolution
{
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
        public class MinecraftNoveltyEvaluator : IPhenomeEvaluator<IBlackBox>
        {
            private readonly int NOVELTY_THRESHOLD = 10;
            private ulong _evalCount;
            private bool _stopConditionSatisfied;
            private MalmoClientPool clientPool;

            private List<bool[]> novelBehaviourArchive = new List<bool[]>();
            private List<bool[]> currentGenerationArchive = new List<bool[]>();
            private Dictionary<ulong, int> distanceDictionary = new Dictionary<ulong, int>();
            private int generation = 1;

            public MalmoClientPool ClientPool
            {
                get => clientPool;
                set => clientPool = value;
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

            public static object myLock = new object();

            /// <summary>
            /// Evaluate the provided IBlackBox against the random tic-tac-toe player and return its fitness score.
            /// Each network plays 10 games against the random player and two games against the expert player.
            /// Half of the games are played as circle and half are played as x.
            /// 
            /// A win is worth 10 points, a draw is worth 1 point, and a loss is worth 0 points.
            /// </summary>
            public FitnessInfo Evaluate(IBlackBox brain)
            {
                distanceDictionary.Clear();

                bool[] fitnessGrid = ClientPool.RunAvailableClient(brain);

                currentGenerationArchive.Add(fitnessGrid);

                int fitness = 0;

                // Update the fitness score of the network
                //fitness += 1;

                // Update the evaluation counter

                while (currentGenerationArchive.Count != 10) {                    
                    
                }
                var distance = getDistance(fitnessGrid);

                ulong currentEval = 0;

                lock (myLock) {
                    currentEval = _evalCount;
                    _evalCount++;
                    Console.WriteLine(_evalCount);
                }


                distanceDictionary.Add(currentEval, distance);

                while(distanceDictionary.Count != 10)
                {
                }

                var highestEvalDistance = distanceDictionary.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                if (highestEvalDistance == currentEval)
                {
                    fitness = distance;
                }
                else
                    fitness = 0;


                if (distance > NOVELTY_THRESHOLD)
                {
                    novelBehaviourArchive.Add(fitnessGrid);
                }
                currentGenerationArchive.Clear();
                    // Return the fitness score
                return new FitnessInfo(fitness, fitness);
            }

            private int getDistance (bool[] fitnessGrid)
            {
                var distance = 0;
                for(int i = 0; i < currentGenerationArchive.Count; i++)
                {
                    if(fitnessGrid != currentGenerationArchive[i])
                    {
                        for (int j = 0; j < 20*20*20; j++)
                        {
                            if (fitnessGrid[j] != currentGenerationArchive[i][j])
                                distance++;
                        }
                    } else
                    {
                        Console.WriteLine("does work");
                    }
                }


                for (int i = 0; i < novelBehaviourArchive.Count; i++)
                {
                    if (fitnessGrid != novelBehaviourArchive[i])
                    {
                        for (int j = 0; j < 20 * 20 * 20; j++)
                        {
                            if (fitnessGrid[j] != novelBehaviourArchive[i][j])
                                distance++;
                        }
                    }
                    else
                    {
                        Console.WriteLine("does work");
                    }
                }

                return distance;
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

            //Method for getting the 20x20x20 grid of the confined area according to agent position, from the 41x41x41 grid
            private bool[] getStructureGrid(bool[] fitnessGrid, AgentPosition agentPosition, int gridWLH)
            {
                //The area in which the agent may place blocks are at most 20x20x20
                bool[] flattenedFitnessGrid = new bool[20 * 20 * 20];

                //The agent starts at Y position 227. Find where the layers within the area starts and ends, that are also above ground level
                int layerStartY = (int)(Math.Abs(agentPosition.initialY - (agentPosition.currentY - ((gridWLH - 1) / 2))));

                //If agent gets out of bound 20 blocks below ground level or 20 blocks above ground level, return a grid that would result in a fitness of 0
                if (agentPosition.currentY < 227 - (gridWLH / 2) || agentPosition.currentY > 227 + (gridWLH / 2))
                {
                    for (int i = 0; i < flattenedFitnessGrid.Length; i++)
                    {
                        flattenedFitnessGrid[i] = false;
                    }

                    return flattenedFitnessGrid;
                }

                //The agent starts at X position 0. Find where the layers within the area starts
                int layerStartX = (int)(Math.Abs(agentPosition.currentX - 20)) + 1;

                //The agent starts at Z position 0. Find where the layers within the area starts
                int layerStartZ = (int)(Math.Abs(agentPosition.currentZ - 20)) + 1;

                // Start at the layer above ground level and run through 20 layers from that layer in the flattened fitness grid
                int currentBlockInGrid = (layerStartY * (gridWLH * gridWLH)) + (layerStartZ * gridWLH) + layerStartX;

                int flattenedGridCounter = 0;

                for (int y = 0; y < 20; y++)
                {
                    //move one layer up in the y position
                    currentBlockInGrid = ((layerStartY + y) * (gridWLH * gridWLH)) + (layerStartZ * gridWLH) + layerStartX;

                    for (int z = 0; z < 20; z++)
                    {
                        for (int x = 0; x < 20; x++)
                        {
                            if (fitnessGrid[currentBlockInGrid].ToString() == "gold_ore")
                            {
                                flattenedFitnessGrid[flattenedGridCounter] = true;
                            }
                            else
                            {
                                flattenedFitnessGrid[flattenedGridCounter] = false;
                            }

                            currentBlockInGrid += 1;
                            flattenedGridCounter++;
                        }

                        //move to next row in the z position
                        currentBlockInGrid += gridWLH - 20;
                    }
                }

                return flattenedFitnessGrid;
            }

            #endregion
        }
    }

}
