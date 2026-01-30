using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.View;
using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester
{
    public partial class ExperimentView : Form
    {
        #region Properties
        internal readonly IExperiment _experiment;

        private readonly TextBox _participantIDBox;
        internal readonly Label _label;
        private readonly Label _fixationCross;

        private readonly System.Windows.Forms.Timer _timer;

        private LinkedListNode<Trial>? _currentTrial;
        private int _currentTrialSequence;
        private LinkedList<Stimulus>? _sampleStimuli;
        private LinkedListNode<Stimulus>? _currentSampleStimulus;

        private int _programStage;
        private readonly List<Action> _programStages;
        #endregion

        #region Form Methods
        /// <summary>
        /// Initializes a new instance of the ProgramView class with the specified experiment and utility objects. Sets up and links all controls and labels.
        /// </summary>
        /// <param name="experiment">The Experiment instance that contains all information about trials, stimuli, and settings. Cannot be null.</param>
        /// <param name="util">The Util instance that supplies utility functions or services required by the view. Cannot be null.</param>
        internal ExperimentView(IExperiment experiment)
        {
            InitializeComponent();

            _experiment = experiment;
            _programStage = -1;

            _participantIDBox = new()
            {
                Size = new Size(400, 60)
            };

            _timer = new();

            _label = new();
            _fixationCross = new()
            {
                Text = "+",
                TextAlign = ContentAlignment.MiddleCenter
            };

            _programStages =
                [
                    GetParticipantID,
                    RunInstructions,
                    TrainingInstructions,
                    RunExperiment,
                    EndExperiment,
                    Application.Exit
                ];
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ActiveControl = null;
            KeyPreview = true;
            SetUpControls();
            ProgramControl();
        }

        private void SetUpControls()
        {
            FormManager.LinkControls(this, [_participantIDBox, _label, _fixationCross]);

            FormManager.SetUpControl
                (
                _participantIDBox,
                "ParticipantIDBox",
                Color.DimGray,
                new Font("Arial", 16F, FontStyle.Regular, GraphicsUnit.Point)
                );

            FormManager.SetUpControl
                (
                _fixationCross,
                "FixationCross",
                Color.Black,
                new Font("Arial", 48F, FontStyle.Regular, GraphicsUnit.Point),
                new Size(1440, 1080)
                );

            _label.Hide();
            _participantIDBox.Hide();
            _fixationCross.Hide();
        }

        private void SetUpInstructions()
        {
            // Set up all controls
            FormManager.LinkControls(this, [_experiment.Instructions.Intro, _experiment.Instructions.Training, _experiment.Instructions.TrainingEnd, _experiment.Instructions.Response, _experiment.Instructions.BlockEnd, _experiment.Instructions.Exit]);
            SuspendLayout();

            FormManager.SetUpControl(_experiment.Instructions.Intro, "Instructions");
            FormManager.SetUpControl(_experiment.Instructions.Training, "TrainingInstructions");
            FormManager.SetUpControl(_experiment.Instructions.TrainingEnd, "TrainingInstructions");
            FormManager.SetUpControl(_experiment.Instructions.Response, "ResponseInstructions");
            FormManager.SetUpControl(_experiment.Instructions.Exit, "ExitInstructions");
            FormManager.SetUpControl(_experiment.Instructions.BlockEnd, "BlockEndInstructions");

            _experiment.Instructions.HideAll();
        }

        #endregion

        /// <summary>
        /// Advances the program to the next stage in the experiment workflow, executing the appropriate actions for the
        /// current stage.
        /// </summary>
        /// <remarks>
        /// This method manages the experiment's progression by incrementing the internal stage
        /// counter and invoking stage-specific logic, such as running tutorials, collecting participant information, or
        /// ending the experiment. The actions performed depend on the current stage.
        /// </remarks>
        internal void ProgramControl()
        {
            _programStage++;
            _programStages[_programStage]();
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
            _participantIDBox.Show();

            _participantIDBox.PlaceholderText = "Participant ID";
            _participantIDBox.BorderStyle = BorderStyle.None;

            _participantIDBox.KeyDown += RegisterIDBoxInput;
        }

        /// <summary>
        /// Presents the participants with instructions.
        /// </summary>
        /// <remarks>
        /// Also subscribes the dismiss instructions event.
        /// </remarks>
        internal void RunInstructions()
        {
            _experiment.Instructions.Intro.Show();
            KeyDown += DismissInstructions;
        }
        internal void TrainingInstructions()
        {
            if (!_experiment.Settings.RunTraining)
            {
                ProgramControl();
                return;
            }
            _experiment.Instructions.Training.Show();

            _timer.Tick += RunTraining;
            TimerStart(10);
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
            _currentTrialSequence = 0;

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
            _experiment.Instructions.Exit.Show();

            _experiment.ExportData();

            KeyDown += Exit;
        }

        internal void InterruptExperiment()
        {
            _experiment.Interrupted = true;
            var exit = MessageBox.Show("It appears you have pressed the escape key. Would you like to terminate the experiment here?", "Interrupt Experiment?", MessageBoxButtons.YesNo);

            if (exit == DialogResult.Yes)
            {
                var keepData = MessageBox.Show("Would you like to register the data from this run?", "Keep Data?", MessageBoxButtons.YesNoCancel);

                if (keepData == DialogResult.Yes)
                {
                    KeyDown -= StartNewTrialSequence;
                    KeyDown -= CatchResponse;
                    _experiment.Instructions.HideAll();
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
                _experiment.SettingsUI.Run();
                return;
            }

            _experiment.ParticipantInfo.ID = participantID;

            e.SuppressKeyPress = true;
            _participantIDBox.KeyDown -= RegisterIDBoxInput;

            _participantIDBox.Hide();

            _experiment.Init();

            SetUpInstructions();

            ProgramControl();
        }
        private void DismissInstructions(object? sender, KeyEventArgs? e)
        {
            if (e == null) return;
            
            e.SuppressKeyPress = true;
            KeyDown -= DismissInstructions;

            _experiment.Instructions.Intro.Hide();

            ProgramControl();
        }
        internal void RunTraining(object? sender, EventArgs? e)
        {
            _timer.Tick -= RunTraining;
            TimerStop();

            var sampleStimulusCount = (int)Math.Sqrt((_experiment.Stimuli.Count / _experiment.Settings.TrialBlocks) / _experiment.Settings.SequencesPerTrialBlock);

            _sampleStimuli = new(_experiment.Stimuli.GetRange(_experiment.Stimuli.Count - sampleStimulusCount, sampleStimulusCount).Select(StimulusManager.Clone));

            _currentSampleStimulus = _sampleStimuli.First;

            StimulusManager.RunStimulus(_currentSampleStimulus.Value);

            _timer.Tick += EndTrainingStimulus;
            TimerStart(_experiment.Settings.StimulusDuration * 1000);
        }
        private void EndTrainingStimulus(object? sender, EventArgs? e)
        {
            TimerStop();
            StimulusManager.StopStimulus(_currentSampleStimulus.Value);

            _currentSampleStimulus = _currentSampleStimulus.Next;

            Thread.Sleep(_experiment.Settings.TimeBetweenTrials * 1000);

            if (_currentSampleStimulus != null)
            {
                StimulusManager.RunStimulus(_currentSampleStimulus.Value);
                TimerStart(_experiment.Settings.StimulusDuration * 1000);
            }
            else
            {
                _timer.Tick -= EndTrainingStimulus;

                _experiment.Instructions.Training.Hide();
                _experiment.Instructions.TrainingEnd.Show();

                KeyDown += DismissTraining;
            }
        }
        private void DismissTraining(object? sender, KeyEventArgs? e)
        {
            if (e == null) return;
                
            e.SuppressKeyPress = true;
            KeyDown -= DismissTraining;

            _experiment.Instructions.TrainingEnd.Hide();

            if (e.KeyCode == _experiment.Settings.RepeatTrainingKey) TrainingInstructions();
            else
            {
                StimulusManager.DisposeOfMultipleStimuli(_experiment.Stimuli);
                ProgramControl();
            }
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

            if (response == Keys.Escape)
            {
                InterruptExperiment();
                return;
            }

            var validResponse = _experiment.Settings.ValidKeys.Contains(response);
            if (!validResponse) return;

            _experiment.Instructions.Response.Hide();
            ResetTrial();

            // Confidence rating which corresponds to given response
            var responseConfidence = _experiment.Settings.TestConfidence ? _experiment.Settings.ConfidenceRatings[response] : 0;

            // TODO: FIX (MESSY)
            TrialManager.RegisterResponse(_currentTrial.Value, response, responseConfidence);
            _experiment.SetTrialResponse(_currentTrial.Value);

            Thread.Sleep(_experiment.Settings.TimeBetweenTrials * 1000);

            NextTrial();
        }
        private void StartNewTrialSequence(object? sender, KeyEventArgs? e)
        {
            if (e == null) return;
            e.SuppressKeyPress = true;

            var key = e.KeyCode;

            if (key == Keys.Escape)
            {
                InterruptExperiment();
                return;
            }

            KeyDown -= StartNewTrialSequence;

            _experiment.Instructions.BlockEnd.Hide();

            _currentTrialSequence++;

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
            _fixationCross.Hide();
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
            _fixationCross.Show();
            TrialManager.RunTrial(currentTrial.Value);
            if (_experiment.Settings.TestReactionTime) KeyDown += CatchResponse;
            ResponseTimer(_experiment.Settings.StimulusDuration * 1000);
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

            // TODO: figure out where to place breaks (between blocks, sequences, etc)
            if (_currentTrial == null) ProgramControl();
            else if (_currentTrial.Value.Sequence != _currentTrialSequence)
            {
                _experiment.Instructions.BlockEnd.Show();
                KeyDown += StartNewTrialSequence;
            }
            else RunTrial(_currentTrial);
        }

        private void CollectResponse()
        {
            StopTrial();

            _experiment.Instructions.Response.Show();

            KeyDown += CatchResponse;
        }
        #endregion

        #region Timer Methods
        private void ResponseTimer(int interval)
        {
            _timer.Tick += TimeUp;
            TimerStart(interval);
        }
        private void TimerStart(int interval)
        {
            _timer.Enabled = true;
            _timer.Interval = interval;
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