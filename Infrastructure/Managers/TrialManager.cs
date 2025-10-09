using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class TrialManager
    {
        internal AudioManager audios;

        internal TrialManager() { }

        internal void Initialize(List<Trial> trials, List<Condition> conditions, int audioDuration)
        {
            foreach (Condition condition in conditions)
            {
                Trial trial = new Trial(condition);
                trial.AudioMix.PrepareMix(condition, audioDuration);

                trials.Add(trial);
            }
        }

        //TODO: implement T1I1 algorithm to shuffle "Trials" IN PLACE (see KW's matlab script)
        internal void T1I1() { }

        internal async void StartTrial(Trial trial, int responseWindow)
        {
            trial.AudioMix.Play();
            //await trial.Response.CollectResponse(responseWindow);
        }

        internal void Run(List<Trial> trials, int timeBetweenTrials, int responseWindow)
        {
            foreach(Trial trial in trials)
            {
                StartTrial(trial, responseWindow);
                Thread.Sleep(timeBetweenTrials);
            }
        }
    }
}
