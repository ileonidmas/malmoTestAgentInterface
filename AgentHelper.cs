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
    public class AgentHelper
    {

        private AgentHost agentHost;

        public AgentHelper(AgentHost agentHost)
        {
            this.agentHost = agentHost;
        }
        public AgentHost AgentHost => agentHost;
        
        public void UpdateDirection(double desiredYaw, double desiredPitch)
        {
            var currentYaw = 0d;
            var currentPitch = 0d;
            do
            {
                try
                {
                    Thread.Sleep(100);
                    var observations = JObject.Parse(agentHost.getWorldState().observations[0].text);

                    currentYaw = (double)observations.GetValue("Yaw");
                    currentPitch = (double)observations.GetValue("Pitch");
                    if (observations == null)
                        continue;
                    else
                    {
                        Debug.WriteLine("Current yaw " + currentYaw);
                        Look(currentYaw, desiredYaw, currentPitch, desiredPitch);
                    }
                } catch (ArgumentOutOfRangeException ex)
                {
                    Debug.WriteLine("error reading observations in Agent helper");
                }
            } while (Math.Round(currentYaw,1) != desiredYaw && Math.Round(currentPitch,1) != desiredPitch);
            agentHost.sendCommand("turn " + 0);
        }

        bool alreadyMovingLeft = false;
        bool alreadyMovingRight = false;
        private void Look(double currentYaw, double desiredYaw, double currentPitch, double desiredPitch)
        {
            //https://stackoverflow.com/questions/38407584/continuous-aim-to-target-in-malmo
            var deltaYaw = desiredYaw - currentYaw;
            while (deltaYaw < -180)
                deltaYaw += 360;
            while (deltaYaw > 180)
                deltaYaw -= 360;
            deltaYaw /= 180.0;
            string deltaYawFormated = FormatValue(deltaYaw);
            var deltaPitch = desiredPitch - currentPitch;
            while (deltaPitch < -180)
                deltaPitch += 360;
            while (deltaPitch > 180)
                deltaPitch -= 360;
            deltaPitch /= 180.0;
            string deltaPitchFormated = FormatValue(deltaPitch);
            agentHost.sendCommand("turn " + deltaYawFormated);
            agentHost.sendCommand("pitch " + deltaPitchFormated);

            
        }

        private string FormatValue(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
