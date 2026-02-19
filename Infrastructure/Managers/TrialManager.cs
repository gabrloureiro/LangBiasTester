using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class TrialManager
    {
        internal static void Init(LinkedList<Trial> trials, List<Stimulus> stimuli, int stimulusCount, int totalBlocks, int sequencesPerBlock, int seed)
        {

            var t1i1 = new T1I1(seed, stimulusCount);

            // Generate a new sequence
            var t1i1sequence = t1i1.GenerateSequence();
            var trialsPerSequence = t1i1sequence.Count;
            
            // Iterate through blocks
            // b = number of blocks procesed so far
            for (int b = 0; b < totalBlocks; b++)
            {
                // First trial in each block is just (number of processed blocks) * (sequences each of those blocks had) * (trials each sequence had)
                var firstTrialInBlock = b * sequencesPerBlock * trialsPerSequence;

                // Iterate through sequences to be added to the block
                // s = processed sequence count
                for (int s = 0; s < sequencesPerBlock; s++)
                {
                    // First trial in sequence is just (first trial in its block) + ((number of processed sequences) * (trials each sequence had))
                    var firstTrialInSequence = firstTrialInBlock + s * trialsPerSequence;

                    // Create a dummy trial to preserve the T1I1 property
                    var firstStimulusNumber = t1i1sequence[0];
                    var firstStimulusIndex = firstStimulusNumber - 1; // T1I1 sequences are 1:n, so convert to 0-indexing
                    InitTrial(trials, StimulusManager.Clone(stimuli[firstStimulusIndex]), firstStimulusNumber, s, b);

                    // Iterate through sequence
                    // t = current trial in the current sequence
                    for (int t = 0; t < trialsPerSequence; t++)
                    {
                        // Get the round number by dividing the current trial in the sequence by the stimulus count and rounding down (integer division)
                        var roundNumber = t / stimulusCount;
                        // Get the index of the first trial in that round
                        var firstTrialInRound = firstTrialInSequence + roundNumber * stimulusCount;

                        // Trim stimuli list to those in the current round
                        var roundStimuli = stimuli.GetRange(firstTrialInRound, stimulusCount);

                        var stimulusNumber = t1i1sequence[t];
                        var stimulusIndex = stimulusNumber - 1; // T1I1 sequences are 1:n, so convert to 0-indexing

                        InitTrial(trials, roundStimuli[stimulusIndex], stimulusNumber, s, b);
                    }

                    // For all iterations before last, generate a new sequence, preserving the first/last trial of the previous sequence
                    if (s < sequencesPerBlock - 1) t1i1sequence = t1i1.GenerateSequence(firstTrialInSequence);
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
