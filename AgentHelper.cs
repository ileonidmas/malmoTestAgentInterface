using Microsoft.Research.Malmo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace RunMission
{
    class AgentHelper
    {
        #region Private members
        private AgentHost agentHost;
        public enum Direction { Front, Left, Right, Back, FrontUnder, LeftUnder, RightUnder, BackUnder, FrontTop, LeftTop, RightTop, BackTop, Under };

        #endregion

        #region Constructor
        public AgentHelper(AgentHost agentHost)
        {
            this.agentHost = agentHost;
        }
        #endregion

        #region Private methods
        private void UpdateDirection(double desiredYaw, double desiredPitch, double precision = 1)
        {
            Thread.Sleep(100);
            var observations = JObject.Parse(agentHost.getWorldState().observations[0].text);

            var currentYaw = (double)observations.GetValue("Yaw");
            var currentPitch = (double)observations.GetValue("Pitch");
            var deltaYaw = 0d;
            var deltaPitch = 0d;
            do
            {
                try
                {
                    Thread.Sleep(100);
                    observations = JObject.Parse(agentHost.getWorldState().observations[0].text);

                    currentYaw = (double)observations.GetValue("Yaw");
                    currentPitch = (double)observations.GetValue("Pitch");

                    Console.WriteLine(String.Format("Yaw: {0} Pitch: {1}", currentYaw, currentPitch));
                    if (observations == null)
                        continue;
                    else
                    {

                        //https://stackoverflow.com/questions/38407584/continuous-aim-to-target-in-malmo
                        deltaYaw = desiredYaw - currentYaw;
                        if (deltaYaw > 180)
                            deltaYaw -= 360;
                        if (deltaYaw < -180)
                            deltaYaw += 360;

                        deltaPitch = desiredPitch - currentPitch;
                        if (deltaPitch > 180)
                            deltaPitch -= 360;
                        if (deltaPitch < -180)
                            deltaPitch += 360;

                        Look(currentYaw, deltaYaw, currentPitch, deltaPitch);
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Debug.WriteLine("error reading observations in Agent helper");
                }
            } while (Math.Abs(deltaYaw) > precision || Math.Abs(deltaPitch) > precision);
            agentHost.sendCommand("turn " + 0);
            agentHost.sendCommand("pitch " + 0);
        }


        private void Look(double currentYaw, double deltaYaw, double currentPitch, double deltaPitch, int precision = 1)
        {
            var perTickYaw = 18;
            var perTickPitch = 18;
            
            //Yaw control
            if (Math.Abs(deltaYaw) > precision)
            {
                if (Math.Abs(deltaYaw) - perTickYaw > 0)
                {
                    if (deltaYaw > 0)                 
                        agentHost.sendCommand(String.Format("turn {0}", 1));
                    else
                        agentHost.sendCommand(String.Format("turn {0}", -1));                  
                }
                else
                {
                    deltaYaw /= 180.0;
                    agentHost.sendCommand("turn " + FormatValue(deltaYaw));
                }
            }

            //Pitch control
            if (Math.Abs(deltaPitch) > precision)
            {                
                if (Math.Abs(deltaPitch) - perTickPitch > 0)
                {
                    if (deltaPitch > 0)
                        agentHost.sendCommand(String.Format("pitch {0}", 1));
                    else
                        agentHost.sendCommand(String.Format("pitch {0}", -1));
                }
                else
                {
                    deltaPitch /= 180.0;
                    agentHost.sendCommand("pitch " + FormatValue(deltaPitch));
                }
            }
        }

        // going to the right ( x pos becomes negative )
        // going to the left ( x pos becomes positive) 
        // going forward ( z pos becomes positive)
        // moving backward ( z begomes negative ) 
        private void MoveTo(double deltaXPos, double deltaZPos, double yaw, double precision)
        {
            var perTickMove = 0.432;
            var overStep = 0.2582;

            if (yaw > 180)
                yaw -= 360;
            if (yaw < -180)
                yaw += 360;

            if (deltaXPos > 1)
            {
                deltaXPos = 1;
            }
            else if (deltaXPos < -1)
            {
                deltaXPos = -1;
            }

            deltaXPos /= 5;

            if (Math.Abs(deltaXPos) > precision)
            {
                if ((yaw < -175 && yaw > -185) || (yaw < 185 && yaw > 175))
                {
                    if (deltaXPos > 0)
                        agentHost.sendCommand(String.Format("strafe {0}", FormatValue(Math.Abs(deltaXPos))));
                    else
                        agentHost.sendCommand(String.Format("strafe {0}", FormatValue(-1.0 * Math.Abs(deltaXPos))));
                }
                else if (yaw < 5 && yaw > -5)
                {
                    if (deltaXPos > 0)
                        agentHost.sendCommand(String.Format("strafe {0}", FormatValue(-1.0 * Math.Abs(deltaXPos))));
                    else
                        agentHost.sendCommand(String.Format("strafe {0}", FormatValue(Math.Abs(deltaXPos))));
                }
                else if (yaw < -85 && yaw > -95)
                {
                    if (deltaXPos > 0)
                        agentHost.sendCommand(String.Format("move {0}", FormatValue(Math.Abs(deltaXPos))));
                    else
                        agentHost.sendCommand(String.Format("move {0}", FormatValue(-1.0 * Math.Abs(deltaXPos))));
                }
                else if (yaw < 95 && yaw > 85)
                {
                    if (deltaXPos > 0)
                        agentHost.sendCommand(String.Format("move {0}", FormatValue(-1.0 * Math.Abs(deltaXPos))));
                    else
                        agentHost.sendCommand(String.Format("move {0}", FormatValue(Math.Abs(deltaXPos))));
                }
            }


            if(deltaZPos > 1)
            {
                deltaZPos = 1;
            } else if (deltaZPos < -1)
            {
                deltaZPos = -1;
            }

            deltaZPos /= 5;

            if (Math.Abs(deltaZPos) > precision)
            {
                if ((yaw < -175 && yaw > -185) || (yaw < 185 && yaw > 175))
                {
                    if (deltaZPos > 0)
                        agentHost.sendCommand(String.Format("move {0}", FormatValue(-1.0 * Math.Abs(deltaZPos))));
                    else
                        agentHost.sendCommand(String.Format("move {0}", FormatValue(Math.Abs(deltaZPos))));
                }
                else if (yaw < 5 && yaw > -5)
                {
                    if (deltaZPos > 0)
                        agentHost.sendCommand(String.Format("move {0}", FormatValue(Math.Abs(deltaZPos))));
                    else
                        agentHost.sendCommand(String.Format("move {0}", FormatValue(-1.0 * Math.Abs(deltaZPos))));
                }
                else if (yaw < -85 && yaw > -95)
                {
                    if (deltaZPos > 0)
                        agentHost.sendCommand(String.Format("strafe {0}", FormatValue(Math.Abs(deltaZPos))));
                    else
                        agentHost.sendCommand(String.Format("strafe {0}", FormatValue(-1.0 * Math.Abs(deltaZPos))));
                }
                else if (yaw < 95 && yaw > 85)
                {
                    if (deltaZPos > 0)
                        agentHost.sendCommand(String.Format("strafe {0}", FormatValue(-1.0 * Math.Abs(deltaZPos))));
                    else
                        agentHost.sendCommand(String.Format("strafe {0}", FormatValue(Math.Abs(deltaZPos))));
                }
            }
        }

        private string FormatValue(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region Public methods

        public void PlaceBlock(Direction where)
        {
            switch (where)
            {
                case Direction.Front:
                    UpdateDirection(0,60);
                    break;
                case Direction.Right:
                    UpdateDirection(90, 60);
                    break;
                case Direction.Back:
                    UpdateDirection(180, 60);
                    break;
                case Direction.Left:
                    UpdateDirection(270, 60);
                    break;
                case Direction.FrontUnder:
                    UpdateDirection(0, 70);
                    break;
                case Direction.RightUnder:
                    UpdateDirection(90, 70);
                    break;
                case Direction.BackUnder:
                    UpdateDirection(180, 70);
                    break;
                case Direction.LeftUnder:
                    UpdateDirection(270, 70);
                    break;
                case Direction.FrontTop:
                    UpdateDirection(0, 45);
                    break;
                case Direction.RightTop:
                    UpdateDirection(90, 45);
                    break;
                case Direction.BackTop:
                    UpdateDirection(180, 45);
                    break;
                case Direction.LeftTop:
                    UpdateDirection(270, 45);
                    break;
                case Direction.Under:
                    var observations = JObject.Parse(agentHost.getWorldState().observations[0].text);
                    var currentYaw = (double)observations.GetValue("Yaw");

                    UpdateDirection(currentYaw, 90);
                    agentHost.sendCommand("jump 1");
                    break;
            }

            agentHost.sendCommand("use 1");
            if(where == Direction.Under)
                Thread.Sleep(300);
            agentHost.sendCommand("use 0");
            Thread.Sleep(100);
            agentHost.sendCommand("use 0");
            agentHost.sendCommand("use 0");
            agentHost.sendCommand("jump 0");
        }

        public void DestroyBlock(Direction where)
        {
            switch (where)
            {
                case Direction.Front:
                    UpdateDirection(0, 60);
                    break;
                case Direction.Right:
                    UpdateDirection(90, 60);
                    break;
                case Direction.Back:
                    UpdateDirection(180, 60);
                    break;
                case Direction.Left:
                    UpdateDirection(270, 60);
                    break;
                case Direction.FrontUnder:
                    UpdateDirection(0, 70);
                    break;
                case Direction.RightUnder:
                    UpdateDirection(90, 70);
                    break;
                case Direction.BackUnder:
                    UpdateDirection(180, 70);
                    break;
                case Direction.LeftUnder:
                    UpdateDirection(270, 70);
                    break;
                case Direction.FrontTop:
                    UpdateDirection(0, 45);
                    break;
                case Direction.RightTop:
                    UpdateDirection(90, 45);
                    break;
                case Direction.BackTop:
                    UpdateDirection(180, 45);
                    break;
                case Direction.LeftTop:
                    UpdateDirection(270, 45);
                    break;
                case Direction.Under:
                    UpdateDirection(180, 90);
                    break;
            }
            
            agentHost.sendCommand("attack 1");
            Thread.Sleep(50);
            agentHost.sendCommand("attack 0");
            agentHost.sendCommand("attack 0");
            agentHost.sendCommand("attack 0");
        }



        // going to the right ( x pos becomes negative )
        // going to the left ( x pos becomes positive) 
        // going forward ( z pos becomes positive)
        // moving backward ( z begomes negative ) 
        public void Move(Direction direction, bool jump = false)
        {
            Thread.Sleep(100);
            var observations = JObject.Parse(agentHost.getWorldState().observations[0].text);
            var precision = 0.08d;
            var currentYaw = (double)observations.GetValue("Yaw");
            var currentXPos = (double)observations.GetValue("XPos");
            var currentYPos = (double)observations.GetValue("YPos");
            var currentZPos = (double)observations.GetValue("ZPos");
            var desiredXPos = 0d;
            var desiredYPos = currentYPos + 1;
            var desiredZPos = 0d;
            var deltaXPos = 1d;
            var deltaYPos = 0d;
            var deltaZPos = 1d;

            switch (direction)
            {
                case Direction.Left:
                    desiredXPos = currentXPos + 1;
                    desiredZPos = currentZPos;
                    break;
                case Direction.Front:
                    desiredXPos = currentXPos;
                    desiredZPos = currentZPos + 1;
                    break;
                case Direction.Right:
                    desiredXPos = currentXPos - 1;
                    desiredZPos = currentZPos;
                    break;
                case Direction.Back:
                    desiredXPos = currentXPos;
                    desiredZPos = currentZPos - 1;
                    break;
            }



            if (jump == true) { 
                agentHost.sendCommand("jump 1");
            }

            Thread.Sleep(100);
            observations = JObject.Parse(agentHost.getWorldState().observations[0].text);
            currentXPos = (double)observations.GetValue("XPos");
            currentYPos = (double)observations.GetValue("YPos");
            currentZPos = (double)observations.GetValue("ZPos");
            deltaXPos = desiredXPos - currentXPos;
            deltaYPos = desiredYPos - currentYPos;
            deltaZPos = desiredZPos - currentZPos;

            do
            {
                Thread.Sleep(100);
                
                MoveTo(deltaXPos, deltaZPos, currentYaw, precision);

                observations = JObject.Parse(agentHost.getWorldState().observations[0].text);
                currentXPos = (double)observations.GetValue("XPos");
                currentYPos = (double)observations.GetValue("YPos");
                currentZPos = (double)observations.GetValue("ZPos");
                deltaXPos = desiredXPos - currentXPos;
                deltaYPos = desiredYPos - currentYPos;
                deltaZPos = desiredZPos - currentZPos;

                if (deltaYPos > 0.5 && jump)
                    agentHost.sendCommand("jump 1");
                else
                    agentHost.sendCommand("jump 0");
            } while ((deltaXPos > precision || deltaXPos < -precision) ^ (Math.Abs(deltaZPos) > precision));

            agentHost.sendCommand("strafe 0");
            agentHost.sendCommand("move 0");
            agentHost.sendCommand("jump 0");
            Thread.Sleep(100);
            agentHost.sendCommand("strafe 0");
            agentHost.sendCommand("move 0");
            agentHost.sendCommand("jump 0");

            Console.WriteLine("XPOS: {0}", currentXPos);
            Console.WriteLine("ZPOS: {0}", currentZPos);
        }


        public void CheckSurroundings()
        {
            Thread.Sleep(100);
            var observations = JObject.Parse(agentHost.getWorldState().observations[0].text);
            var allBlocks = observations.GetValue("floor3x3x3");
            var something = allBlocks[0];
            Console.WriteLine(String.Format(
                "Under:{0}\n" +
                "Back under:{1}\n" +
                "Left under:{2}\n" +
                "Front under:{3}\n" +
                "Right under:{4}\n" + 
                "Back:{5}\n" +
                "Left:{6}\n" +
                "Front:{7}\n" +
                "Right:{8}\n" +
                "Back top:{9}\n" +
                "Left top:{10}\n" +
                "Front top:{11}\n" +
                "Right top:{12}\n",
                allBlocks[4], allBlocks[1], allBlocks[5], allBlocks[7], allBlocks[3],
                allBlocks[10], allBlocks[14], allBlocks[16], allBlocks[12],
                allBlocks[19], allBlocks[23], allBlocks[25], allBlocks[21]));
        }

        public void strafeLeftTest(double precision = 0.015)
        {
            Thread.Sleep(100);
            var observations = JObject.Parse(agentHost.getWorldState().observations[0].text);

            var currentYaw = (double)observations.GetValue("Yaw");
            var currentXPos = (double)observations.GetValue("XPos");
            var currentZPos = (double)observations.GetValue("ZPos");
            var desiredXPos = 0d;
            var desiredZPos = 0d;
            var deltaXPos = 1d;
            var deltaZPos = 1d;

            if (currentYaw > -1 && currentYaw < 1)
            {
                desiredXPos = currentXPos + 1d;

                do
                {
                    try
                    {
                        observations = JObject.Parse(agentHost.getWorldState().observations[0].text);
                        currentXPos = (double)observations.GetValue("XPos");
                        deltaXPos = desiredXPos - currentXPos;

                        if (deltaXPos > precision)
                        {
                            agentHost.sendCommand(String.Format("strafe {0}", FormatValue(-0.2)));
                        }

                        if (deltaXPos < 0)
                        {
                            agentHost.sendCommand(String.Format("strafe {0}", FormatValue(0.06)));
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        Debug.WriteLine("error reading observations in Agent helper");
                    }
                } while (deltaXPos > precision || deltaXPos < 0);
                agentHost.sendCommand("strafe " + 0);
            }

            if (currentYaw < -89 && currentYaw > -91)
            {
                desiredZPos = currentZPos - 1d;

                do
                {
                    try
                    {
                        observations = JObject.Parse(agentHost.getWorldState().observations[0].text);
                        currentZPos = (double)observations.GetValue("ZPos");
                        deltaZPos = desiredZPos - currentZPos;

                        if (deltaZPos < 0)
                        {
                            agentHost.sendCommand(String.Format("strafe {0}", FormatValue(-0.2)));
                        }

                        if (deltaZPos > precision)
                        {
                            agentHost.sendCommand(String.Format("strafe {0}", FormatValue(0.06)));
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        Debug.WriteLine("error reading observations in Agent helper");
                    }
                } while (deltaZPos > precision || deltaZPos < 0);
                agentHost.sendCommand("strafe " + 0);
            }

            if (currentYaw > 179 && currentYaw < 181)
            {
                desiredXPos = currentXPos - 1d;

                do
                {
                    try
                    {
                        observations = JObject.Parse(agentHost.getWorldState().observations[0].text);
                        currentXPos = (double)observations.GetValue("XPos");
                        deltaXPos = desiredXPos - currentXPos;

                        Console.WriteLine(deltaXPos);

                        if (deltaXPos < 0)
                        {
                            agentHost.sendCommand(String.Format("strafe {0}", FormatValue(-0.2)));
                        }

                        if (deltaXPos > precision)
                        {
                            agentHost.sendCommand(String.Format("strafe {0}", FormatValue(0.06)));
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        Debug.WriteLine("error reading observations in Agent helper");
                    }
                } while (deltaXPos > precision || deltaXPos < 0);
                agentHost.sendCommand("strafe " + 0);
            }

            if (currentYaw > 89 && currentYaw < 91)
            {
                desiredZPos = currentZPos + 1d;

                do
                {
                    try
                    {
                        observations = JObject.Parse(agentHost.getWorldState().observations[0].text);
                        currentZPos = (double)observations.GetValue("ZPos");
                        deltaZPos = desiredZPos - currentZPos;

                        if (deltaZPos > precision)
                        {
                            agentHost.sendCommand(String.Format("strafe {0}", FormatValue(-0.2)));
                        }

                        if (deltaZPos < 0)
                        {
                            agentHost.sendCommand(String.Format("strafe {0}", FormatValue(0.06)));
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        Debug.WriteLine("error reading observations in Agent helper");
                    }
                } while (deltaZPos > precision || deltaZPos < 0);
                agentHost.sendCommand("strafe " + 0);
            }
        }
        #endregion
    }
}
