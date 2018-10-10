using Microsoft.Research.Malmo;
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

        public void RunMalmo()
        {
            agentHost = new AgentHost();

            InitializeMission();

            CreateWorld();

            if (!TryStartMission())
            {
                return;
            }

            ConsoleOutputWhileMissionLoads();

            Console.WriteLine("Mission has started!");

            agentPerform();

            agentHost.Dispose();

            Console.WriteLine("Mission has ended!");
        }

        private void InitializeMission()
        {
            string missionXMLpath = "C:\\Users\\Pierre\\Documents\\malmoTestAgentInterface\\";
            missionXMLpath += "myworld.xml";
            
            string rawMissionXML = SaferRead(missionXMLpath);

            try
            {
                mission = new MissionSpec(rawMissionXML, true);
            }
            catch (Exception ex)
            {
                string errorLine = "Fatal error when starting a mission in ProgramMalmo: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("\nFatal error when starting a mission in ProgramMalmo: " + ex.Message);
                Environment.Exit(1);
            }

            mission.setModeToCreative();
            mission.timeLimitInSeconds(10000);
        }

        private bool TryStartMission()
        {
            bool returnValue = true;
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
            return returnValue;
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

        private void agentPerform()
        {
            listOfCommands = new List<string>();
            Thread.Sleep(100);

            while(worldState.is_mission_running)
            {
                worldState = agentHost.getWorldState();
                //ExecuteCommands();
                listOfCommands = new List<string>();
            }
        }

        public void AddCommandToList(string newCommand)
        {
            listOfCommands.Add(newCommand);
        }

        private void ExecuteCommands()
        {
            try
            {
                foreach (string command in listOfCommands)
                {
                    agentHost.sendCommand(command);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error executing commands at ProgramMalmo: " + ex.Message);
                string errorLine = "Error executing commands at ProgramMalmo: " + ex.Message;
            }
        }

        private void ConsoleOutputWhileMissionLoads()
        {
            worldState = agentHost.getWorldState();
            while (!worldState.has_mission_begun)
            {
                Console.Write(".");
                Thread.Sleep(100);
                worldState = agentHost.getWorldState();
            }
        }
    }
}
