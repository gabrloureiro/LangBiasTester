using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class TrialManager
    {
        internal static void Init(LinkedList<Trial> trials, List<Stimulus> stimuli, int stimulusCount, int trialBlocks, int sequencesPerBlock, int seed)
        {

            var t1i1 = new T1I1(seed, stimulusCount);

            // Generate a new sequence
            var trialSequence = t1i1.GenerateSequence();
            var trialsPerSequence = trialSequence.Count;
            
            // Iterate through each block
            for (int blockNumber = 0; blockNumber < trialBlocks; blockNumber++)
            {
                var firstTrialInBlock = blockNumber * sequencesPerBlock * trialsPerSequence;

                // Iterate through each sequence to be added to the block.
                for (int sequenceNumber = 0; sequenceNumber < sequencesPerBlock; sequenceNumber++)
                {
                    var firstTrialInSequence = firstTrialInBlock + sequenceNumber * trialsPerSequence;

                    // Create a dummy trial to preserve the T1I1 property
                    var firstStimulus = stimuli[firstTrialInSequence];
                    var firstStimulusNumber = trialSequence[0];
                    InitTrial(trials, StimulusManager.Clone(firstStimulus), firstStimulusNumber, sequenceNumber, blockNumber);

                    // Iterate through sequence
                    for (int i = 0; i < trialsPerSequence; i++)
                    {
                        // Get the round number and the index of the first trial in that round
                        var roundNumber = i / stimulusCount;
                        var firstTrialInRound = firstTrialInSequence + roundNumber * stimulusCount;

                        // Trim stimuli list to those in the current round
                        var roundStimuli = stimuli.GetRange(firstTrialInRound, stimulusCount);

                        var stimulusNumber = trialSequence[i];
                        // T1I1 sequences are 1:n, so convert for 0-indexing
                        var stimulusIndex = stimulusNumber - 1;

                        InitTrial(trials, roundStimuli[stimulusIndex], stimulusNumber, sequenceNumber, blockNumber);
                    }

                    // For all iterations before last, generate a new sequence, preserving the first/last trial of the previous sequence
                    if (sequenceNumber < sequencesPerBlock - 1) trialSequence = t1i1.GenerateSequence(firstTrialInSequence);
                }
            }
        }
        
        private static void InitTrial(LinkedList<Trial> trials, Stimulus stimulus, int stimulusNumber, int sequenceNumber, int blockNumber)
        {
            var newTrial = new Trial(stimulus)
            {
                Block = blockNumber,
                Sequence = sequenceNumber,
                StimulusNumber = stimulusNumber
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
