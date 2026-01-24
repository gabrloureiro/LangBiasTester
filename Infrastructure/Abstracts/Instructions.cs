namespace LangBiasTester.Infrastructure.Abstracts
{
    internal abstract class Instructions
    {
        internal Control Intro;
        internal Control Training;
        internal Control TrainingEnd;
        internal Control Response;
        internal Control BlockEnd;
        internal Control Exit;

        internal Instructions()
        {
            Intro = new();
            Training = new();
            TrainingEnd = new();
            Response = new();
            BlockEnd = new();
            Exit = new();
        }
        internal abstract void Init();
        internal void HideAll()
        {
            Intro.Hide();
            Training.Hide();
            TrainingEnd.Hide();
            Response.Hide();
            BlockEnd.Hide();
            Exit.Hide();
        }
    }
}
