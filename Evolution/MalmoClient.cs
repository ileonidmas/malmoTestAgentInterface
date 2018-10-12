using Microsoft.Research.Malmo;
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
        private ClientPool clientPool;

        private bool isWorldCreated = false;

        private List<string> listOfCommands;

        public MalmoClient()
        {
            isWorldCreated = false;

            clientPool = new ClientPool();
            clientPool.add(new ClientInfo("127.0.0.1", 10000));
        }

        private static readonly Object obj = new Object();
        public void RunMalmo(IBlackBox brain)
        {
            agentHost = new AgentHost();
            neatPlayer = new NeatAgentController(brain, agentHost);

            InitializeMission();

            CreateWorld();
            lock(obj){
            TryStartMission();
                        
            ConsoleOutputWhileMissionLoads();

            Console.WriteLine("Mission has started!");

            while (worldState.is_mission_running)
            {
                worldState = agentHost.getWorldState();
                if (worldState.observations.Count == 0)
                {
                     continue;
                }
                neatPlayer.AgentHelper.ConstantObservations = worldState.observations;
                neatPlayer.UpdateFitness();
                neatPlayer.PerformAction();
            }
                Thread.Sleep(2000);
            }
            agentHost.Dispose();

            Console.WriteLine("Mission has ended!");
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

        public static object MyLock;
        private void TryStartMission()
        {                                                                               
            
            try
            {
                agentHost.startMission(mission, clientPool, new MissionRecordSpec(), 0, "Test Builder");
                
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
