using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Audio
    {
        public string? FileName { get; set; }
        public string? FileLocation { get; set; }

        public TimeSpan Duration { get; set; }
        public Lang Lang { get; set; }
        public VoiceGender VoiceGender { get; set; }

        public int SnippetTimestamp { get; set; }
        public int SnippetDuration { get; set; }
    }
}
