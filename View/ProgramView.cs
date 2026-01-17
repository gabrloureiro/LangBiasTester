using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Operators.DataHandlers;
using LangBiasTester.View;
using LangBiasTester.Infrastructure.Enums;
using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester
{
    public partial class ProgramView : Form
    {
        #region Properties
        internal readonly Experiment _experiment;

        internal readonly Util _util;

        private readonly TextBox _participantIDBox;
        internal readonly Label _label;

        private readonly SettingsForm _settingsMenu;
        private readonly DemoRunner _demo;

        private readonly System.Windows.Forms.Timer _timer;

        private LinkedListNode<Trial>? _currentTrial;
        private int _currentTrialBlock;

        private int _programStage;
        #endregion

        /// <summary>
        /// Initializes a new instance of the ProgramView class with the specified experiment and utility objects.
        /// </summary>
        /// <param name="experiment">The Experiment instance that contains all information about trials, stimuli, and settings. Cannot be null.</param>
        /// <param name="util">The Util instance that supplies utility functions or services required by the view. Cannot be null.</param>
        internal ProgramView(Experiment experiment, Util util)
        {
            InitializeComponent();

            _experiment = experiment;
            _util = util;
            _programStage = 0;

            _participantIDBox = new();
            _label = new();
            _settingsMenu = new(_experiment.Settings, _util);
            _demo = new(this);

            _timer = new();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ActiveControl = null;
            KeyPreview = true;
            ProgramControl();
        }

        /// <summary>
        /// Advances the program to the next stage in the experiment workflow, executing the appropriate actions for the
        /// current stage.
        /// </summary>
        /// <remarks>
        /// This method manages the experiment's progression by incrementing the internal stage
        /// counter and invoking stage-specific logic, such as running tutorials, collecting participant information, or
        /// ending the experiment. The actions performed depend on the current stage and whether the experiment is
        /// running in demo mode.
        /// </remarks>
        internal void ProgramControl()
        {
            _programStage++;
            var runningDemoMode = _experiment.Settings.RunDemo;

            if (runningDemoMode)
            {
                _demo.DemoControl(_programStage);
                return;
            }

            switch (_programStage)
            {
                case 1: GetParticipantID();
                    break;

                case 2: RunInstructions();
                    break;

                case 3: RunExperiment();
                    break;

                case 4: EndExperiment();
                    break;

                case 5: Application.Exit();
                    break;
            }
        }

        #region Intro Methods
        /// <summary>
        /// Initializes and configures the participant ID input control.
        /// </summary>
        /// <remarks>
        /// This method sets up the participant ID text box with appropriate size, style,
        /// placeholder text, and event handlers. It should be called during the form's initialization to ensure the
        /// participant ID input is ready for user interaction.
        /// </remarks>
        internal void GetParticipantID()
        {
            FormManager.SetUpControl
                (
                this,
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

        /// <summary>
        /// Presents the participants with instructions. Order and direction of languages are randomized.
        /// </summary>
        /// <remarks>
        /// Also subscribes the dismiss instructions event.
        /// </remarks>
        internal void RunInstructions()
        {
            FormManager.SetUpControl
                (
                this,
                _label,
                "Instructions",
                new Size(1440, 1080),
                Color.Black,
                new Font("Arial", 32F, FontStyle.Bold, GraphicsUnit.Point, 0),
                _experiment.Instructions.Intro
                );

            _label.TextAlign = ContentAlignment.MiddleCenter;

            KeyDown += DismissInstructions;
        }
        #endregion

        #region Experiment Methods
        /// <summary>
        /// Initializes and starts the experiment by setting up the initial display and running the first trial.
        /// </summary>
        /// <remarks>
        /// This method should be called to begin the experiment sequence. It resets the current
        /// trial and round counters before starting the first trial. Subsequent trials are managed
        /// internally.
        /// </remarks>
        internal void RunExperiment()
        {
            _currentTrial = _experiment.Trials.First;
            _currentTrialBlock = 0;

            RunTrial(_currentTrial);
        }

        /// <summary>
        /// Performs cleanup and finalization tasks at the end of the experiment, including displaying a completion
        /// message, exporting experiment data, and preparing the application to exit.
        /// </summary>
        /// <remarks>
        /// Call this method when the experiment has concluded to ensure that all results are
        /// saved and the participant is informed that the session is complete. After this method is called, pressing
        /// any key will exit the application.
        /// </remarks>
        internal void EndExperiment()
        {
            FormManager.GenericLabel
                (
                _label,
                _experiment.Instructions.Exit
                );

            _experiment.ExportData();

            KeyDown += Exit;
        }

        internal void InterruptExperiment()
        {
            var exit = MessageBox.Show("It appears you have pressed the escape key. Would you like to terminate the experiment here?", "Interrupt Experiment?", MessageBoxButtons.YesNo);

            if (exit == DialogResult.Yes)
            {
                var keepData = MessageBox.Show("Would you like to register the data from this run?", "Keep Data?", MessageBoxButtons.YesNoCancel);

                if (keepData == DialogResult.Yes)
                {
                    KeyDown -= CatchResponse;
                    EndExperiment();
                }
                else if (keepData == DialogResult.Cancel) return;
                else Application.Exit();
            }
        }
        #endregion

        #region Event Methods
        private void RegisterIDBoxInput(object? sender, KeyEventArgs? e)
        {
            var inputIsNumber = int.TryParse(_participantIDBox.Text, out int participantID);

            if (e == null || e.KeyCode != Keys.Enter || !inputIsNumber) return;
            
            if (participantID == _experiment.Settings.AdminID)
            {
                _settingsMenu.Run();
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
        private void DismissInstructions(object? sender, KeyEventArgs? e)
        {
            if (e != null) e.SuppressKeyPress = true;
            KeyDown -= DismissInstructions;

            Controls.Remove(_label);

            ProgramControl();
        }
        private void TimeUp(object? sender, EventArgs? e)
        {
            // If reaction time is not being tested, collect response after the trial ends.
            // Does not currently have a logic to record the response time (if enabled)
            if (!_experiment.Settings.TestReactionTime)
            {
                CollectResponse();
                return;
            }

            StopTrial();
            ResetTrial();

            // If time is up and reaction time is being tested, register no response.
            _currentTrial.Value.Response = null;

            Thread.Sleep(_experiment.Settings.TimeBetweenTrials * 1000);

            NextTrial();
        }
        private void CatchResponse(object? sender, KeyEventArgs? e)
        {
            if (e == null) return;
            e.SuppressKeyPress = true;

            var response = e.KeyCode;

            if (response == Keys.Escape) InterruptExperiment();
            
            var validResponse = _experiment.Settings.ValidKeys.Contains(response);
            if (!validResponse) return;
            
            ResetTrial();

            // First Lang whose list of Keys contains the given response
            var responseLang = _experiment.Settings.LangKeyDictionary.Where(entry => entry.Value.Contains(response)).First().Key;

            // Confidence rating which corresponds to given response
            var responseConfidence = _experiment.Settings.TestConfidence ? _experiment.Settings.ConfidenceRatings[response] : 0;

            TrialManager.RegisterResponse(_currentTrial.Value, response, responseConfidence, responseLang);

            Thread.Sleep(_experiment.Settings.TimeBetweenTrials * 1000);

            NextTrial();
        }
        private void StartNewTrialBlock(object? sender, KeyEventArgs? e)
        {
            if (e != null) e.SuppressKeyPress = true;
            KeyDown -= StartNewTrialBlock;

            Controls.Remove(_label);

            _currentTrialBlock++;

            RunTrial(_currentTrial);
        }
        private void Exit(object? sender, KeyEventArgs? e)
        {
            if (e != null) e.SuppressKeyPress = true;
            KeyDown -= Exit;
            ProgramControl();
        }
        #endregion

        #region Trial Methods
        private void StopTrial()
        {
            TimerStop();
            TrialManager.EndTrial(_currentTrial.Value);
        }
        /// <summary>
        /// Resets the trial state by stopping timers and detaching event handlers.
        /// </summary>
        /// <remarks>Call this method to clean up resources and prepare for a new trial. This method
        /// should be invoked before starting a new trial to ensure that previous state and resources do not interfere
        /// with subsequent trials.</remarks>
        private void ResetTrial()
        {
            _timer.Tick -= TimeUp;
            KeyDown -= CatchResponse;
        }
        /// <summary>
        /// Executes a single trial by preparing and playing all associated audio snippets, and sets up
        /// response collection.
        /// </summary>
        /// <remarks>
        /// If the experiment is configured to test reaction time, this method subscribes to both
        /// keydown and timer events to capture the participant's response. All audio snippets for the trial are played
        /// simultaneously unless mixed audio is used, in which case only one stream is created.
        /// </remarks>
        /// <param name="currentTrial">The linked list node representing the current trial to run. Must not be null and must contain a valid trial
        /// with associated audio snippets.</param>
        private void RunTrial(LinkedListNode<Trial>? currentTrial)
        {
            FormManager.SetUpControl
                (
                this,
                _label,
                "FixationCross",
                new Size(1440, 1080),
                Color.Black,
                new Font("Arial", 48F, FontStyle.Regular, GraphicsUnit.Point),
                "+"
                );

            TrialManager.RunTrial(currentTrial.Value);
            if (_experiment.Settings.TestReactionTime) KeyDown += CatchResponse;
            TimerStart();
        }
        /// <summary>
        /// Advances to the next trial in the sequence, starting a new round if necessary.
        /// </summary>
        /// <remarks>
        /// If the next trial belongs to a new round, the method prompts the participant to continue and
        /// sets up the event handler to start the new round. Otherwise, it runs the next trial immediately.
        /// </remarks>
        private void NextTrial()
        {
            _currentTrial = _currentTrial.Next;

            if (_currentTrial == null) ProgramControl();
            else if (_currentTrial.Value.Block != _currentTrialBlock)
            {
                FormManager.GenericLabel
                    (
                        _label,
                        _experiment.Instructions.BlockEnd
                    );
                KeyDown += StartNewTrialBlock;
            }
            else RunTrial(_currentTrial);
        }

        private void CollectResponse()
        {
            StopTrial();

            FormManager.GenericLabel
                (
                    _label,
                    _experiment.Instructions.Response
                );
            KeyDown += CatchResponse;
        }
        #endregion

        #region Timer Methods
        private void TimerStart()
        {
            _timer.Enabled = true;
            _timer.Tick += TimeUp;
            _timer.Interval = _experiment.Settings.StimulusDuration * 1000;
            _timer.Start();
        }
        private void TimerStop()
        {
            _timer.Stop();
            _timer.Enabled = false;
        }
        #endregion
    }
}
