using Microsoft.Research.Malmo;
using RunMission.Evolution.Enums;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunMission.Evolution
{
    public class NeatAgentController
    {
        public double Fitness { get; set; }
        /// <summary>
        /// The neural network that this player uses to make its decision.
        /// </summary>
        public IBlackBox Brain { get; set; }
        private AgentHelper agentHelper;
        public AgentHelper AgentHelper
        {
            get => agentHelper;
            set => agentHelper = value;
        }

        /// <summary>
        /// Creates a new NEAT player with the specified brain.
        /// </summary>
        public NeatAgentController(IBlackBox brain, AgentHost agentHost)
        {
            Brain = brain;
            agentHelper = new AgentHelper(agentHost);
        }


        public void PerformAction()
        {
                                                   
            // Clear the network
            Brain.ResetState();
            // Get observations
            var observations = agentHelper.CheckSurroundings();
            // Convert the world observations into an input array for the network
            setInputSignalArray(Brain.InputSignalArray, observations);
            // Activate the network
            Brain.Activate();
            // Convert the action and perform the command
            outputToCommands();

        }

        // Method for passing observations as inputs for the ANN
        private void setInputSignalArray(ISignalArray inputArr, string[] board)
        {
            inputArr[0] = blockToInt(board[0]);
            inputArr[1] = blockToInt(board[1]);
            inputArr[2] = blockToInt(board[2]);
            inputArr[3] = blockToInt(board[3]);
            inputArr[4] = blockToInt(board[4]);
            inputArr[5] = blockToInt(board[5]);
            inputArr[6] = blockToInt(board[6]);
            inputArr[7] = blockToInt(board[7]);
            inputArr[8] = blockToInt(board[8]);
            inputArr[9] = blockToInt(board[9]);
            inputArr[10] = blockToInt(board[10]);
            inputArr[11] = blockToInt(board[11]);
            inputArr[12] = blockToInt(board[12]);

        }

        private int blockToInt(string block)
        {
            if (block == "air")
                return 0;
            return 1;

        }

        // Method for passing outputs of the neural network to the client
        private void outputToCommands()
        {
            bool actionIsPerformed = false;

            double move = Brain.OutputSignalArray[0];
            double placeBlock = Brain.OutputSignalArray[1];
            double destroyBlock = Brain.OutputSignalArray[2];

            double directionRaw = Brain.OutputSignalArray[3];

            Direction direction = Direction.Under;

            if(directionRaw < (1d / 13d))
            {
                direction = Direction.Under;
            } else if (directionRaw >= (1d / 13d) && directionRaw < (2d / 13d))
            {
                direction = Direction.BackUnder;
            } else if (directionRaw >= (2d / 13d) && directionRaw < (3d / 13d))
            {
                direction = Direction.LeftUnder;
            } else if (directionRaw >= (3d / 13d) && directionRaw < (4d / 13d))
            {
                direction = Direction.FrontUnder;
            } else if (directionRaw >= (4d / 13d) && directionRaw < (5d / 13d))
            {
                direction = Direction.RightUnder;
            } else if (directionRaw >= (5d / 13d) && directionRaw < (6d / 13d))
            {
                direction = Direction.Back;
            } else if (directionRaw >= (6d / 13d) && directionRaw < (7d / 13d))
            {
                direction = Direction.Left;
            } else if (directionRaw >= (7d / 13d) && directionRaw < (8d / 13d))
            {
                direction = Direction.Front;
            } else if (directionRaw >= (8d / 13d) && directionRaw < (9d / 13d))
            {
                direction = Direction.Right;
            } else if (directionRaw >= (9d / 13d) && directionRaw < (10d / 13d))
            {
                direction = Direction.BackTop;
            } else if (directionRaw >= (10d / 13d) && directionRaw < (11d / 13d))
            {
                direction = Direction.LeftTop;
            } else if (directionRaw >= (11d / 13d) && directionRaw < (12d / 13d))
            {
                direction = Direction.FrontTop;
            } else if (directionRaw >= (12d / 13d) && directionRaw < (13d / 13d))
            {
                direction = Direction.RightTop;
            }

            if (move > placeBlock && move > destroyBlock && direction != Direction.Under)
            {
                if (direction == Direction.BackUnder || direction == Direction.BackTop)
                {
                    direction = Direction.Back;                
                } else if (direction == Direction.RightUnder || direction == Direction.RightTop)
                {
                    direction = Direction.Right;
                } else if (direction == Direction.FrontUnder || direction == Direction.FrontTop)
                {
                    direction = Direction.Front;
                } else if (direction == Direction.LeftUnder || direction == Direction.LeftTop)
                {
                    direction = Direction.Left;
                }
                if (agentHelper.CanMoveThisDirection(direction))
                {
                    agentHelper.Move(direction, agentHelper.ShouldJumpDirection(direction));

                    actionIsPerformed = true;

                    Console.WriteLine(String.Format("Move action"));
                } 

            } else if (placeBlock > destroyBlock)
            {
                if (!agentHelper.IsThereABlock(direction))
                {
                    if (direction == Direction.BackTop && !agentHelper.IsThereABlock(Direction.Back))
                    {
                        Console.WriteLine(String.Format("No action"));
                        return;
                    } else if (direction == Direction.RightTop && !agentHelper.IsThereABlock(Direction.Right))
                    {
                        Console.WriteLine(String.Format("No action"));
                        return;
                    } else if (direction == Direction.FrontTop && !agentHelper.IsThereABlock(Direction.Front))
                    {
                        Console.WriteLine(String.Format("No action"));
                        return;
                    } else if (direction == Direction.LeftTop && !agentHelper.IsThereABlock(Direction.Left))
                    {
                        Console.WriteLine(String.Format("No action"));
                        return;
                    } else if (direction == Direction.Back && !agentHelper.IsThereABlock(Direction.BackUnder))
                    {
                        Console.WriteLine(String.Format("No action"));
                        return;
                    }
                    else if (direction == Direction.Right && !agentHelper.IsThereABlock(Direction.RightUnder))
                    {
                        Console.WriteLine(String.Format("No action"));
                        return;
                    }
                    else if (direction == Direction.Front && !agentHelper.IsThereABlock(Direction.FrontUnder))
                    {
                        Console.WriteLine(String.Format("No action"));
                        return;
                    }
                    else if (direction == Direction.Left && !agentHelper.IsThereABlock(Direction.LeftUnder))
                    {
                        Console.WriteLine(String.Format("No action"));
                        return;
                    }

                    agentHelper.PlaceBlock(direction);
                    actionIsPerformed = true;
                    Console.WriteLine(String.Format("Place action"));

                }
            } else
            {
                if (agentHelper.IsThereABlock(direction))
                {
                    agentHelper.DestroyBlock(direction);

                    Console.WriteLine(String.Format("Destroy action"));

                    actionIsPerformed = true;
                }
            }
            if (!actionIsPerformed)
            {
                Console.WriteLine(String.Format("No action"));
            }
        }

        //public void UpdateFitness()
        //{
        //    Fitness = agentHelper.CalculateGrid();
        //}
    }
}
