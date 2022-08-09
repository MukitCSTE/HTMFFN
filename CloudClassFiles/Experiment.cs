using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyExperiment
{
    /// <summary>
    /// This class implements the whole long-running experiment.
    /// </summary>
    public class Experiment : IExperiment
    {
        
        private AzureBlobStorageProvider storageProvider;
        private AzureTableStorageProvider tableStorageProvider;

        private ILogger logger;

        private MyConfig config;

        private L4Config layer4config;
        private L2Config layer2config;
        private CommonConfig common_config;
        private Sequence sequence;



        private string expectedProjectName; 

        /// <summary>
        /// TODO....
        /// </summary>
        /// <param name="configSection"></param>
        /// <param name="storageProvider"></param>
        /// <param name="expectedPrjName"></param>
        /// <param name="log"></param>
        public Experiment(IConfigurationSection configSection, AzureBlobStorageProvider storageProvider, AzureTableStorageProvider tableStorage, string expectedPrjName, ILogger log)
        {
            this.storageProvider = storageProvider;
            this.tableStorageProvider = tableStorage;
            this.logger = log;
            this.expectedProjectName = expectedPrjName;
            config = new MyConfig();
            configSection.Bind(config);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFile">The inpout msg  sequence.</param>
        /// <returns></returns>
        public Task<ExperimentResult> Run(string inputMsgSequence)
        {
            // TODO read file from Azure Storage by using this.storageProvider.DownloadInputFile

         
            DateTime ExpStart = DateTime.UtcNow;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            HtmFeedForwardNetExperiment experiment = new HtmFeedForwardNetExperiment();
            try
            {
                experiment.FeedForwardNetTest(this.layer4config, this.layer2config, this.common_config, this.sequence, this.config, inputMsgSequence);
                stopWatch.Stop();
            }
            catch { }

            ExperimentResult res = new ExperimentResult
            {
                PartionKey = RuntimeExpTempResultHolder.message_id ?? "MSG00",
                RowKey =  "SEQ0"+RuntimeExpTempResultHolder.sequence_id+"-"+Guid.NewGuid().ToString() ?? "MSG_XX",
                GroupName = this.config.GroupId,
                ExperimentId = common_config.expId,
                ExpDoneBy = common_config.expDoneby,
                StartTimeUtc = ExpStart,
                EndTimeUtc = DateTime.UtcNow,
                Status = RuntimeExpTempResultHolder.experiment_staus,
                Error = RuntimeExpTempResultHolder.error_msg,
                Accuracy = RuntimeExpTempResultHolder.accuracy,
                DurationSec = 12,
                OutputFileName = RuntimeExpTempResultHolder.blob_file_name,
                Message_id = RuntimeExpTempResultHolder.message_id,
                Sequence_id = RuntimeExpTempResultHolder.sequence_id,
                Exp_result_log = RuntimeExpTempResultHolder.resultoutcome_msg,
                Match_repeat = RuntimeExpTempResultHolder.match_repeat,
                Number_of_cycles_has_needed = RuntimeExpTempResultHolder.number_of_cycles_needed,
                Exp_log_blob_file = RuntimeExpTempResultHolder.blob_file_name


            };


            return Task.FromResult< ExperimentResult>(res); 
        }

        /// <inheritdoc/>
        public async Task RunQueueListener(CancellationToken cancelToken)
        {

            QueueClient queueClient = await CreateQueueAsync(config);
            string connectionString = config.StorageConnectionString;
            string containerName = config.InputContainer;
            

            while (cancelToken.IsCancellationRequested == false)
            {


                
                QueueMessage[] retrievedMessages = await queueClient.ReceiveMessagesAsync();

                if (retrievedMessages.Length > 0)
                {

                   

                    ExerimentRequestMessage ob = new ExerimentRequestMessage();

                    if (retrievedMessages != null)
                    {
                        foreach (QueueMessage message in retrievedMessages)
                        {

                            Console.WriteLine($"Received Queue messages with content '{message.Body}'");
                            

                            Chilkat.JsonObject json = new Chilkat.JsonObject();
                            json.Load(message.Body.ToString());
                            Chilkat.JsonArray messages = json.ArrayOf("messages");

                            int numMessages = messages.Size;
                            int j = 0;
                            while (j < numMessages)
                            {
                                try
                                {
                                 
                                    Chilkat.JsonObject empobj = messages.ObjectAt(j);
                                    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                                    BlobClient blobClient = containerClient.GetBlobClient(empobj.StringOf("L4_config_blob"));

                                    using (var stream = await blobClient.OpenReadAsync())
                                    using (var sr = new StreamReader(stream))
                                    using (var jr = new JsonTextReader(sr))
                                    {
                                        layer4config = JsonSerializer.CreateDefault().Deserialize<L4Config>(jr);
                                    }

                                    blobClient = containerClient.GetBlobClient(empobj.StringOf("L2_config_blob"));

                                    using (var stream = await blobClient.OpenReadAsync())
                                    using (var sr = new StreamReader(stream))
                                    using (var jr = new JsonTextReader(sr))
                                    {
                                        layer2config = JsonSerializer.CreateDefault().Deserialize<L2Config>(jr);
                                    }

                                    blobClient = containerClient.GetBlobClient(empobj.StringOf("Common_config_blob"));

                                    using (var stream = await blobClient.OpenReadAsync())
                                    using (var sr = new StreamReader(stream))
                                    using (var jr = new JsonTextReader(sr))
                                    {
                                        common_config = JsonSerializer.CreateDefault().Deserialize<CommonConfig>(jr);
                                    }

                                    blobClient = containerClient.GetBlobClient(empobj.StringOf("Sequence_blob"));

                                    using (var stream = await blobClient.OpenReadAsync())
                                    using (var sr = new StreamReader(stream))
                                    using (var jr = new JsonTextReader(sr))
                                    {
                                        sequence = JsonSerializer.CreateDefault().Deserialize<Sequence>(jr);
                                    }
                                }
                                catch
                                {
                                    //
                                }
                                Console.WriteLine("experiment on message: " + (j + 1)+" is going to start now");
                                ExperimentResult result = await Run((j+1).ToString());
                                Console.WriteLine("experiment on message: " + (j + 1) + "has end");

                                tableStorageProvider.uploaddata(result);

                                j = j + 1;
                            }

                            queueClient.DeleteMessage(message.MessageId, message.PopReceipt);
                        }

                    }
                   
                }
                else
                {
                    // TODO. some log entry that says, that there are no tmessages in the queue.
                    Console.WriteLine($"Waiting for receving new Messages");
                    await Task.Delay(1000);
                }
            }

            this.logger?.LogInformation("Cancel pressed. Exiting the listener loop.");
        }




    


        /// <summary>
        /// Validate the connection string information in app.config and throws an exception if it looks like 
        /// the user hasn't updated this to valid values. 
        /// </summary>
        /// <param name="storageConnectionString">The storage connection string</param>
        /// <returns>CloudStorageAccount object</returns>
        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        /// <summary>
        /// Create a queue for the sample application to process messages in. 
        /// </summary>
        /// <returns>A CloudQueue object</returns>
        public static async Task<QueueClient> CreateQueueAsync(MyConfig config)
        {
            

            // Get the connection string from app settings
            string connectionString = config.StorageConnectionString;

            // Instantiate a QueueClient which will be used to manipulate the queue
            QueueClient queueClient = new QueueClient(connectionString, config.Queue);


            // Create the queue if it doesn't already exist
            await queueClient.CreateIfNotExistsAsync();


            if (await queueClient.ExistsAsync())
            {
                Console.WriteLine($"Queue '{queueClient.Name}' created");
            }
            else
            {
                Console.WriteLine($"Queue '{queueClient.Name}' exists");
            }

            return queueClient;
        }
      
    }
}
