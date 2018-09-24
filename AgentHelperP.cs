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
        private AgentHost agentHost;
        public enum Direction { Front, Left, Right, Back, BottomFront, BottomLeft, BottomRight, BottomBack , TopFront, TopLeft, TopRight, TopBack, Under };
        public AgentHelperP(AgentHost agentHost)
        {
            this.agentHost = agentHost;
        }
        public AgentHost AgentHost => agentHost;

        public void UpdateDirection(double desiredYaw, double desiredPitch, double precision = 0.1)
        {
            var currentYaw = 0d;
            var currentPitch = 0d;

            var tickCountYaw = 0;
            var remainingTickYawVal = desiredYaw;

            var tickCountPitch = 0;
            var remainingTickPitchVal = desiredPitch;

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
                        Debug.WriteLine("Current yaw " + currentYaw);
                        Look(currentYaw, desiredYaw, currentPitch, desiredPitch, tickCountYaw, remainingTickYawVal, tickCountPitch, remainingTickPitchVal);

                        remainingTickYawVal = currentYaw - remainingTickYawVal;
                        tickCountYaw++;

                        remainingTickPitchVal = currentPitch - remainingTickPitchVal;
                        tickCountPitch++;
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Debug.WriteLine("error reading observations in Agent helper");
                }
            } while (Math.Abs(currentYaw - desiredYaw) > precision || Math.Abs(currentPitch - desiredPitch) > precision);
            agentHost.sendCommand("turn " + 0);
            agentHost.sendCommand("pitch " + 0);
        }
        
        private void Look(double currentYaw, double desiredYaw, double currentPitch, double desiredPitch, int tickCountYaw, double remainingTickYawVal, int tickCountPitch, double remainingTickPitchVal)
        {
            var perTickYaw = 18;
            var perTickPitch = 18;
            
            Console.WriteLine(Math.Round(currentYaw, 0));
            Console.WriteLine(Math.Round(currentPitch, 0));
            //Yaw control
            if (Math.Round(currentYaw, 0) != desiredYaw)
            {
                if (tickCountYaw < (desiredYaw / perTickYaw) - 1)
                {
                    remainingTickYawVal = (desiredYaw - (tickCountYaw * perTickYaw));
                    Console.WriteLine(remainingTickYawVal);

                    if (remainingTickYawVal > perTickYaw)
                    {
                        agentHost.sendCommand(String.Format("turn {0}", 1));
                        tickCountYaw++;
                    }
                }
                else
                {
                    //https://stackoverflow.com/questions/38407584/continuous-aim-to-target-in-malmo
                    var deltaYaw = desiredYaw - currentYaw;
                    while (deltaYaw < -180)
                        deltaYaw += 360;
                    while (deltaYaw > 180)
                        deltaYaw -= 360;
                    deltaYaw /= 180.0;
                    string deltaYawFormated = FormatValue(deltaYaw);

                    agentHost.sendCommand("turn " + deltaYawFormated);
                }
            } 

            //Pitch control
            if (Math.Round(currentPitch, 0) != desiredPitch)
            {
                if (tickCountPitch < (desiredPitch / perTickPitch) - 1)
                {
                    remainingTickPitchVal = (desiredPitch - (tickCountPitch * perTickPitch));
                    Console.WriteLine(remainingTickPitchVal);

                    if (remainingTickPitchVal > perTickPitch)
                    {
                        agentHost.sendCommand(String.Format("pitch {0}", 1));
                        tickCountPitch++;
                    }
                }
                else
                {
                    //https://stackoverflow.com/questions/38407584/continuous-aim-to-target-in-malmo
                    var deltaPitch = desiredPitch - currentPitch;
                    while (deltaPitch < -180)
                        deltaPitch += 360;
                    while (deltaPitch > 180)
                        deltaPitch -= 360;
                    deltaPitch /= 180.0;
                    string deltaPitchFormated = FormatValue(deltaPitch);

                    agentHost.sendCommand("pitch " + deltaPitchFormated);
                }
            }
        }

        

        public void PlaceBlock(Direction where)
        {
            switch (where)
            {
                case Direction.Front:
                    UpdateDirection(0,60, 1);
                    break;
                case Direction.Right:
                    UpdateDirection(90, 60, 1);
                    break;
                case Direction.Back:
                    UpdateDirection(180, 60, 1);
                    break;
                case Direction.Left:
                    UpdateDirection(270, 60, 1);
                    break;
                case Direction.BottomFront:
                    UpdateDirection(0, 60, 1);
                    break;
                case Direction.BottomRight:
                    UpdateDirection(90, 60, 1);
                    break;
                case Direction.BottomBack:
                    UpdateDirection(180, 60, 1);
                    break;
                case Direction.BottomLeft:
                    UpdateDirection(270, 60, 1);
                    break;
                case Direction.TopFront:
                    UpdateDirection(0, 45, 1);
                    break;
                case Direction.TopRight:
                    UpdateDirection(90, 45, 1);
                    break;
                case Direction.TopBack:
                    UpdateDirection(180, 45, 1);
                    break;
                case Direction.TopLeft:
                    UpdateDirection(270, 45, 1);
                    break;
                case Direction.Under:
                    UpdateDirection(180, 90, 1);
                    agentHost.sendCommand("jump 1");
                    break;
            }

            agentHost.sendCommand("use 1");
            Thread.Sleep(500);
            agentHost.sendCommand("use 0");
            agentHost.sendCommand("jump 0");
        }

        public void DestroyBlock(Direction where)
        {
            switch (where)
            {
                case Direction.Front:
                    UpdateDirection(0, 0);
                    break;
                case Direction.Right:
                    UpdateDirection(0, 0);
                    break;
                case Direction.Back:
                    UpdateDirection(0, 0);
                    break;
                case Direction.Left:
                    UpdateDirection(0, 0);
                    break;
                case Direction.BottomFront:
                    UpdateDirection(0, 0);
                    break;
                case Direction.BottomRight:
                    UpdateDirection(0, 0);
                    break;
                case Direction.BottomBack:
                    UpdateDirection(0, 0);
                    break;
                case Direction.BottomLeft:
                    UpdateDirection(0, 0);
                    break;
                case Direction.TopFront:
                    UpdateDirection(0, 0);
                    break;
                case Direction.TopRight:
                    UpdateDirection(0, 0);
                    break;
                case Direction.TopBack:
                    UpdateDirection(0, 0);
                    break;
                case Direction.TopLeft:
                    UpdateDirection(0, 0);
                    break;
            }


            agentHost.sendCommand("attack 1");
            agentHost.sendCommand("attack 0");
        }

        private string FormatValue(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
