namespace LangBiasTester.Infrastructure.Abstracts
{
    internal abstract class Audio
    {
        public string? FileName { get; set; }
        public string? FileLocation { get; set; }
        public TimeSpan Duration { get; set; }

        public TimeSpan SnippetTimestamp { get; set; }
        public TimeSpan SnippetDuration { get; set; }
    }
}
