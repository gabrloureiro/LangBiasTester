using System.Text.Json;

namespace LangBiasTester.Operators
{
    internal class T1I1
    {
        private int StimulusCount { get; set; }
        private List<int> Sequence { get; }
        private readonly Random Rnd;
        private readonly List<int> Stimuli;
        private readonly string JsonPath;
        private readonly Dictionary<int, List<int>> RawSequences;

        public T1I1(int seed, int stimulusCount)
        {
            JsonPath = "config/RawT1I1Sequences.json";
            Rnd = new(seed);
            StimulusCount = stimulusCount;

            RawSequences = LoadSequences();

            var randomizedStimulus = Enumerable.Range(1, stimulusCount).ToArray();
            Rnd.Shuffle(randomizedStimulus);
            Stimuli = [.. randomizedStimulus];

            Sequence = GetRawSequence(StimulusCount);
        }

        private Dictionary<int, List<int>> LoadSequences()
        {
            var lists = JsonSerializer.Deserialize<Dictionary<int, List<int>>>(File.ReadAllText(JsonPath));

            if (lists == null) ErrorManager.ThrowError("Type-1 Index-1 raw sequence file not found.");

            return lists;
        }

        private List<int> GetRawSequence(int listLength)
        {
            if (RawSequences.TryGetValue(listLength, out List<int>? sequence)) return sequence;
            else
            {
                ErrorManager.ThrowError("No type-1 index-1 sequences are available for the number of stimuli specified.",
                    "Type-1 Index-1 Sequence Not Available");

                return null;
            }
        }

        public List<int>? GenerateSequence(int elementToMaintain = -1)
        {
            RemapSequence(elementToMaintain);
            
            //TODO: add logic to further shuffle the sequence

            if (ValidateSequence(Sequence)) return Sequence;
            else
            {
                ErrorManager.ThrowError("No valid type-1 index-1 sequences could be generated.");
                return null;
            }
        }

        private void RemapSequence(int elementToMaintain = -1)
        {
            // Map elements from original sequence (keys) to new values (values)
            var mappings = new Dictionary<int, int>();

            // If an element is to be maintained, map it to itself
            if (elementToMaintain != -1) mappings.Add(elementToMaintain, elementToMaintain);

            var tempKey = 0;
            var tempValue = 0;

            for (int i = 0; i < StimulusCount; i++)
            {
                // Since sequence is 1-indexed, map element
                var element = i + 1;

                // Do not map element
                if (element == elementToMaintain)
                {
                    tempValue = Stimuli[i];
                }
                else if (Stimuli[i] == elementToMaintain)
                {
                    tempKey = element;
                }
                else mappings[element] = Stimuli[i];
            }
            
            if (tempKey != 0 && tempValue != 0) mappings.Add(tempKey, tempValue);

            var elementPositions = GetPositionsOfElements();

            foreach (var element in elementPositions)
            {
                foreach (int index in element.Value)
                {
                    Sequence[index] = mappings[element.Key];
                }
            }
        }

        private bool ValidateSequence(List<int> tentativeSequence)
        {
            int n = StimulusCount;

            // check sequence is of the right length (O(1))
            if (tentativeSequence.Count != n * n) return false;

            // Build indexed view with position info for efficient filtering
            var indexedSequence = tentativeSequence
                .Select((value, index) => new
                {
                    Value = value,
                    Index = index,
                    Round = index / n,
                    PosInRound = index % n
                })
                .ToList();

            // last items in each round must have successor equal to themselves (O(n))
            var numbersNotFollowedByThemselves = indexedSequence
                .Where(item => item.PosInRound == n - 1 && item.Index < tentativeSequence.Count - 1)
                .Any(item => tentativeSequence[item.Index + 1] != item.Value);

            if (numbersNotFollowedByThemselves) return false;

            // no repeated elements within each round (O(n))
            var roundsWithRepeatedElements = indexedSequence
                .GroupBy(item => item.Round)
                .Where(roundGroup => roundGroup
                    .Select(item => item.Value)
                    .Distinct()
                    .Count() < roundGroup.Count())
                .Select(roundGroup => roundGroup.Key)
                .ToList();

            var repeatedElementsInRound = roundsWithRepeatedElements.Count > 0;

            if (repeatedElementsInRound) return false;

            // each element must appear n times in the sequence (O(n))
            var elementsDoNotAppearNTimes = Enumerable.Range(1, n)
                .Any(value => tentativeSequence.Count(x => x == value) != n);

            if (elementsDoNotAppearNTimes) return false;

            // identical values must have different successors (O(n) to O(n²))
            var repeatedSequences = tentativeSequence
                .Select((value, index) => new { Value = value, Successor = (index + 1) < tentativeSequence.Count ? tentativeSequence[index + 1] : 0 })
                .GroupBy(item => item.Value)
                .Where(group => group.Count() > 1)
                .Any(group => group
                    .Select(item => item.Successor)
                    .Distinct()
                    .Count() < group.Count());

            if (repeatedSequences) return false;

            return true;
        }

        private Dictionary<int, List<int>> GetPositionsOfElements()
        {
            var occurrences = new Dictionary<int, List<int>>();

            for (int i = 0; i < Sequence.Count; i++)
            {
                if (!occurrences.TryGetValue(Sequence[i], out List<int>? value))
                {
                    occurrences[Sequence[i]] = [i];
                }
                else
                {
                    value.Add(i);
                }
            }

            return occurrences;
        }

        new private string ToString()
        {
            var sequenceString = "";

            for (int i = 0; i < Sequence.Count / StimulusCount; i++)
            {
                // take stimulusCount elements for each round
                var roundElements = Sequence.GetRange(i * StimulusCount, StimulusCount);
                sequenceString += string.Join(", ", roundElements) + "\n";
            }

            return sequenceString;
        }
    }
}