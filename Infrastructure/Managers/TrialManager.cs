using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class TrialManager
    {
        internal static void Init(LinkedList<Trial> trials, List<Stimulus> stimuli, int trialBlocks, int sequencesPerBlock, int seed)
        {
            var t1i1 = new T1I1(seed, stimuli.Count);

            // Generate a new sequence
            var trialSequence = t1i1.GenerateSequence();

            // Create a dummy trial to preserve the T1I1 property
            var firstTrial = trialSequence[0];
            // T1I1 sequences are 1:n, so convert for 0-indexing
            var firstTrialIndex = firstTrial - 1;
            InitTrial(trials, StimulusManager.Clone(stimuli[firstTrialIndex]), firstTrial, -1);

            // Iterate through each block
            for (int blockNumber = 0; blockNumber < trialBlocks; blockNumber++)
            {
                // Iterate through each sequence to be added to the block.
                // Index by 1 for easier readability when debugging
                for (int sequenceNumber = 1; sequenceNumber <= sequencesPerBlock; sequenceNumber++)
                {
                    // Iterate through sequence
                    for (int i = 0; i < trialSequence.Count; i++)
                    {
                        var stimulus = trialSequence[i];
                        // T1I1 sequences are 1:n, so convert for 0-indexing
                        var stimulusIndex = stimulus - 1;
                        InitTrial(trials, StimulusManager.Clone(stimuli[stimulusIndex]), stimulus, blockNumber);
                    }

                    // For all iterations before last, generate a new sequence, preserving the first/last trial of the previous sequence
                    if (sequenceNumber < sequencesPerBlock) trialSequence = t1i1.GenerateSequence(firstTrial);
                }
            }
        }
        
        private static void InitTrial(LinkedList<Trial> trials, Stimulus stimulus, int stimulusIndex, int blockNumber)
        {
            var newTrial = new Trial(stimulus)
            {
                Block = blockNumber,
                StimulusNumber = stimulusIndex
            };

            trials.AddLast(newTrial);
        }

        internal static void RegisterResponse(Trial trial, Keys response, float responseConfidence=0)
        {
            trial.Response = response;
            if (responseConfidence > 0) trial.ResponseConfidence = responseConfidence;
        }
        
        internal static void RunTrial(Trial trial)
        {
            StimulusManager.RunStimulus(trial.Stimulus);
        }

        internal static void EndTrial(Trial trial)
        {
            StimulusManager.StopStimulus(trial.Stimulus);
            StimulusManager.DisposeOfStimulus(trial.Stimulus);
        }
    }
}
