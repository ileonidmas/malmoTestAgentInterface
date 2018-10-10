﻿using RunMission.Evolution.Enums;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunMission.Evolution
{
    public class NeatAgentController
    {
        /// <summary>
        /// The neural network that this player uses to make its decision.
        /// </summary>
        public IBlackBox Brain { get; set; }
        private MalmoClient client;
        private AgentHelper agentHelper;

        /// <summary>
        /// Creates a new NEAT player with the specified brain.
        /// </summary>
        public NeatAgentController(IBlackBox brain, MalmoClient malmoClient)
        {
            Brain = brain;
            client = malmoClient;

            agentHelper = new AgentHelper(client.agentHost);
        }

        /// <summary>
        /// This method will take numbers and pass them as commands to MalmoClient.
        /// Malmo works with dot as separator (0.9) and we must make sure
        /// numbers are not printed using comas (0,9) which would not work.
        /// </summary>
        void SetDotAsDecimalSeparator()
        {
            System.Globalization.CultureInfo customCulture =
                    (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }

        public void GetAction(string[] observations)
        {
            // Clear the network
            Brain.ResetState();

            // Convert the world observations into an input array for the network
            setInputSignalArray(Brain.InputSignalArray, observations);

            // Activate the network
            Brain.Activate();

            // Find the highest-scoring available move


            //return Action;
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

            if(directionRaw < (double)(1 / 13))
            {
                direction = Direction.Under;
            } else if (directionRaw >= (double)(1 / 13) && directionRaw < (double)(2 / 13))
            {
                direction = Direction.BackUnder;
            } else if (directionRaw >= (double)(2 / 13) && directionRaw < (double)(3 / 13))
            {
                direction = Direction.LeftUnder;
            } else if (directionRaw >= (double)(3 / 13) && directionRaw < (double)(4 / 13))
            {
                direction = Direction.FrontUnder;
            } else if (directionRaw >= (double)(4 / 13) && directionRaw < (double)(5 / 13))
            {
                direction = Direction.RightUnder;
            } else if (directionRaw >= (double)(5 / 13) && directionRaw < (double)(6 / 13))
            {
                direction = Direction.Back;
            } else if (directionRaw >= (double)(6 / 13) && directionRaw < (double)(7 / 13))
            {
                direction = Direction.Left;
            } else if (directionRaw >= (double)(7 / 13) && directionRaw < (double)(8 / 13))
            {
                direction = Direction.Front;
            } else if (directionRaw >= (double)(8 / 13) && directionRaw < (double)(9 / 13))
            {
                direction = Direction.Right;
            } else if (directionRaw >= (double)(9 / 13) && directionRaw < (double)(10 / 13))
            {
                direction = Direction.BackTop;
            } else if (directionRaw >= (double)(10 / 13) && directionRaw < (double)(11 / 13))
            {
                direction = Direction.LeftTop;
            } else if (directionRaw >= (double)(11 / 13) && directionRaw < (double)(12 / 13))
            {
                direction = Direction.FrontTop;
            } else if (directionRaw >= (double)(12 / 13) && directionRaw < (double)(13 / 13))
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
                } 

            } else if (placeBlock > destroyBlock)
            {
                if (!agentHelper.IsThereABlock(direction))
                {
                    agentHelper.PlaceBlock(direction);

                    actionIsPerformed = true;
                }
            } else
            {
                if (agentHelper.IsThereABlock(direction))
                {
                    agentHelper.DestroyBlock(direction);

                    actionIsPerformed = true;
                }
            }
        }
    }
}
