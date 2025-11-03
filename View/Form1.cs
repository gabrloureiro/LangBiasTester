using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using static LangBiasTester.Infrastructure.Models.Lang;
using LangBiasTester.Operators.DataHandlers;
using System.Security.Cryptography;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using LangBiasTester.Operators;

namespace LangBiasTester
{
    public partial class Form1 : Form
    {
        #region Properties
        private readonly ExperimentManager _experiment;

        private readonly Util _util;

        private readonly TextBox _participantIDBox = new();
        private readonly Label _label = new();

        private readonly System.Windows.Forms.Timer _timer = new();
        private readonly Dictionary<Keys, string> _frenchKeysDict = new()
        {
            { Keys.Left, "Gauche" },
            { Keys.Right, "Droite" }
        };

        private LinkedListNode<Trial>? _currentTrial;
        private int _currentRound;
        private Dictionary<OffsetSampleProvider, WaveOut>? _waveStreams;

        internal Form1(ExperimentManager experiment, Util util)
        {
            InitializeComponent();

            _experiment = experiment;
            _util = util;
            ActiveControl = null;
        }
        #endregion

        #region Form Control Methods
        private void SetUpControl(Control control, string name, Size size, Color backColor, Font font, string text)
        {
            Controls.Add(control);
            SuspendLayout();

            control.Name = name;

            control.Anchor = AnchorStyles.None;
            control.Size = size;
            control.Location = new Point(0, 0);
            CenterControl(control);

            control.BackColor = backColor;
            control.ForeColor = Color.AntiqueWhite;

            control.Text = text;
            control.Font = font;

            control.TabIndex = 0;
        }
        private void CenterControl(Control controlToCenter)
        {
            if (controlToCenter.Parent != null)
            {
                controlToCenter.Location = new Point(
                    (controlToCenter.Parent.ClientSize.Width / 2) - (controlToCenter.Width / 2),
                    (controlToCenter.Parent.ClientSize.Height / 2) - (controlToCenter.Height / 2));
            }
        }
        #endregion

        #region Event Methods
        private void ParticipantIDBox_KeyDown(object sender, KeyEventArgs e)
        {
            int participantID;

            if (e.KeyCode == Keys.Enter && int.TryParse(_participantIDBox.Text, out participantID))
            {
                if (participantID != _experiment.Settings.AdminID)
                {
                    _experiment.ParticipantInfo.ID = participantID;

                    e.SuppressKeyPress = true;
                    _participantIDBox.KeyDown -= ParticipantIDBox_KeyDown;

                    Controls.Remove(_participantIDBox);
                    Controls.Remove(_label);

                    _experiment.Init();

                    if (_experiment.Settings.RunDemo) InstructionsTutorial();
                    else RunInstructions();
                }
                else RunSettings();
            }
        }
        private void Instructions_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            KeyDown -= Instructions_KeyDown;

            Controls.Remove(_label);

            RunExperiment();
        }
        private void TimeUp(object sender, EventArgs e)
        {
            ResetTrial();

            if (_experiment.Settings.TestReactionTime)
            {
                _currentTrial.Value.Response = null;
                _currentTrial.Value.ResponseLang = null;

                Thread.Sleep(_experiment.Settings.TimeBetweenTrials * 1000);

                NextTrial();
            }
            else KeyDown += CatchResponse;
        }
        private void CatchResponse(object sender, KeyEventArgs e)
        {
            var response = e.KeyCode;
            if (response == Keys.Right || response == Keys.Left)
            {
                ResetTrial();

                e.SuppressKeyPress = true;

                _currentTrial.Value.Response = response;
                _currentTrial.Value.ResponseLang = _experiment.Settings.LangKeyDictionary.Where(entry => entry.Value == response).First().Key;

                Thread.Sleep(_experiment.Settings.TimeBetweenTrials * 1000);

                NextTrial();
            }
        }
        private void StartNewRound(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            KeyDown -= StartNewRound;

            Controls.Remove(_label);

            _currentRound++;

            RunTrial(_currentTrial);
        }
        private void Exit(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            KeyDown -= Exit;
            Application.Exit();
        }
        #endregion

        #region Helper Methods
        private void RunSettings()
        //TODO: settings menu
        {
            MessageBox.Show("This menu has not been implemented yet. To run custom settings, modify the Settings.json file, but ensure to NEVER modify it during or leading up to the actual experiment.");
        }
        private void TimerStart()
        {
            _timer.Enabled = true;
            _timer.Tick += TimeUp;
            _timer.Interval = _experiment.Settings.ResponseWindow * 1000;
            _timer.Start();
        }
        private void TimerStop()
        {
            _timer.Stop();
            _timer.Enabled = false;
        }
        private void ResetTrial()
        {
            TimerStop();
            
            _timer.Tick -= TimeUp;
            KeyDown -= CatchResponse;

            foreach (KeyValuePair<OffsetSampleProvider, WaveOut> stream in _waveStreams)
            {
                stream.Value.Stop();
                stream.Value.Dispose();
            }
        }
        private void RunTrial(LinkedListNode<Trial>? currentTrial)
        {
            _waveStreams = [];

            foreach (Audio audio in currentTrial.Value.Audios)
            {
                var waveStream = new OffsetSampleProvider(new AudioFileReader(audio.FileLocation));
                var waveOut = new WaveOut();

                waveStream.SkipOver = TimeSpan.FromSeconds(audio.SnippetTimestamp);

                waveOut.Init(waveStream);

                _waveStreams.Add(waveStream, waveOut);
            }

            foreach (KeyValuePair<OffsetSampleProvider, WaveOut> stream in _waveStreams)
            {
                stream.Value.Play();
            }

            if (_experiment.Settings.TestReactionTime) KeyDown += CatchResponse; // subscribes KeyDown event
            
            TimerStart(); // subscribes timer end (Tick) event
        }
        private void NextTrial()
        {
            _currentTrial = _currentTrial.Next;

            if (_currentTrial == null)
            {
                EndExperiment();
            }
            else if (_currentTrial.Value.Round != _currentRound)
            {
                GenericLabel
                    (
                    """
                    Press any key to continue.
                    """
                    );
                KeyDown += StartNewRound;
            }
            else RunTrial(_currentTrial);
        }
        private void GenericLabel(string text)
        {
            SetUpControl
                (
                _label,
                "GenericLabel",
                new Size(1440, 1080),
                Color.Black,
                new Font("Arial", 24F, FontStyle.Bold, GraphicsUnit.Point),
                text
                );

            _label.TextAlign = ContentAlignment.MiddleCenter;
        }
        #endregion

        #region Demo Events
        private void StartDemoKey(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            KeyDown -= StartDemoKey;

            Controls.Remove(_label);
            if (e.KeyCode == Keys.Escape)
            {
                _experiment.Settings.RunDemo = false;
            }

            GetParticipantID();
        }
        private void InstructionsTutorialKey(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            KeyDown -= InstructionsTutorialKey;

            Controls.Remove(_label);

            RunInstructions();
        }
        #endregion

        #region Demo Methods
        private void StartDemo()
        {
            GenericLabel
                (
                """
                You are running this application in "Demo" mode. Text like this will appear throughout to guide you through the program.
                
                To disable this function in following runs, set the property "RunDemo" in Settings.json to false. To disable it now, press the escape key.
                
                To continue, press any other key.
                """
                );

            KeyDown += StartDemoKey;
        }
        private void IDScreenTutorial()
        {
            GenericLabel
                (
                $"""
                In this screen, an identifier for the participant can be inserted in the field below. As is the case here, most screens were purposefully left with minimal text to avoid introducing any language bias.
                This screen can also be used as an override of the standard settings, through the use of a specified ID. Currently, this ID is set to {_experiment.Settings.AdminID}, but it can also be modified in Settings.json.
                To proceed, insert some number for the ID and hit the enter key.
                """
                );
            _label.TextAlign = ContentAlignment.TopCenter;
        }
        private void InstructionsTutorial()
        {
            var conditions = _experiment.Settings.Conditions;
            string conditionsString = "";

            foreach (List<int> condition in conditions)
            {
                conditionsString += $"{condition[0]}/{condition[1]}, ";
            }

            conditionsString = conditionsString.TrimEnd(',', ' ');

            GenericLabel
                (
                $"""
                The next screen presents the participants with simple instructions for the task. The order of the languages displayed is randomized every time the program is run, as is the direction which the participant has to press in order to register an answer as either English or French.
                All of these settings are also configurable within Settings.json, and they are recorded and exported along with the information and responses from the participant.
                
                At this point in the program, the trials have all been initialized with the following settings:
                Conditions: {conditionsString}
                Rounds: {_experiment.Settings.Rounds}
                Trials per condition: {_experiment.Settings.TrialsPerCondition}
                Time between trials: {_experiment.Settings.TimeBetweenTrials} seconds
                Response window: {_experiment.Settings.ResponseWindow} seconds

                Currently, the algorithm to generate trials based on T1I1 sequences is not yet operational.
                Thus, at least as a minimum testing ground, the program is set up with a fixed number of trials per condition, and with a fixed number of conditions per round.
                
                After the next screen, the trials will run as normal, as if this was already a proper run of the experiment.
                There are "rest screens" every time a round of trials ends.
                
                Press any key to proceed.
                """
                );

            KeyDown += InstructionsTutorialKey;
        }
        #endregion

        #region Action Methods
        private void Form1_Load(object sender, EventArgs e)
        {
            KeyPreview = true;

            if (_experiment.Settings.RunDemo) StartDemo();
            else GetParticipantID();
        }
        private void GetParticipantID()
        {
            SetUpControl
                (
                _participantIDBox,
                "ParticipantIDBox",
                new Size(400, 60),
                Color.DimGray,
                new Font("Arial", 16F, FontStyle.Regular, GraphicsUnit.Point),
                ""
                );

            _participantIDBox.PlaceholderText = "Participant ID";
            _participantIDBox.BorderStyle = BorderStyle.None;

            _participantIDBox.KeyDown += ParticipantIDBox_KeyDown;

            if (_experiment.Settings.RunDemo) IDScreenTutorial();
        }
        private void RunInstructions()
        {
            Dictionary<Lang, string> instructions = [];
            instructions.Add(EN,
                $"You will be presented with a series of sounds. Your task is to indicate whether what you hear is English (by pressing the {
                    _experiment.Settings.LangKeyDictionary[EN].ToString().ToLower()
                    } arrow key) or French (by pressing the {
                    _experiment.Settings.LangKeyDictionary[FR].ToString().ToLower()
                    } arrow key)."
                );
            instructions.Add(FR, //add an ENFR dictionary to properly display these
                $"Vous serez presenté.e avec des différents sons. Votre rôle est d'indiquer si ce que vous entendiez est Français (en appuyant la flèche {
                    _frenchKeysDict[_experiment.Settings.LangKeyDictionary[FR]].ToLower()
                    }) ou Anglais (en appuyant la flèche {
                    _frenchKeysDict[_experiment.Settings.LangKeyDictionary[EN]].ToLower()
                    })."
                );

            SetUpControl
                (
                _label,
                "Instructions",
                new Size(1440, 1080),
                Color.Black,
                new Font("Arial", 32F, FontStyle.Bold, GraphicsUnit.Point, 0),
                $"{instructions[_experiment.Settings.TestLang1]}\n\n{instructions[_experiment.Settings.TestLang2]}"
                );
            
            _label.TextAlign = ContentAlignment.MiddleCenter;

            KeyDown += Instructions_KeyDown;
        }
        private void RunExperiment()
        {
            SetUpControl
                (
                _label,
                "FixationCross",
                new Size(1440, 1080),
                Color.Black,
                new Font("Arial", 48F, FontStyle.Regular, GraphicsUnit.Point),
                "+"
                );

            _currentTrial = _experiment.Trials.First;
            _currentRound = 0;

            RunTrial(_currentTrial);
        }
        private void EndExperiment()
        {
            GenericLabel //TODO change message -- add french and display according to lang1 lang2 order
                (
                """
                The experiment is over. Thank you for your participation.
                
                Press any key to exit.
                """
                );

            _experiment.ExportData();

            KeyDown += Exit;
        }
        #endregion
    }
}
