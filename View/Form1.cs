using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators.DataHandlers;
using System.Security.Cryptography;
using System.Windows.Forms;
using NAudio.Wave;

namespace LangBiasTester
{
    public partial class Form1 : Form
    {
        //private KeyManager KeyManager;
        private readonly ExperimentManager _experiment;
        private readonly ParticipantInfo _participantInfo;

        private readonly Util _util;

        private TextBox _participantIDBox;
        private Label _label;

        private System.Windows.Forms.Timer _timer = new();

        private LinkedListNode<Trial>? _currentTrial;
        private int _currentRound;
        private Dictionary<WaveStream, WaveOut>? _waveStreams;

        internal Form1(ExperimentManager experiment, Util util)
        {
            InitializeComponent();

            //KeyManager = new KeyManager(this);
            _participantInfo = new ParticipantInfo();
            _participantIDBox = new TextBox();
            _label = new Label();

            _experiment = experiment;
            _util = util;
            ActiveControl = null;
        }

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
                    _participantInfo.ID = participantID;

                    e.SuppressKeyPress = true;

                    _participantIDBox.KeyDown -= ParticipantIDBox_KeyDown;
                    Controls.Remove(_participantIDBox);

                    RunInstructions();
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

            _currentTrial.Value.Response = null;

            Thread.Sleep(_experiment.Settings.TimeBetweenTrials*1000);

            NextTrial();
        }
        private void CatchResponse(object sender, KeyEventArgs e)
        {
            var response = e.KeyCode;
            if (response == Keys.Right || response == Keys.Left)
            {
                ResetTrial();

                e.SuppressKeyPress = true;

                _currentTrial.Value.Response = response;

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
            var runSettings = MessageBox.Show("Load audio files information?", "Settings", MessageBoxButtons.YesNo);
            if (runSettings == DialogResult.Yes)
            {
                _util.ExtractAudioMetadata();
            }
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

            foreach (KeyValuePair<WaveStream, WaveOut> stream in _waveStreams)
            {
                stream.Value.Stop();
                stream.Value.Dispose();
                stream.Key.Dispose();
            }
        }
        private void RunTrial(LinkedListNode<Trial>? currentTrial)
        {
            _waveStreams = [];

            foreach (Audio audio in currentTrial.Value.Audios)
            {
                var waveStream = new AudioFileReader(audio.FileLocation);
                var waveOut = new WaveOut();
                waveOut.Init(waveStream);

                _waveStreams.Add(waveStream, waveOut);
            }

            foreach (KeyValuePair<WaveStream, WaveOut> stream in _waveStreams)
            {
                stream.Value.Play();
            }

            KeyDown += CatchResponse; // subscribes KeyDown event
            TimerStart(); // subscribes timer end (Tick) event
            // whichever is triggered first will cancel the other and call RunNextTrial again
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
                SetUpControl //TODO write label
                    (
                    _label,
                    "Instructions",
                    new Size(1920, 1080),
                    Color.Black,
                    new Font("Arial", 48F, FontStyle.Bold, GraphicsUnit.Point, 0),
                    """
                    have a break have a kitkat
                    """
                    );
                KeyDown += StartNewRound;
            }
            else RunTrial(_currentTrial);
        }
        #endregion

        #region Action Methods
        private void Form1_Load(object sender, EventArgs e)
        {
            KeyPreview = true;

            //TODO: Add function which checks if settings is configured to run in demo mode, run a small "readme" if so

            GetParticipantID();
        }
        private void GetParticipantID()
        {
            SetUpControl
                (
                _participantIDBox,
                "ParticipantIDBox",
                new Size(200, 15),
                Color.DarkSlateGray,
                new Font("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                ""
                );

            _participantIDBox.PlaceholderText = "Participant ID";
            _participantIDBox.BorderStyle = BorderStyle.None;

            _participantIDBox.KeyDown += ParticipantIDBox_KeyDown;
        }
        //TODO: displays instructional messages. Don't forget to shuffle language settings. Calls RunTraining()
        private void RunInstructions()
        {
            SetUpControl
                (
                _label,
                "Instructions",
                new Size(1920, 1080),
                Color.Black,
                new Font("Arial", 48F, FontStyle.Bold, GraphicsUnit.Point, 0),
                """
                FR/EN INSTRUCTIONS
                
                EN/FR INSTRUCTIONS
                """
                );
            
            _label.TextAlign = ContentAlignment.MiddleCenter;

            KeyDown += Instructions_KeyDown;
        }
        private void RunExperiment()
        {
            MessageBox.Show("Running experiment"); //TODO remove

            _experiment.Init();

            _currentTrial = _experiment.Trials.First;
            _currentRound = 0;

            RunTrial(_currentTrial);
        }
        private void EndExperiment()
        {
            SetUpControl //TODO write concluding message
                (
                _label,
                "Instructions",
                new Size(1920, 1080),
                Color.Black,
                new Font("Arial", 48F, FontStyle.Bold, GraphicsUnit.Point, 0),
                """
                ciao thx grazie buonaseratta
                """
                );
            KeyDown += Exit;
        }
        #endregion
    }
}
