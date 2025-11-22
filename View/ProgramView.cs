using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators.DataHandlers;
using static LangBiasTester.Infrastructure.Models.Lang;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace LangBiasTester
{
    public partial class ProgramView : Form
    {
        #region Properties
        private readonly Experiment _experiment;

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

        private int _programStage;
        #endregion

        internal ProgramView(Experiment experiment, Util util)
        {
            InitializeComponent();

            _experiment = experiment;
            _util = util;
            ActiveControl = null;
            _programStage = -1;
        }

        private void ProgramControl()
        {
            _programStage++;
            var runningDemoMode = _experiment.Settings.RunDemo;

            switch (_programStage)
            {
                case 0:
                    if (runningDemoMode) StartDemo();
                    else ProgramControl();
                    break;

                case 1:
                    if (runningDemoMode) IDScreenTutorial();
                    GetParticipantID();
                    break;

                case 2:
                    if (runningDemoMode) InstructionsTutorial();
                    else RunInstructions();
                    break;

                case 3:
                    RunExperiment();
                    break;

                case 4:
                    EndExperiment();
                    break;

                case 5:
                    Application.Exit();
                    break;
            }
        }

        #region Action Methods
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

            _participantIDBox.KeyDown += RegisterIDBoxInput;
        }
        private void RunInstructions()
        {
            Dictionary<Lang, string> instructions = [];
            instructions.Add(EN,
                $"You will be presented with a series of sounds. Your task is to indicate whether what you hear is English (by pressing the {_experiment.Settings.LangKeyDictionary[EN].ToString().ToLower()} arrow key) or French (by pressing the {_experiment.Settings.LangKeyDictionary[FR].ToString().ToLower()} arrow key)."
                );
            instructions.Add(FR, //add an ENFR dictionary to properly display these
                $"Vous serez presenté.e avec des différents sons. Votre rôle est d'indiquer si ce que vous entendiez est Français (en appuyant la flèche {_frenchKeysDict[_experiment.Settings.LangKeyDictionary[FR]].ToLower()}) ou Anglais (en appuyant la flèche {_frenchKeysDict[_experiment.Settings.LangKeyDictionary[EN]].ToLower()})."
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

            KeyDown += DismissInstructions;
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

        #region Event Methods
        private void RegisterIDBoxInput(object sender, KeyEventArgs e)
        {
            int participantID;
            var inputIsNumber = int.TryParse(_participantIDBox.Text, out participantID);

            if (!(e.KeyCode == Keys.Enter) || !inputIsNumber) return;
            
            if (participantID == _experiment.Settings.AdminID)
            {
                OpenSettingsMenu();
                return;
            }

            _experiment.ParticipantInfo.ID = participantID;

            e.SuppressKeyPress = true;
            _participantIDBox.KeyDown -= RegisterIDBoxInput;

            Controls.Remove(_participantIDBox);
            Controls.Remove(_label);

            _experiment.Init();

            ProgramControl();
        }
        private void DismissInstructions(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            KeyDown -= DismissInstructions;

            Controls.Remove(_label);

            ProgramControl();
        }
        private void TimeUp(object sender, EventArgs e)
        {
            ResetTrial();

            // If reaction time is not being tested, subscribe to keydown event to catch response after the trial ends.
            if (!_experiment.Settings.TestReactionTime)
            {
                KeyDown += CatchResponse;
                return;
            }

            // If time is up and reaction time is being tested, register no response.
            _currentTrial.Value.Response = null;
            _currentTrial.Value.ResponseLang = null;

            Thread.Sleep(_experiment.Settings.TimeBetweenTrials * 1000);

            NextTrial();
        }
        private void CatchResponse(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            var response = e.KeyCode;
            var validResponse = response == Keys.Right || response == Keys.Left;

            if (!validResponse) return;
            
            ResetTrial();
            RegisterResponse(response);

            Thread.Sleep(_experiment.Settings.TimeBetweenTrials * 1000);

            NextTrial();
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
            ProgramControl();
        }
        #endregion

        #region Trial Methods
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

            // Iterates through all audio snippets in the current trial and plays them simultaneously.
            // If mixed audio is used instead, only one stream will be created.
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

            // If reaction time is being tested, subscribe keydown event alongside timer event.
            // Whichever one is triggered first ends the current trial.
            if (_experiment.Settings.TestReactionTime) KeyDown += CatchResponse;
            TimerStart();
        }
        private void NextTrial()
        {
            _currentTrial = _currentTrial.Next;

            if (_currentTrial == null) ProgramControl();
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
        #endregion

        #region Response Methods
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
        private void RegisterResponse(Keys response)
        {
            _currentTrial.Value.Response = response;
            _currentTrial.Value.ResponseLang = _experiment.Settings.LangKeyDictionary.Where(entry => entry.Value == response).First().Key;
        }
        #endregion

        #region Form Control Methods
        private void Form1_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            ProgramControl();
        }
        /// <summary>
        /// Configures the specified control with the provided properties and adds it to the form's control collection.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to configure and add to the form.</param>
        /// <param name="name">The name to assign to the control.</param>
        /// <param name="size">The size of the control, specified as a <see cref="Size"/>.</param>
        /// <param name="backColor">The background color to apply to the control.</param>
        /// <param name="font">The font to use for the control's text.</param>
        /// <param name="text">The text to display on the control.</param>
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
        /// <summary>
        /// Centers the specified control within its parent container.
        /// </summary>
        /// <param name="controlToCenter">The control to be centered. Must have a parent container.</param>
        private void CenterControl(Control controlToCenter)
        {
            if (controlToCenter.Parent != null)
            {
                controlToCenter.Location = new Point(
                    (controlToCenter.Parent.ClientSize.Width / 2) - (controlToCenter.Width / 2),
                    (controlToCenter.Parent.ClientSize.Height / 2) - (controlToCenter.Height / 2));
            }
        }
        /// <summary>
        /// Centers multiple controls within their parent container and aligns them either vertically or horizontally.
        /// </summary>
        /// <param name="controlsToCenter">The controls to be centered and aligned.</param>
        /// <param name="alignVertically">If true, aligns the controls vertically; otherwise, aligns them horizontally.</param>
        private void CenterControl(IEnumerable<Control> controlsToCenter, bool alignVertically)
        {
            if (controlsToCenter == null || !controlsToCenter.Any()) return;

            var parent = controlsToCenter.First().Parent;
            if (parent == null) return;

            int totalWidth = 0, totalHeight = 0;
            foreach (var control in controlsToCenter)
            {
                totalWidth += control.Width;
                totalHeight += control.Height;
            }

            int startX = (parent.ClientSize.Width / 2) - (alignVertically ? controlsToCenter.First().Width / 2 : totalWidth / 2);
            int startY = (parent.ClientSize.Height / 2) - (alignVertically ? totalHeight / 2 : controlsToCenter.First().Height / 2);

            foreach (var control in controlsToCenter)
            {
                control.Location = new Point(startX, startY);

                if (alignVertically)
                {
                    startY += control.Height;
                }
                else
                {
                    startX += control.Width;
                }
            }
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
        private void OpenSettingsMenu()
        {
            Button audioJsonButton = new()
            {
                Text = "Collect Audio Metadata",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            Button createStaticMixButton = new()
            {
                Text = "Create Static Audio Mixes",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            GroupBox utilButtons = new()
            {
                Text = "Program setup",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            utilButtons.Controls.Add(audioJsonButton);
            utilButtons.Controls.Add(createStaticMixButton);

            CenterControl([audioJsonButton, createStaticMixButton], true);

            audioJsonButton.Click += (s, e) =>
            {
                _util.ExtractRawAudioMetadata(_experiment.Settings, true);
                MessageBox.Show("AudioMetadata JSON generated or rewritten.");
            };

            createStaticMixButton.Click += (s, e) =>
            {
                _util.CreateStaticAudioMixes(_experiment.Settings);
            };

            Form settingsForm = new()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            settingsForm.Controls.Add(utilButtons);
            CenterControl(utilButtons);

            settingsForm.Show();
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

            ProgramControl();
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

            foreach (KeyValuePair<int, int> condition in conditions)
            {
                conditionsString += $"{condition.Key}/{condition.Value}, ";
            }

            conditionsString = conditionsString.TrimEnd(',', ' ');

            GenericLabel
                (
                $"""
                The next screen presents the participants with simple instructions for the task. The order of the languages displayed is randomized every time the program is run, as is the direction which the participant has to press in order to register an answer as either English or French.
                All of these settings are also configurable within Settings.json, and they are recorded and exported along with the information and responses from the participant.
                
                At this point in the program, the trials have all been initialized with the following settings:
                Conditions: {conditionsString}
                Time between trials: {_experiment.Settings.TimeBetweenTrials} seconds
                Response window: {_experiment.Settings.ResponseWindow} seconds
                                
                After the next screen, the trials will run as normal, as if this was already a proper run of the experiment.
                There are "rest screens" every time a round of trials ends.
                
                Press any key to proceed.
                """
                );

            KeyDown += InstructionsTutorialKey;
        }
        #endregion

    }
}
