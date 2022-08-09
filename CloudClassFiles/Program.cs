using MyCloudProject.Common;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading;
using MyExperiment;

namespace MyCloudProject
{
    class Program
    {
        /// <summary>
        /// Your project ID from the last semester.
        /// </summary>
        private static string projectName = "ML20/21-5.2";

        static void Main(string[] args)
        {
            CancellationTokenSource tokeSrc = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                tokeSrc.Cancel();
            };

            while (true) {
                Console.WriteLine($"Started experiment: {projectName}");

                //init configuration
                var cfgRoot = Common.InitHelpers.InitConfiguration(args);

                var cfgSec = cfgRoot.GetSection("Values");

                // InitLogging
                var logFactory = InitHelpers.InitLogging(cfgRoot);
                var logger = logFactory.CreateLogger("Train.Console");

                logger?.LogInformation($"{DateTime.Now} -  Started experiment: {projectName}");

                AzureBlobStorageProvider storageProvider = new AzureBlobStorageProvider(cfgSec);
                AzureTableStorageProvider tableStorage = new AzureTableStorageProvider(cfgSec);

                Experiment experiment = new Experiment(cfgSec, storageProvider, tableStorage ,projectName, logger/* put some additional config here */);

                try
                {
                    experiment.RunQueueListener(tokeSrc.Token).Wait();
                }
                catch(Exception e)
                {
                    logger?.LogInformation($"Error"+ e.ToString());
                    Console.WriteLine($"Error" + e.ToString());
                }

                logger?.LogInformation($"{DateTime.Now} -  Experiment exit: {projectName}");
                
                }
        }


    }
}
