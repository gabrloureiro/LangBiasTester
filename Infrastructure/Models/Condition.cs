using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Condition
    {
        internal Dictionary<Lang, int> Langs;

        internal Condition(Lang lang1, Lang lang2, int lang1ct, int lang2ct)
        {
            Langs = [];
            Langs.Add(lang1, lang1ct);
            Langs.Add(lang2, lang2ct);
        }
    }
}
