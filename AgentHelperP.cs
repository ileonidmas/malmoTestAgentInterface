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
    class AgentHelperP
    {
        private AgentHost agentHost;

        public AgentHelperP(AgentHost agentHost)
        {
            this.agentHost = agentHost;
        }
        public AgentHost AgentHost => agentHost;

        public void UpdateDirection(double desiredYaw, double desiredPitch)
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
            } while (Math.Round(currentYaw, 0) != desiredYaw || Math.Round(currentPitch, 0) != desiredPitch);
            agentHost.sendCommand("turn " + 0);
            agentHost.sendCommand("pitch " + 0);
        }
        
        private void Look(double currentYaw, double desiredYaw, double currentPitch, double desiredPitch, int tickCountYaw, double remainingTickYawVal, int tickCountPitch, double remainingTickPitchVal)
        {
            var perTickYaw = 18;
            var perTickPitch = 6;
            
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
                        agentHost.sendCommand(String.Format("turn {0}", 1));
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

        private string FormatValue(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
