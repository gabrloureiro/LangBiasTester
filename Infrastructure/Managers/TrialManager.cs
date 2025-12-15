using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class TrialManager
    {
        internal static void Init(LinkedList<Trial> trials, List<Stimulus> stimuli, int trialBlocks, int sequencesPerBlock, int seed, bool useT1I1 = true)
        {
            var t1i1 = new T1I1(seed, stimuli.Count);

            // Iterate through each block
            for (int blockNumber = 0; blockNumber < trialBlocks; blockNumber++)
            {
                // Iterate through each sequence to be added to the block
                for (int sequenceNumber = 0; sequenceNumber < sequencesPerBlock; sequenceNumber++)
                {
                    // Generate a new sequence
                    var trialSequence = t1i1.GenerateSequence();

                    // Create a dummy trial to preserve the T1I1 property
                    var firstTrialIndex = trialSequence[0] - 1;
                    InitTrial(trials, StimulusManager.Clone(stimuli[firstTrialIndex]), firstTrialIndex, blockNumber);

                    // Iterate through sequence
                    for (int i = 0; i < trialSequence.Count; i++)
                    {
                        // T1I1 sequences are 1:n, so convert for 0-indexing
                        var stimulusIndex = trialSequence[i] - 1;
                        InitTrial(trials, StimulusManager.Clone(stimuli[stimulusIndex]), stimulusIndex, blockNumber);
                    }
                }
            }
        }
        
        private static void InitTrial(LinkedList<Trial> trials, Stimulus stimulus, int stimulusIndex, int blockNumber)
        {
            var newTrial = new Trial(stimulus)
            {
                Block = blockNumber,
                StimulusNumber = stimulusIndex + 1
            };

            trials.AddLast(newTrial);
        }

        internal static void RegisterResponse(Trial trial, Keys response)
        {
            trial.Response = response;

            //_currentTrial.Value.ResponseLang = _experiment.Settings.LangKeyDictionary.Where(entry => entry.Value == response).First().Key;
        }
        
        internal static void RunTrial(Trial trial)
        {
            StimulusManager.RunStimulus(trial.Stimulus);
        }

        internal static void EndTrial(Trial trial)
        {
            StimulusManager.StopStimulus(trial.Stimulus);
        }
    }
}
