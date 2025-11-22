using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Operators
{
    internal class T1I1
    {
        private int Seed { get; set; }
        private int ConditionCount { get; set; }
        private List<int> Sequence { get; }

        internal T1I1(int seed, int conditionCount)
        {
            Seed = seed;
            ConditionCount = conditionCount;
            Sequence = GenerateStarterSequence();
        }

        private List<int> GenerateStarterSequence()
        {
            var random = new Random(Seed);

            var sequenceSize = ConditionCount * ConditionCount;

            var starterSequence = Enumerable.Range(1, sequenceSize)
                .Select(_ => random.Next(1, ConditionCount+1))
                .ToList();

            return starterSequence;
        }

        public List<int> GenerateSequence()
        {
            if (ValidateSequence(Sequence))
            {
                TweakSequence();
                return GenerateSequence();
            }

            return Sequence;
        }

        private void TweakSequence()
        {
            // tweak sequence according to some logic which brings it closer to validity
        }

        private bool ValidateSequence(List<int> tentativeSequence)
        {
            int n = ConditionCount;

            // check sequence is of the right length (O(1))
            if (tentativeSequence.Count != n) return false;

            // last item in the sequence must be the same as the first (O(1))
            if (tentativeSequence[^1] != tentativeSequence[0]) return false;

            // Build indexed view with position info for efficient filtering
            var indexedSequence = tentativeSequence
                .Select((value, index) => new
                {
                    Value = value,
                    Index = index,
                    Round = (index + 1) / n,
                    PosInRound = (index + 1) % n
                })
                .ToList();

            // last items in each round must have successor equal to themselves (O(n))
            var numbersNotFollowedByThemselves = indexedSequence
                .Where(item => item.PosInRound == n && item.Index < tentativeSequence.Count - 1)
                .Any(item => tentativeSequence[item.Index + 1] != item.Value);

            if (numbersNotFollowedByThemselves) return false;

            // no repeated elements within each round (O(n))
            var repeatedElementsInRound = indexedSequence
                .GroupBy(item => item.Round)
                .Any(roundGroup => roundGroup
                    .Select(item => item.Value)
                    .Distinct()
                    .Count() < roundGroup.Count());

            if (repeatedElementsInRound) return false;

            // no value should appear at position 1 more than once (O(n))
            var repeatedElementsInFirstPosition = indexedSequence
                .Where(item => item.PosInRound == 1)
                .GroupBy(item => item.Value)
                .Any(group => group.Count() > 1);

            if (repeatedElementsInFirstPosition) return false;

            // no value should appear at position n more than once (O(n))
            var repeatedElementsInLastPosition = indexedSequence
                .Where(item => item.PosInRound == n)
                .GroupBy(item => item.Value)
                .Any(group => group.Count() > 1);

            if (repeatedElementsInLastPosition) return false;

            // identical values must have different successors (O(n) to O(n²))
            var repeatedSequences = tentativeSequence
                .Where((value, index) => index < tentativeSequence.Count - 1)
                .Select((value, index) => new { Value = value, Successor = tentativeSequence[index + 1] })
                .GroupBy(item => item.Value)
                .Where(group => group.Count() > 1)
                .Any(group => group
                    .Select(item => item.Successor)
                    .Distinct()
                    .Count() < group.Count());

            if (repeatedSequences) return false;

            return true;
        }
    }
}