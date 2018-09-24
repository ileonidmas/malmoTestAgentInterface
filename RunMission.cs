// --------------------------------------------------------------------------------------------------
//  Copyright (c) 2016 Microsoft Corporation
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
//  associated documentation files (the "Software"), to deal in the Software without restriction,
//  including without limitation the rights to use, copy, modify, merge, publish, distribute,
//  sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all copies or
//  substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
//  NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
//  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Research.Malmo;
using Newtonsoft.Json.Linq;
using RunMission;

class Program
{
    public static void Main()
    {
        AgentHost agentHost = new AgentHost();
        AgentHelperP agentHelper = new AgentHelperP(agentHost);
        try
        {
            var something = Environment.GetCommandLineArgs();
            Console.WriteLine(something);
            agentHost.parse( new StringVector( something  ) );
        }
        catch( Exception ex )
        {
            Console.Error.WriteLine("ERROR: {0}", ex.Message);
            Console.Error.WriteLine(agentHost.getUsage());
            Environment.Exit(1);
        }
        if( agentHost.receivedArgument("help") )
        {
            Console.Error.WriteLine(agentHost.getUsage());
            Environment.Exit(0);
        }
        float startX = -230.5f, startY = 227.5f, startZ = -555.5f;
        /*MissionSpec mission = new MissionSpec();
        mission.timeLimitInSeconds(10000);
        //mission.requestVideo( 320, 240 );
        //mission.rewardForReachingPosition(19.5f,0.0f,19.5f,100.0f,1.1f);
        mission.startAt(startX,startY,startZ);
        */
        MissionRecordSpec missionRecord = new MissionRecordSpec("./saved_data.tgz");
        
        missionRecord.recordCommands();
       // missionRecord.recordMP4(20, 400000);
        //missionRecord.recordRewards();
        missionRecord.recordObservations();

        //https://microsoft.github.io/malmo/0.14.0/Schemas/Mission.html#element_AgentStart
        //https://microsoft.github.io/malmo/0.14.0/Schemas/Mission.html#element_InventoryItem
        var xml = System.IO.File.ReadAllText(@"C:\Users\lema\Documents\GitHub\malmoTestAgentInterface\myworld.xml");
        MissionSpec mission = new MissionSpec(xml, false);
        mission.setModeToCreative();
        int attempts = 0;
        bool connected = false;
        while (!connected)
        {
            try
            {
                attempts += 1;
                agentHost.startMission(mission, missionRecord);
                connected = true;
            }
            catch (MissionException ex)
            {
                // Using catch(Exception ex) would also work, but specifying MissionException allows
                // us to access the error code:
                Console.Error.WriteLine("Error starting mission: {0}", ex.Message);
                Console.Error.WriteLine("Error code: {0}", ex.getMissionErrorCode());
                // We can do more specific error handling using this code, eg:
                if (ex.getMissionErrorCode() == MissionException.MissionErrorCode.MISSION_INSUFFICIENT_CLIENTS_AVAILABLE)
                    Console.Error.WriteLine("Have you started a Minecraft client?");
                if (attempts >= 3)   // Give up after three goes.
                    Environment.Exit(1);
                Thread.Sleep(1000); // Wait a second and try again.
            }
        }
        WorldState worldState;

        Console.WriteLine("Waiting for the mission to start");
        do
        {
            Console.Write(".");
            Thread.Sleep(100);
            worldState = agentHost.getWorldState();

            foreach (TimestampedString error in worldState.errors) Console.Error.WriteLine("Error: {0}", error.text);
        }
        while (!worldState.has_mission_begun);
        
        Console.WriteLine();

        Random rand = new Random();
        // main loop:
        //agentHost.sendCommand("jump 1");

        Thread.Sleep(500);
        bool goForward = true;

        bool runonce = false;
        do
        {
            try
            {
                Thread.Sleep(100);
                worldState = agentHost.getWorldState();
                var observations = worldState.observations[0].text;
                var actualObservation = worldState.observations[0].text;
                var observationJson = JObject.Parse(observations);


                var yaw = observationJson.GetValue("Yaw");
                var xpos = observationJson.GetValue("XPos");
                var ypos = observationJson.GetValue("YPos");
                var zpos = observationJson.GetValue("ZPos");

                //Console.WriteLine(String.Format("XPos: {0} YPos: {1} ZPos: {2} Yaw: {3} Acc: {4}", xpos, ypos,zpos, yaw, a));
                if ((double)zpos <= -553 && goForward)
                    agentHost.sendCommand("move 1");
                else
                {
                    if (Math.Round((double)yaw) != 180)
                    {
                        goForward = false;
                        agentHost.sendCommand("move 0");
                        //agentHost.sendCommand("turn -0.5");
                        agentHelper.UpdateDirection(180,60);
                    } else
                    {
                        if (!runonce)
                        {
                            agentHost.sendCommand("turn 0");
                            agentHost.sendCommand("attack 1");
                            agentHost.sendCommand("attack 0");
                            Thread.Sleep(50);
                            agentHost.sendCommand("use 1");
                            agentHost.sendCommand("use 0");
                            runonce = true;

                            agentHelper.UpdateDirection(180, 90);


                            agentHost.sendCommand("use 1");
                            agentHost.sendCommand("use 0");
                        }
                    }
                }
            } catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine("Error reading observations in RunMission");
            }
            foreach (TimestampedReward reward in worldState.rewards) Console.Error.WriteLine("Summed reward: {0}", reward.getValue());
            foreach (TimestampedString error in worldState.errors) Console.Error.WriteLine("Error: {0}", error.text);
        }
        while (worldState.is_mission_running);
        //Console.ReadKey();
        Console.WriteLine("Mission has stopped.");
    }



    
}