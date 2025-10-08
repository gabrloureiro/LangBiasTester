using LangBiasTester.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Experiment
    {
        internal RoundManager Rounds;
        
        internal Experiment()
        {
            Rounds = new RoundManager();
        }
    }
}
