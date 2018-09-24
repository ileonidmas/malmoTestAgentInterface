﻿using Microsoft.Research.Malmo;
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
    class AgentHelperP
    {
        #region Private members
        private AgentHost agentHost;
        public enum Direction { Front, Left, Right, Back, FrontUnder, LeftUnder, RightUnder, BackUnder, FrontTop, LeftTop, RightTop, BackTop, Under };

        #endregion

        #region Constructor
        public AgentHelperP(AgentHost agentHost)
        {
            this.agentHost = agentHost;
        }
        #endregion

        #region Private methods
        private void UpdateDirection(double desiredYaw, double desiredPitch, double precision = 1)
        {
            var currentYaw = 0d;
            var currentPitch = 0d;
            var deltaYaw = 0d;
            var deltaPitch = 0d;
            do
            {
                try
                {
                    Thread.Sleep(100);
                    var observations = JObject.Parse(agentHost.getWorldState().observations[0].text);

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
            if (Math.Abs(deltaYaw) > 1)
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
                    UpdateDirection(180, 90);
                    agentHost.sendCommand("jump 1");
                    break;
            }

            agentHost.sendCommand("use 1");
            if(where == Direction.Under)
                Thread.Sleep(500);
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
            agentHost.sendCommand("attack 0");
        }
        #endregion
    }
}
