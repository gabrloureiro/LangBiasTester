using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Operators;
using LangBiasTester.Infrastructure.Enums;
using static LangBiasTester.Infrastructure.Enums.Lang;

namespace LangBiasTester.Infrastructure.Models
{
    internal class LangInstructions : Instructions
    {
        private readonly List<Lang>? TestLangs;

        public LangInstructions(Settings settings)
        {
            TestLangs = settings.TestLangs;

            SetIntroInstructions(settings.TestConfidence, settings.LangKeyDictionary);
            SetResponseInstructions();
            SetBlockEndInstructions();
            SetExitInstructions();
        }

        //TODO: write method to display instructions on the form?
        private void ShowInstructions()
        {

        }

        private void SetIntroInstructions(bool testConfidence, Dictionary<Lang, List<Keys>> langKeyDict)
        {
            var instructions = new Dictionary<Lang, string>();

            if (testConfidence)
            {
                instructions.Add(
                    EN, "A series of sounds will be presented to you. For each, your task is to indicate what language you hear and your degree of confidence in your response. " +
                        "There is no time limit for responding, but try to do so as quickly as possible after the instructions appear.");
                instructions.Add(
                    FR, "Une série de sons vous sera présentée. Pour chacun d’eux, votre tâche est d’indiquer quelle langue vous entendez et votre degré de confiance dans votre réponse. " +
                        "Il n’y a pas de limite de temps pour répondre, mais essayez de le faire le plus vite possible après l’affichage de la consigne.");
            }
            else
            {
                instructions.Add(
                    EN, $"A series of sounds will be presented to you. For each, your task is to indicate whether what you hear is " +
                        $"English (by pressing {langKeyDict[EN][0]}) or " +
                        $"French (by pressing {langKeyDict[FR][0]}). " +
                        $"There is no time limit to respond, but try to do so as quickly as possible after the instructions appear."
                    );

                instructions.Add(
                    FR, $"Une série de sons vous sera présentée. Pour chacun d’eux, votre tâche est d’indiquer si ce que vous entendez est " +
                        $"Français (en appuyant {langKeyDict[FR][0]}) ou " +
                        $"Anglais (en appuyant {langKeyDict[EN][0]}). " +
                        $"Il n’y a pas de limite de temps pour répondre, mais essayez de le faire le plus vite possible après l’affichage de la consigne."
                    );
            }

            Intro = $"{instructions[TestLangs[0]]}\n\n{instructions[TestLangs[1]]}";
        }

        private void SetResponseInstructions()
        {
            //TODO add 6 blocks of text, one for each confidence rating
            var instructions = new Dictionary<Lang, string>()
            {
                { EN, "" },
                { FR, "" }
            };

            Response =
                $"""
                {2} asd
                """;
        } 

        private void SetBlockEndInstructions()
        {
            //TODO change message
            var instructions = new Dictionary<Lang, string>()
            {
                { EN, "Press any key to continue" },
                { FR, "" }
            };

            BlockEnd = $"{instructions[TestLangs[0]]}\n\n{instructions[TestLangs[1]]}";
        }

        private void SetExitInstructions()
        {
            var instructions = new Dictionary<Lang, string>()
            {
                { EN,
                    """
                    The experiment is over. Thank you for your participation.
            
                    Press any key to exit.
                    """ 
                },
                { FR, "" }
            };

            Exit = $"{instructions[TestLangs[0]]}\n\n{instructions[TestLangs[1]]}";
        }
    }
}
