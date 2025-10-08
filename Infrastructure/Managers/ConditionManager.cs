using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class ConditionManager
    {
        internal List<Condition> Conditions;

        internal ConditionManager()
        {
            Conditions = [];
        }

        //TODO: split audios evenly according to conditions. Calls ValidateSplit to ensure it can be done.
        internal void EvenSplit() { }

        //TODO: check if audios can be evenly split given the conditions
        internal void ValidateSplit() { }
    }
}
