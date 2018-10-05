using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunMission.Evolution
{
    public class NeatAgent
    {
        /// <summary>
        /// The neural network that this player uses to make its decision.
        /// </summary>
        public IBlackBox Brain { get; set; }

        /// <summary>
        /// Creates a new NEAT player with the specified brain.
        /// </summary>
        public NeatAgent(IBlackBox brain)
        {
            Brain = brain;
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

        // Loads the observations as 1d array
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
    }
}
