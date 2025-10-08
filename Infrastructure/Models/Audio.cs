using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Audio
    {
        internal string FileName;
        internal int SnippetTimestamp;
        internal int SnippetDuration { get; set; }
        internal int TotalDuration;
        internal Lang Lang;

        internal Audio() { }
    }
}
