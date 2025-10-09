using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Managers;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Trial
    {
        internal Condition Condition;
        internal List<Audio> Audios;
        internal Keys? Response { get; set; }
        internal int Round;
        
        internal Trial(Condition condition, int round)
        {
            Audios = [];
            Condition = condition;
            Round = round;
        }
    }
}
