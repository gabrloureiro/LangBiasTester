using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Infrastructure.Models
{
    internal class ParticipantInfo
    {
        internal int ID;
        internal Lang PreferredLang; // should be modified once preferred language criteria is established (as per Dr. Titone's suggestions)

        internal ParticipantInfo(int id, Lang preferredLang)
        {
            this.ID = id;
            this.PreferredLang = preferredLang;
        }
    }
}
