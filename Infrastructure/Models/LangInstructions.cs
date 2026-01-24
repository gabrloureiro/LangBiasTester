using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Operators;
using LangBiasTester.Infrastructure.Enums;
using static LangBiasTester.Infrastructure.Enums.Lang;
using LangBiasTester.Operators.InputHandlers;
using LangBiasTester.View;
using System.Security.Cryptography;

namespace LangBiasTester.Infrastructure.Models
{
    internal class LangInstructions : Instructions
    {
        private readonly LangSettings _settings;

        public LangInstructions(LangSettings settings)
        {
            if (settings.TestLangs == null) ErrorManager.ThrowError("Missing information about languages to be tested. Check settings.json.", "Error: Could Not Create Instructions");
            if (settings.LangKeys == null) ErrorManager.ThrowError("Missing key assignment for languages. Check settings.json.", "Error: Could Not Create Instructions");

            Intro = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter
            };
            Training = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter
            };
            TrainingEnd = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter
            };
            BlockEnd = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter
            };
            Exit = new Label()
            {
                TextAlign = ContentAlignment.MiddleCenter
            };

            _settings = settings;
        }

        internal override void Init()
        {
            SetIntroInstructions(_settings.TestConfidence);

            if (_settings.RunTraining)
            {
                var trainingKey = (Keys)_settings.RepeatTrainingKey;
                SetTrainingInstructions();
                SetTrainingEndInstructions(trainingKey);
            }
            SetResponseInstructions();
            SetBlockEndInstructions();
            SetExitInstructions();
        }

        private void SetIntroInstructions(bool testConfidence)
        {
            var instructions = new Dictionary<Lang, string>();

            if (testConfidence)
            {
                instructions.Add(
                    EN, "A series of sounds will be presented to you. For each, your task is to indicate what language you hear and your level of certainty in your response. " +
                        "There is no time limit for responding, but try to do so as quickly as possible after the instructions appear.");
                instructions.Add(
                    FR, "Une série de sons vous sera présentée. Pour chacun d’eux, votre tâche est d’indiquer quelle langue vous entendez et votre degré de certitude dans votre réponse. " +
                        "Il n’y a pas de limite de temps pour répondre, mais essayez de le faire le plus vite possible après l’affichage de la consigne.");
            }
            else
            {
                instructions.Add(
                    EN, $"A series of sounds will be presented to you. For each, your task is to indicate whether what you hear is " +
                        $"English (by pressing {_settings.LangKeyDictionary[EN][0]}) or " +
                        $"French (by pressing {_settings.LangKeyDictionary[FR][0]}). " +
                        $"There is no time limit to respond, but try to do so as quickly as possible after the instructions appear."
                    );

                instructions.Add(
                    FR, $"Une série de sons vous sera présentée. Pour chacun d’eux, votre tâche est d’indiquer si ce que vous entendez est " +
                        $"Français (en appuyant {_settings.LangKeyDictionary[FR][0]}) ou " +
                        $"Anglais (en appuyant {_settings.LangKeyDictionary[EN][0]}). " +
                        $"Il n’y a pas de limite de temps pour répondre, mais essayez de le faire le plus vite possible après l’affichage de la consigne."
                    );
            }

            Intro.Text = $"{instructions[_settings.TestLangs[0]]}\n\n{instructions[_settings.TestLangs[1]]}";
        }

        private void SetTrainingInstructions()
        {
            //TODO change message
            var instructions = new Dictionary<Lang, string>()
            {
                { EN, "These are some examples of sounds you will hear." },
                { FR, "Voici quelques exemples des sons que vous allez entendre." }
            };

            Training.Text = $"{instructions[_settings.TestLangs[0]]}\n\n{instructions[_settings.TestLangs[1]]}";
        }

        private void SetTrainingEndInstructions(Keys repeatTrainingKey)
        {
            //TODO change message
            var instructions = new Dictionary<Lang, string>()
            {
                { EN, $"Press {repeatTrainingKey} to repeat these examples. Press any other key to advance to trials." },
                { FR, $"Appuyez sur {repeatTrainingKey} pour répéter ces exemples. Appuyez sur toute autre touche pour passer aux essais." }
            };

            TrainingEnd.Text = $"{instructions[_settings.TestLangs[0]]}\n\n{instructions[_settings.TestLangs[1]]}";
        }

        private void SetResponseInstructions()
        {
            var instructions = new Dictionary<Lang, string>()
            {
                { EN, "Please press the key corresponding to the language and your level of certainty." },
                { FR, "Veuillez appuyer sur la touche correspondant à la langue et à votre niveau de certitude." }
            };

            var confidenceRatings = new Dictionary<Lang, List<string>>()
            {
                { EN, ["not certain", "moderately certain", "very certain"] },
                { FR, ["pas certain.e", "moyennement certain.e", "très certain.e"] }
            };

            var languages = new Dictionary<Lang, Dictionary<Lang, string>>()
            {
                { EN, new()
                    {
                        { EN, "English" },
                        { FR, "French" }
                    }
                },
                { FR, new()
                    {
                        { EN, "Anglais" },
                        { FR, "Français" }
                    }
                }
            };

            // Get position of keys on keyboard
            var keySides = KeyboardManager.AssignListsToUI(_settings.LangKeys[0], _settings.LangKeys[1]);

            keySides[KeyboardSide.Left].Reverse();

            // From position of keys, determine which side each langugage should be on
            var langSides = keySides.Select(sides => (sides.Key, _settings.LangKeyDictionary.FirstOrDefault(entry => entry.Value.SequenceEqual(sides.Value)).Key)).ToDictionary();

            var strings = new List<List<string>>();

            // Padding to separate both halves of the grid
            var padding = "       ";

            for (int i = 0; i < _settings.TestLangs.Count; i++)
            {
                var innerList = new List<string>();
                foreach (var side in Enum.GetValues<KeyboardSide>())
                {
                    var langChoice = langSides[side];
                    var textLang = _settings.TestLangs[i];
                    var str = confidenceRatings[textLang].Select(confidence => languages[textLang][langChoice] + ",\n" + confidence).ToList();

                    // If on the left side, reverse order to preserve low confidence ratings in the middle and add space to separate sides
                    if (side == KeyboardSide.Left)
                    {
                        str.Reverse();
                        str.Add(padding);
                    }
                    innerList.AddRange(str);
                }

                strings.Add(innerList);

                if (i == 0)
                {
                    // Order langs by KeyboardSide enum (Left first), map to keys, and convert keys to strings
                    var keys = _settings.TestLangs
                        .OrderBy(lang => langSides.Where(entry => entry.Value == lang).First().Key)
                        .SelectMany(lang => _settings.LangKeyDictionary![lang].Select(key => key.ToString()))
                        .ToList();

                    // Get position of last key on left side to insert padding between sides
                    var emptyStringIndex = keySides[KeyboardSide.Left].Count;
                    keys.Insert(emptyStringIndex, padding);
                    strings.Add(keys);
                }
            }

            var header = instructions[_settings.TestLangs[0]];
            var footer = instructions[_settings.TestLangs[1]];

            Response = FormManager.LabelGrid(header, footer, strings);
        } 

        private void SetBlockEndInstructions()
        {
            //TODO change message
            var instructions = new Dictionary<Lang, string>()
            {
                { EN, "This block of trials is over. Feel free to take some time to rest. Press any key when you are ready to continue." },
                { FR, "Ce bloc d'essais est terminé. N'hésitez pas à prendre un moment pour vous reposer. Appuyez sur n'importe quelle touche quand vous êtes prêt.e à continuer." }
            };

            BlockEnd.Text = $"{instructions[_settings.TestLangs[0]]}\n\n{instructions[_settings.TestLangs[1]]}";
        }

        private void SetExitInstructions()
        {
            //TODO change message
            var instructions = new Dictionary<Lang, string>()
            {
                { EN, "The experiment is over. Thank you for your participation." },
                { FR, "L'expérience est terminée. Merci de votre participation." }
            };

            Exit.Text = $"{instructions[_settings.TestLangs[0]]}\n\n{instructions[_settings.TestLangs[1]]}";
        }
    }
}
