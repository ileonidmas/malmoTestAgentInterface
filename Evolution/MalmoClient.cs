using Microsoft.Research.Malmo;
using Newtonsoft.Json.Linq;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RunMission.Evolution
{
    public class MalmoClient
    {
        public NeatAgentController neatPlayer { get; set; }
        public AgentHost agentHost { get; set; }
        private MissionSpec mission;
        private WorldState worldState;
        private ClientPool availableClients;

        private bool isWorldCreated = false;

        private AgentPosition agentPosition;

        public MalmoClient(ClientPool clientPool)
        {
            isWorldCreated = false;

            availableClients = clientPool;
        }
        
        public void RunMalmo(IBlackBox brain)
        {
            agentHost = new AgentHost();
            neatPlayer = new NeatAgentController(brain, agentHost);

            InitializeMission();

            CreateWorld();
            TryStartMission();
                        
            ConsoleOutputWhileMissionLoads();

            Console.WriteLine("Mission has started!");
            
            agentPosition = new AgentPosition();
            bool gotStartPosition = false;

            while (worldState.is_mission_running)
            {
                //Give observations to agent and let agent perform actions according to these
                worldState = agentHost.getWorldState();
                if (worldState.observations.Count == 0)
                {
                     continue;
                }
                neatPlayer.AgentHelper.ConstantObservations = worldState.observations;
                //neatPlayer.UpdateFitness();
                neatPlayer.PerformAction();

                //Get end position and fitness grid after every performed action by the agent
                if (worldState.observations != null)
                {
                    var observations = JObject.Parse(worldState.observations[0].text);

                    if(!gotStartPosition)
                    {
                        agentPosition.startX = (double)observations.GetValue("XPos");
                        agentPosition.startY = (double)observations.GetValue("YPos");
                        agentPosition.startZ = (double)observations.GetValue("ZPos");

                        gotStartPosition = true;
                    }

                    agentPosition.endX = (double)observations.GetValue("XPos");
                    agentPosition.endY = (double)observations.GetValue("YPos");
                    agentPosition.endZ = (double)observations.GetValue("ZPos");

                    neatPlayer.AgentHelper.FitnessGrid = observations.GetValue("floor9x9x9");
                }
            }

            Thread.Sleep(2000);
            agentHost.Dispose();

            Console.WriteLine("Mission has ended!");
        }

        public JToken GetFitnessGrid()
        {
            return neatPlayer.AgentHelper.FitnessGrid;
        }

        public AgentPosition GetAgentPosition()
        {
            return agentPosition;
        }

        private void InitializeMission()
        {
            // string xml;
            string missionXMLpath = "";
            if (System.Environment.UserName == "lema")
                missionXMLpath = System.IO.File.ReadAllText(@"C:\Users\lema\Documents\GitHub\malmoTestAgentInterface\myworld.xml");
            else
                missionXMLpath = System.IO.File.ReadAllText(@"C:\Users\Pierre\Documents\malmoTestAgentInterface\myworld.xml");
            mission = new MissionSpec(missionXMLpath, false);
            mission.setModeToCreative();            
        }
        
        private void TryStartMission()
        {                                                                               
            
            try
            {
                agentHost.startMission(mission, availableClients, new MissionRecordSpec(), 0, "Test Builder");
                
             }
             catch (Exception ex)
             {
                string errorLine = "Fatal error when starting a mission in ProgramMalmo: " + ex.Message;
                Console.WriteLine("Error starting mission: {0}", ex.Message);
                Environment.Exit(1);
             } 
        }

        private string SaferRead(string path)
        {
            string newFile = null;
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    newFile = reader.ReadToEnd();
                }
            }
            return newFile;
        }

        private void CreateWorld()
        {
            if (!isWorldCreated)
            {
                mission.forceWorldReset();
                isWorldCreated = true;
            }
        }
        private void ConsoleOutputWhileMissionLoads()
        {
            worldState = agentHost.getWorldState();
            while (!worldState.has_mission_begun)
            {
                //Console.Write(".");
                Thread.Sleep(100);
                worldState = agentHost.getWorldState();
            }
        }
    }
}
