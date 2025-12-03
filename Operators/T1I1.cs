using NAudio.Midi;
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
        private List<int>? Sequence { get; }
        private Stack<int> SequenceStack;
        private Dictionary<int, Stack<int>> SuccessorsStack;
        private readonly List<int>? StarterRound;
        private readonly Dictionary<int, List<int>>? Successors;
        private readonly Random Rnd;
        private readonly HashSet<int> Conditions;

        public T1I1(int seed, int conditionCount)
        {
            Seed = seed;
            Rnd = new Random(seed);
            ConditionCount = conditionCount;

            SequenceStack = new Stack<int>();
            SuccessorsStack = BuildSuccessorStack();

            var randomizedConditions = Enumerable.Range(1, conditionCount);
            Rnd.Shuffle(randomizedConditions.ToArray());
            Conditions = [.. randomizedConditions];
        }

        private Dictionary<int, Stack<int>> BuildSuccessorStack()
        {
            var successorsStack = new Dictionary<int, Stack<int>>();

            for (int i = 1; i <= ConditionCount; i++)
            {
                successorsStack[i] = new Stack<int>();
            }

            return successorsStack;
        }

        // Searches possibilities for a valid sequence using DFS tree search, returns whether that branch was successful. Continues exploring until a valid sequence is found or all possibilities exhausted.
        // Impractical for more than 8 conditions due to massive branching factor (n!, complexity somewhere in the realm of O(n!^n)).
        private bool DFSTreeSearch(int startingNode, int depth, HashSet<int> numbersUsedThisRound)
        {
            // If the depth of the search is larger than the expected size of the sequence, last run found the complete sequence. Return true to propagate success upwards.
            if (SequenceStack.Count == ConditionCount * ConditionCount) return true;

            // If at the start of a new round, push the starting node as its own successor, update round HashSet, and proceed to next depth
            if (depth%ConditionCount == ConditionCount-1)
            {
                numbersUsedThisRound.Add(startingNode);

                SuccessorsStack[startingNode].Push(startingNode);
                SequenceStack.Push(startingNode);
                HashSet<int> nextRoundNumbers = [startingNode];

                // If DFS on subtree returns true, sequence found; propagate true upwards
                if (DFSTreeSearch(startingNode, depth + 1, nextRoundNumbers)) return true;

                SuccessorsStack[startingNode].Pop();
                SequenceStack.Pop();
                numbersUsedThisRound.Remove(startingNode);

                // If not, backtrack
                return false;
            }

            // Find which successors have already been used by this node's number (Hashset guarantees O(1) "Contains" check)
            var unavailableSuccessors = new HashSet<int>(SuccessorsStack[startingNode]);

            unavailableSuccessors.UnionWith(numbersUsedThisRound);
            unavailableSuccessors.UnionWith([startingNode]);

            var childrenList = Conditions
                .Where(i => !unavailableSuccessors.Contains(i))
                .ToArray();

            // Push children onto stack for easier and more efficient exploration
            var children = new Stack<int>(childrenList);

            while (children.Count > 0)
            {
                var nextChild = children.Pop();

                SuccessorsStack[startingNode].Push(nextChild);

                SequenceStack.Push(nextChild);

                numbersUsedThisRound.Add(nextChild);

                // If DFS on subtree returns true, sequence found; propagate true upwards
                if (DFSTreeSearch(nextChild, depth + 1, numbersUsedThisRound)) return true;

                // If not, backtrack: remove last added elements; loop will proceed with next candidates for children
                SuccessorsStack[startingNode].Pop();
                SequenceStack.Pop();
                numbersUsedThisRound.Remove(nextChild);
            }

            // Children exhausted, return false to backtrack
            return false;
        }

        private List<int> GenerateStarterSequence()
        {
            //var sequenceSize = ConditionCount * ConditionCount;

            //var starterSequence = Enumerable.Range(1, sequenceSize)
            //    .Select(_ => Rnd.Next(1, ConditionCount + 1))
            //    .ToList();

            //return starterSequence;
            return [.. StarterRound];
        }

        public List<int> GenerateSequence()
        {
            //for (int i = 1; i <= ConditionCount; i++)
            //{
            //    Sequence.AddRange(GenerateRound());
            //    MessageBox.Show(ToString());
            //}

            //if (!ValidateSequence(Sequence))
            //{
            //    return GenerateSequence();
            //}

            var startingNumber = Rnd.Next(1, ConditionCount + 1);

            SequenceStack.Push(startingNumber);

            if (DFSTreeSearch(startingNumber, 0, [startingNumber]))
            {
                MessageBox.Show(ToString());
                return [.. SequenceStack];
            }
            else
            {
                var errorMessage = "No valid sequence could be generated with the given parameters.";
                MessageBox.Show(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        private void SelectNextElement(int numberToFollow, Stack<int> choices)
        {
            if (SequenceStack.Count == ConditionCount*ConditionCount)
            {
                return;
            }
            if (choices.Count == 0)
            {
                choices = new Stack<int>(Enumerable.Range(1, ConditionCount));
            }

            Successors[numberToFollow].Add(choices.Pop());

            return;
        }

        private List<int> GenerateStarterRound()
        {
            var starterRound = Enumerable.Range(1, ConditionCount)
                .Select(_ => Rnd.Next(1, ConditionCount + 1))
                .ToList();

            var nonRepeatingRound = FixRepeatedElements(starterRound);

            for (int i = 0; i < ConditionCount - 1; i++)
            {
                Successors[nonRepeatingRound[i]] = [nonRepeatingRound[i + 1]];
            }

            return starterRound;
        }

        private List<int> GenerateRound()
        {
            var shuffledRound = new List<int>(StarterRound);

            return ShuffleRound(shuffledRound);
        }

        private List<int> ShuffleRound(List<int> roundToShuffle)
        {
            Rnd.Shuffle(roundToShuffle.ToArray());

            if (ValidateRound(roundToShuffle))
            {
                return roundToShuffle;
            }

            return ShuffleRound(roundToShuffle);
        }

        private bool ValidateRound(List<int> roundToValidate)
        {
            for (int i = 0; i < ConditionCount - 1; i++)
            {
                if (Successors.TryGetValue(roundToValidate[i], out List<int>? value) && value.Contains(roundToValidate[i + 1]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidateSequence(List<int> tentativeSequence)
        {
            int n = ConditionCount;

            // check sequence is of the right length (O(1))
            if (tentativeSequence.Count != n*n)
            {
                ResizeSequence();
                return false;
            }

            // last item in the sequence must be the same as the first (O(1))
            //if (tentativeSequence[^1] != tentativeSequence[0])
            //{
            //    EqualizeFirstAndLastItems();
            //    return false;
            //}

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
                .Where(item => item.PosInRound == n-1 && item.Index < tentativeSequence.Count - 1)
                .Any(item => tentativeSequence[item.Index + 1] != item.Value);

            if (numbersNotFollowedByThemselves)
            {
            //    FollowNumbersWithThemselves();
            //    return false;
            }

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

            if (repeatedElementsInRound)
            {
                FixRepeatedElementsInRound(roundsWithRepeatedElements);
                return false;
            }

            // no value should appear at position 0 more than once (O(n))
            var repeatedElementsInFirstPosition = indexedSequence
                .Where(item => item.PosInRound == 0)
                .GroupBy(item => item.Value)
                .Any(group => group.Count() > 1);

            if (repeatedElementsInFirstPosition)
            {
                FixRepeatedElementsInFirstPosition();
                return false;
            }

            // no value should appear at position n-1 more than once (O(n))
            var repeatedElementsInLastPosition = indexedSequence
                .Where(item => item.PosInRound == n-1)
                .GroupBy(item => item.Value)
                .Any(group => group.Count() > 1);

            if (repeatedElementsInLastPosition)
            {
                FixRepeatedElementsInLastPosition();
                return false;
            }

            // each element must appear n times in the sequence (O(n))
            var elementsDoNotAppearNTimes = Enumerable.Range(1, n)
                .Any(value => tentativeSequence.Count(x => x == value) != n);

            if (elementsDoNotAppearNTimes)
            {
                InsertElementsNTimes();
                return false;
            }

            // identical values must have different successors (O(n) to O(n²))
            var repeatedSequences = tentativeSequence
                .Select((value, index) => new { Value = value, Successor = (index + 1) < tentativeSequence.Count ? tentativeSequence[index + 1] : 0 })
                .GroupBy(item => item.Value)
                .Where(group => group.Count() > 1)
                .Any(group => group
                    .Select(item => item.Successor)
                    .Distinct()
                    .Count() < group.Count());

            if (repeatedSequences)
            {
                ChangeRepeatedSuccessors();
                return false;
            }

            return true;
        }

        private void ResizeSequence()
        {
            var sequenceLength = Sequence.Count;

            if (sequenceLength < ConditionCount)
            {
                while (Sequence.Count < ConditionCount)
                {
                    Sequence.Add(Rnd.Next(1, ConditionCount + 1));
                }
            }
            else if (sequenceLength > ConditionCount)
            {
                Sequence.RemoveRange(ConditionCount, sequenceLength - ConditionCount);
            }
        }

        private void EqualizeFirstAndLastItems()
        {
            Sequence[^1] = Sequence[0];
        }

        private void FollowNumbersWithThemselves()
        {
            var positionsOfElements = GetPositionsOfElements();

            var sequenceLength = Sequence.Count;

            for (int i = 1; i < ConditionCount; i++)
            {
                var indexOfLastItemInRound = i * ConditionCount - 1;

                var lastItemInRound = Sequence[indexOfLastItemInRound];

                var indexInNextRound = positionsOfElements[lastItemInRound]
                    .FirstOrDefault(index => index > indexOfLastItemInRound);

                var indexOfFirstItemInNextRound = indexOfLastItemInRound + 1 < sequenceLength ? indexOfLastItemInRound + 1 : 0;

                Sequence[indexInNextRound] = Sequence[indexOfFirstItemInNextRound];

                Sequence[indexOfFirstItemInNextRound] = lastItemInRound;
            }
        }

        private void FixRepeatedElementsInRound(List<int> roundsWithRepeatedElements)
        {
            foreach (var round in roundsWithRepeatedElements)
            {
                int startIndex = round * ConditionCount;
                var roundElements = Sequence.GetRange(startIndex, ConditionCount);

                var fixedRoundElements = FixRepeatedElements(roundElements);

                for (int i = 0; i < ConditionCount; i++)
                {
                    Sequence[startIndex + i] = fixedRoundElements[i];
                }
            }
        }

        private void FixRepeatedElementsInFirstPosition()
        {
            var positionOfElements = GetPositionsOfElements();

            var firstPositionElements = new List<int>();

            for (int i = 0; i < ConditionCount; i++)
            {
                firstPositionElements.Add(Sequence[i * ConditionCount]);
            }

            var fixedFirstPositionElements = FixRepeatedElements(firstPositionElements);

            for (int i = 0; i < ConditionCount; i++)
            {
                var indexOfFirstElement = i * ConditionCount;
                var firstElement = Sequence[indexOfFirstElement];

                var indexToPushFirstElementTo = positionOfElements[fixedFirstPositionElements[i]]
                    .FirstOrDefault(index => index > i * ConditionCount);

                Sequence[indexToPushFirstElementTo] = firstElement;
                Sequence[indexOfFirstElement] = fixedFirstPositionElements[i];
            }
        }

        private void FixRepeatedElementsInLastPosition()
        {
            var positionOfElements = GetPositionsOfElements();

            var lastPositionElements = new List<int>();

            for (int i = 1; i <= ConditionCount; i++)
            {
                lastPositionElements.Add(Sequence[i * ConditionCount - 1]);
            }

            var fixedLastPositionElements = FixRepeatedElements(lastPositionElements);

            for (int i = 1; i <= ConditionCount; i++)
            {
                var indexOfLastElement = i * ConditionCount - 1;
                var lastElement = Sequence[indexOfLastElement];

                var indexToPullLastElementTo = positionOfElements[fixedLastPositionElements[i - 1]]
                    .FirstOrDefault(index => index < i * ConditionCount - 1);

                Sequence[indexToPullLastElementTo] = lastElement;
                Sequence[indexOfLastElement] = fixedLastPositionElements[i - 1];
            }
        }

        private void InsertElementsNTimes()
        {
            var occurrences = GetPositionsOfElements();

            var extraIndices = new Queue<int>();
            var numbersMissingIndices = new Dictionary<int, int>();

            foreach (var entry in occurrences)
            {
                if (entry.Value.Count == ConditionCount) continue;

                if (entry.Value.Count < ConditionCount)
                {
                    numbersMissingIndices[entry.Key] = ConditionCount - entry.Value.Count;
                }

                if (entry.Value.Count > ConditionCount)
                {
                    foreach (var index in entry.Value.GetRange(ConditionCount, entry.Value.Count - ConditionCount))
                    {
                        extraIndices.Enqueue(index);
                    }
                    entry.Value.RemoveRange(ConditionCount, entry.Value.Count - ConditionCount);
                }
            }

            // for each number missing indices, assign one extra index to it and update the sequence accoRndingly
            foreach (var entry in numbersMissingIndices)
            {
                for (int i = 0; i < entry.Value; i++)
                {
                    if (extraIndices.Count == 0) break;
                    var indexToReplace = extraIndices.Dequeue();
                    Sequence[indexToReplace] = entry.Key;
                }
            }
        }

        private void ChangeRepeatedSuccessors()
        {
            var successors = new Dictionary<int, List<int>>();
            var occurences = new Dictionary<int, List<int>>();

            var sequenceLength = Sequence.Count;

            for (int i = 0; i < sequenceLength; i++)
            {
                var successor = (i + 1) < sequenceLength ? Sequence[i+1] : Sequence[0];

                if (!successors.TryGetValue(Sequence[i], out List<int>? value))
                {
                    successors[Sequence[i]] = [successor];
                    occurences[Sequence[i]] = [i];
                }
                else
                {
                    value.Add(successor);
                    occurences[Sequence[i]].Add(i);
                }
            }

            foreach (var entry in successors)
            {
                var distinctCount = entry.Value.Distinct().Count();
                if (distinctCount == entry.Value.Count) continue;

                var fixedEntry = FixRepeatedElements(entry.Value);
                var occurrenceIndices = occurences[entry.Key];

                for (int i = 0; i < entry.Value.Count; i++)
                {
                    if (entry.Value[i] != fixedEntry[i])
                    {
                        var occurrenceIndex = occurrenceIndices[i];

                        var indexInSequence = (occurrenceIndex + 1) < sequenceLength ? occurrenceIndex + 1 : 0;

                        Sequence[indexInSequence] = fixedEntry[i];
                    }
                }
            }
        }

        private List<int> FixRepeatedElements(List<int> list)
        {
            var modList = new List<int>(list);
            var valueSet = new HashSet<int>(modList);
            var repeatedIndices = new List<int>();
            var seen = new HashSet<int>();

            // Identify repeated element indices in single pass
            for (int i = 0; i < modList.Count; i++)
            {
                if (!seen.Add(modList[i]))
                {
                    repeatedIndices.Add(i);
                }
            }

            // Find missing elements efficiently
            var missingElements = new List<int>(repeatedIndices.Count);
            for (int i = 1; i <= ConditionCount; i++)
            {
                if (!valueSet.Contains(i))
                {
                    missingElements.Add(i);
                }
            }

            // Replace repeated elements with missing ones
            for (int i = 0; i < repeatedIndices.Count; i++)
            {
                if (i >= missingElements.Count) return [.. modList.Take(i)];

                modList[repeatedIndices[i]] = missingElements[i];
            }

            return modList;
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

            var seq = SequenceStack.ToList();

            for (int i = 0; i < seq.Count/ConditionCount; i++)
            {
                // take conditionCount elements for each round
                var roundElements = seq.GetRange(i * ConditionCount, ConditionCount);
                sequenceString += string.Join(", ", roundElements) + "\n";
            }

            return sequenceString;
        }
    }
}