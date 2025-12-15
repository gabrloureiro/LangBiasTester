namespace LangBiasTester.Infrastructure.Abstracts
{
    internal abstract class Audio
    {
        public string? FileName { get; set; }
        public string? FileLocation { get; set; }
        public TimeSpan Duration { get; set; }

        public int SnippetTimestamp { get; set; }
        public int SnippetDuration { get; set; }
    }
}
