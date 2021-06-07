
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ML2021_5._2_HTM_FeedForward_network
{

    /// <summary>
    /// In the brain the Layer 4 has feed forforward connection with Layer 2 in CortexLayer.
    /// So, instead of using layer name L1 we give it as L4
    /// </summary>

    public class FeedForwardNetExperimentLayer4
    {
        CortexLayer<object, object> layerL4, layerL2;
        Dictionary<double, int[]> L4_ActiveCell_sdr_log = new Dictionary<double, int[]>();
        Dictionary<double, int[]> L2_ActiveCell_sdr_log = new Dictionary<double, int[]>();

        TemporalMemory tm4, tm2;
        bool isSimilar_L4_active_cell_sdr = false;
        string key;



        public void FeedForwardNetTest()
        {
            int cellsPerColumnL4 = 20;
            int numColumnsL4 = 2048;
            int cellsPerColumnL2 = 20;
            int numColumnsL2 = 500;

            int inputBits = 100;
            double minOctOverlapCycles = 1.0;
            double maxBoost = 10.0;
            double max = 20;

            HtmConfig htmConfig_L4 = new HtmConfig(new int[] { inputBits }, new int[] { numColumnsL4 })
            {
                Random = new ThreadSafeRandom(42),
                CellsPerColumn = cellsPerColumnL4,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumnsL4,
                PotentialRadius = inputBits,// Ever column is connected to 50 of 100 input cells.
                InhibitionRadius = 15,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                MaxSynapsesPerSegment = (int)(0.02 * numColumnsL4),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.15,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };

            // The HTM of the L2 is connected to cells of the HTM of L4.
            int inputsL2 = numColumnsL4 * cellsPerColumnL4;

            HtmConfig htmConfig_L2 = new HtmConfig(new int[] { inputsL2 }, new int[] { numColumnsL2 })
            {
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = cellsPerColumnL2,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.2 * numColumnsL2,
                PotentialRadius = inputsL2, // Every columns 
                InhibitionRadius = 15,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                MaxSynapsesPerSegment = (int)(0.05 * numColumnsL2),
                ActivationThreshold = 10,
                ConnectedPermanence = 0.10,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };

            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            EncoderBase encoder = new ScalarEncoder(settings);
            //List<double> inputValues = new List<double>(new double[] { 12, 12, 17, 17, 12 });
            List<double> inputValues = new List<double>(new double[] { 7, 8, 9, 10, 11, 8, 9, 12 });
            //List<double> inputValues = new List<double>(new double[] { 7, 8, 9 });

            RunExperiment(inputBits, htmConfig_L4, encoder, inputValues, htmConfig_L2);
        }

        private void RunExperiment(int inputBits, HtmConfig cfgL4, EncoderBase encoder, List<double> inputValues, HtmConfig cfgL2)
        {
            Stopwatch swL2 = new Stopwatch();

            int maxMatchCnt = 0;
            bool learn = true;
            bool isSP4Stable = false;
            bool isSP2STable = false;

            var memL4 = new Connections(cfgL4);
            var memL2 = new Connections(cfgL2);

            var numInputs = inputValues.Distinct<double>().ToList().Count;
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            layerL4 = new CortexLayer<object, object>("L4");
            layerL2 = new CortexLayer<object, object>("L2");
            //tm4 = new TemporalMemoryMT();
            //tm2 = new TemporalMemoryMT();
            tm4 = new TemporalMemory();
            tm2 = new TemporalMemory();

            // HPC for Layer 4 SP

            HomeostaticPlasticityController hpa_sp_L4 = new HomeostaticPlasticityController(memL4, numInputs * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    Debug.WriteLine($"SP L4 STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    Debug.WriteLine($"SP L4 INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                learn = isSP4Stable = isStable;
                cls.ClearState();

            }, numOfCyclesToWaitOnChange: 50);


            // HPC for Layer 2 SP

            HomeostaticPlasticityController hpa_sp_L2 = new HomeostaticPlasticityController(memL2, numInputs * 50, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    Debug.WriteLine($"SP L2 STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    Debug.WriteLine($"SP L2 INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                learn = isSP2STable = isStable;
                cls.ClearState();
            }, numOfCyclesToWaitOnChange: 50);

            SpatialPooler sp4 = new SpatialPooler(hpa_sp_L4);

            SpatialPooler sp2 = new SpatialPooler(hpa_sp_L2);

            sp4.Init(memL4);
            sp2.Init(memL2);

            // memL2.TraceInputPotential();

            tm4.Init(memL4);
            tm2.Init(memL2);

            layerL4.HtmModules.Add("encoder", encoder);
            layerL4.HtmModules.Add("sp", sp4);
            layerL4.HtmModules.Add("tm", tm4);

            layerL2.HtmModules.Add("sp", sp2);
            layerL2.HtmModules.Add("tm", tm2);

            int[] inpCellsL4ToL2 = new int[cfgL4.CellsPerColumn * cfgL4.NumColumns];

            double[] inputs = inputValues.ToArray();
            int[] prevActiveCols = new int[0];
            int cycle = 0;
            int matches = 0;
            string lastPredictedValue = "0";
            int maxCycles = 3500;
            int maxPrevInputs = inputValues.Count - 1;
            List<string> previousInputs = new List<string>();
            

            //
            // Training SP at Layer 4 to get stable. New-born stage.
            //

            using (StreamWriter swL4Sdrs = new StreamWriter($"L4-SDRs-in_{cfgL2.NumInputs}-col_{cfgL2.NumColumns}-r_{cfgL2.PotentialRadius}.txt"))
            {
                using (StreamWriter sw = new StreamWriter($"in_{cfgL2.NumInputs}-col_{cfgL2.NumColumns}-r_{cfgL2.PotentialRadius}.txt"))
                {
                    for (int i = 0; i < maxCycles; i++)
                    {
                        matches = 0;
                        cycle = i;
                        Debug.WriteLine($"-------------- Newborn Cycle {cycle} at L4 SP region  ---------------");

                        foreach (var input in inputs)
                        {
                            Debug.WriteLine($" INPUT: '{input}'\t Cycle:{cycle}");
                            Debug.Write("L4: ");
                            var lyrOut = layerL4.Compute(input, learn);
                            var activeColumns = layerL4.GetResult("sp") as int[];
                            int[] cellSdrL4Indexes = memL4.ActiveCells.Select(c => c.Index).ToArray();
                            Debug.WriteLine($"L4out Active Coloumn for input: {input}: {Helpers.StringifyVector(activeColumns)}");
                            Debug.WriteLine($"L4out SDR for input: {input}: {Helpers.StringifyVector(cellSdrL4Indexes)}");

                        }


                        if (isSP4Stable)
                            break;


                    }
                }
            }



            // TM Stability Check at L4

            //
            // Now training with SP+TM. SP is pretrained on the given input pattern set.
            for (int i = 0; i < maxCycles; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Cycle {cycle} ---------------");

                foreach (var input in inputs)
                {
                    Debug.WriteLine($"-------------- {input} ---------------");

                    var lyrOut = layerL4.Compute(input, learn) as ComputeCycle;

                    // lyrOut is null when the TM is added to the layer inside of HPC callback by entering of the stable state.
                    //if (isInStableState && lyrOut != null)
                    {
                        var activeColumns = layerL4.GetResult("sp") as int[];

                        //layer2.Compute(lyrOut.WinnerCells, true);
                        //activeColumnsLst[input].Add(activeColumns.ToList());

                        previousInputs.Add(input.ToString());
                        if (previousInputs.Count > (maxPrevInputs + 1))
                            previousInputs.RemoveAt(0);

                        // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                        // In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
                        // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                        // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                        // memorized, it will match as the first one.
                        if (previousInputs.Count < maxPrevInputs)
                            continue;

                        string key = GetKey(previousInputs, input);

                        List<Cell> actCells;

                        if (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count)
                        {
                            actCells = lyrOut.ActiveCells;
                        }
                        else
                        {
                            actCells = lyrOut.WinnerCells;
                        }

                        cls.Learn(key, actCells.ToArray());

                        if (learn == false)
                            Debug.WriteLine($"Inference mode");

                        Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                        Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                        if (key == lastPredictedValue)
                        {
                            matches++;
                            Debug.WriteLine($"Match. Actual value: {key} - Predicted value: {lastPredictedValue}");
                        }
                        else
                            Debug.WriteLine($"Missmatch! Actual value: {key} - Predicted value: {lastPredictedValue}");

                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            //var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
                            var predictedInputValues = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            foreach (var item in predictedInputValues)
                            {
                                Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {item}");
                            }

                            lastPredictedValue = predictedInputValues.First().PredictedInput;
                        }
                        else
                        {
                            Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                            lastPredictedValue = String.Empty;
                        }
                    }
                }

                // The brain does not do that this way, so we don't use it.
                // tm1.reset(mem);

                double accuracy = (double)matches / (double)inputs.Length * 100.0;

                Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {accuracy}%");

                if (accuracy == 100.0)
                {
                    maxMatchCnt++;
                    Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");
                    //
                    // Experiment is completed if we are 30 cycles long at the 100% accuracy.
                    if (maxMatchCnt >= 30)
                    {
                       
                        Debug.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy");
                        learn = false;
                        break;
                    }
                }
                else if (maxMatchCnt > 0)
                {
                    Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with {accuracy}. This indicates instable state. Learning will be continued.");
                    maxMatchCnt = 0;
                }
            }

            Debug.WriteLine("------------ END ------------");
        }





           

        private static string GetKey(List<string> prevInputs, double input)
        {
            string key = String.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += (prevInputs[i]);
            }

            return key;
        }
    }
}
