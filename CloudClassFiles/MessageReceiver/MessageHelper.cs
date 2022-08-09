using System;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace MessageReceiver
{
    public  class MessageHelper
    {

     

        //-------------------------------------------------
        // Create a message queue
        //-------------------------------------------------
        public static bool CreateQueue()
        {

            
            string queueName = "messages-queue";

            var connParameter = "AzureWebJobsStorage";
          
            var connectionString = System.Environment.GetEnvironmentVariable($"ConnectionStrings:{connParameter}");
            if (string.IsNullOrEmpty(connectionString))
            {
                Myconfig.StorageConnectionString = connectionString;
            }



            try
            {

                // Instantiate a QueueClient which will be used to create and manipulate the queue
                QueueClient queueClient = new QueueClient(Myconfig.StorageConnectionString, queueName);

                // Create the queue
                queueClient.CreateIfNotExists();

                if (queueClient.Exists())
                {
                    Console.WriteLine($"Queue created: '{queueClient.Name}'");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\n\n");
                Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
                return false;
            }
        }


        //-------------------------------------------------
        // Insert a message into a queue
        //-------------------------------------------------
        public static void InsertMessage(string message)
        {

         

          
                Myconfig.StorageConnectionString = System.Environment.GetEnvironmentVariable($"AzureWebJobsStorage");
            

            // Get the connection string from app settings
            //string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            // Instantiate a QueueClient which will be used to create and manipulate the queue
            string queueName = "messages-queue";
            QueueClient queueClient = new QueueClient(Myconfig.StorageConnectionString, queueName);

            // Create the queue if it doesn't already exist
            queueClient.CreateIfNotExists();

            if (queueClient.Exists())
            {
                // Send a message to the queue
                queueClient.SendMessage(message);
            }

            //Console.WriteLine($"Inserted: {message}");
        }

    }
}
