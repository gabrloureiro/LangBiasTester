using LangBiasTester.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Round
    {
        internal TrialManager Trials;

        internal Round()
        {
            Trials = new TrialManager();
        }
    }
}
