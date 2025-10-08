using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class ResponseManager
    {
        internal Response Response;

        internal ResponseManager()
        {
            this.Response = new Response();
        }

        //TODO: gets keyboard/mouse input, sets Response.LanguageIdentified.
        //Skips automatically after time specified by responseWindow.
        //Might want to use "await Task.Delay()".
        //Has to break as soon as it receives a response, or once time is up.
        internal async Task CollectResponse(int responseWindow)
        {
            
        }
    }
}