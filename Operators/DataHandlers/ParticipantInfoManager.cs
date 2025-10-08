using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester.Operators.DataHandlers
{
    internal class ParticipantInfoManager
    {
        internal ParticipantInfo ParticipantInfo;

        internal ParticipantInfoManager(int id, Lang preferredLang)
        {
            this.ParticipantInfo = new ParticipantInfo(id, preferredLang);
        }
    }
}
