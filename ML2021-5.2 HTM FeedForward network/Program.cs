using System;

namespace ML2021_5._2_HTM_FeedForward_network
{
    class Program
    {
        static void Main(string[] args)
        {
           //Experiment start Without L2 SP Pretrain
           FeedForwardNetExperiment experiment = new FeedForwardNetExperiment();
           experiment.FeedForwardNetTest();
        }
    }
}
