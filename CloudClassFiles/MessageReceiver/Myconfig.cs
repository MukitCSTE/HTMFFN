using System;
using System.Collections.Generic;
using System.Text;

namespace MessageReceiver
{
    class Myconfig
    {

        public string AzureWebJobsStorage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FUNCTIONS_WORKER_RUNTIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static string StorageConnectionString { get; set; }
    }
}
