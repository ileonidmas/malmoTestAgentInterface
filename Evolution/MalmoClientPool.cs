using Microsoft.Research.Malmo;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RunMission.Evolution
{
    class MalmoClientPool
    {
        private static SemaphoreSlim semaphore;
        private ClientPool availableClients;

        public MalmoClientPool(int poolSize)
        {
            semaphore = new SemaphoreSlim(poolSize);
            availableClients = new ClientPool();
            InitializeClients(poolSize);
        }

        private void InitializeClients(int poolSize)
        {
            for(int offset = 0; offset < poolSize; offset++)
            {
                availableClients.add(new ClientInfo("127.0.0.1", 10000 + offset));
            }
        }

        public void RunAvailableClient(IBlackBox brain)
        {
            semaphore.Wait();
            try
            {
                MalmoClient client = new MalmoClient(availableClients);
                client.RunMalmo(brain);
            } catch(Exception ex)
            {
                Console.WriteLine("RACE CONDITION!!!!!!!!!!!");
            }
            semaphore.Release();
        }
    }
}
