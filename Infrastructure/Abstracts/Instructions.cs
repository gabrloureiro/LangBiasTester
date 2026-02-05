using System.Reflection;

namespace LangBiasTester.Infrastructure.Abstracts
{
    public enum InstructionType
    {
        Intro,
        Training,
        TrainingEnd,
        ResponseTraining,
        ExperimentStart,
        Response,
        BlockEnd,
        End
    }

    internal abstract class Instructions
    {
        protected Dictionary<InstructionType, Control> InstructionDict = [];

        internal Instructions()
        {
            foreach (var instructionType in Enum.GetValues(typeof(InstructionType)).Cast<InstructionType>())
            {
                InstructionDict.Add(instructionType, new Control());
            }
        }
        internal abstract void Init();
        internal void HideAll()
        {
            foreach (var instruction in InstructionDict)
            {
                instruction.Value.Hide();
            }
        }
        internal void Show(InstructionType instruction)
        {
            InstructionDict[instruction].Show();
        }
        internal void Hide(InstructionType instruction)
        {
            InstructionDict[instruction].Hide();
        }
        internal List<Control> GetControls() => [.. InstructionDict.Values];
    }
}
