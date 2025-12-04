using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class TrialManager
    {
        internal static void Init(LinkedList<Trial> trials, List<Stimulus> stimuli, List<Audio> mixedAudios, int audioDuration, int trialBlocks, int sequencesPerBlock, int seed, bool useT1I1 = true)
        {
            var t1i1 = new T1I1(seed, stimuli.Count);

            for (int blockNumber = 0; blockNumber < trialBlocks; blockNumber++)
            {
                for (int sequenceNumber = 0; sequenceNumber < sequencesPerBlock; sequenceNumber++)
                {
                    // Generate a new sequence
                    var trialSequence = t1i1.GenerateSequence();

                    // Create a dummy trial to preserve the T1I1 property
                    var firstTrialIndex = trialSequence[0] - 1;
                    InitTrial(trials, trialSequence, stimuli[firstTrialIndex], mixedAudios[firstTrialIndex], audioDuration, firstTrialIndex, blockNumber);

                    for (int i = 0; i < trialSequence.Count; i++)
                    {
                        // T1I1 sequences are 1:n, so convert for 0-indexing
                        var stimulusIndex = trialSequence[i] - 1;
                        InitTrial(trials, trialSequence, stimuli[stimulusIndex], mixedAudios[stimulusIndex], audioDuration, stimulusIndex, blockNumber);
                    }
                }
            }
        }

        private static void InitTrial(LinkedList<Trial> trials, List<int> trialSequence, Stimulus stimulus, Audio audio, int audioDuration, int stimulusIndex, int blockNumber)
        {
            var newTrial = new Trial(stimulus)
            {
                Block = blockNumber,
                StimulusNumber = stimulusIndex + 1
            };

            newTrial.Audios.Add(audio);
            newTrial.Audios[0].SnippetDuration = audioDuration;

            trials.AddLast(newTrial);
        }
    }
}
