using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators.DataHandlers;
using System.Security.Cryptography;
using System.Windows.Forms;

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

                    // Optionally, prevent the default "ding" sound or prevent the form from being submitted if it's the default accept button.
                    e.SuppressKeyPress = true;

                    _participantIDBox.KeyDown -= ParticipantIDBox_KeyDown;
                    Controls.Remove(_participantIDBox);

                    MessageBox.Show("Participant ID registered: " + _participantInfo.ID.ToString()); //change when settings

                    RunInstructions();
                }
                //TODO: settings menu
                else MessageBox.Show("Settings menu will go here");
            }
        }
        private void Instructions_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            Controls.Remove(_label);

            MessageBox.Show("Instructions check"); //remove

            KeyDown -= Instructions_KeyDown;

            RunExperiment();
        }
        private void TimeUp(object sender, EventArgs e)
        {
            ResetTrial();

            MessageBox.Show("Time's up"); //remove

            _currentTrial.Value.Response = null;

            Thread.Sleep(_experiment.Settings.TimeBetweenTrials*1000);

            RunTrial(_currentTrial.Next);
        }
        private void CatchResponse(object sender, KeyEventArgs e)
        {
            ResetTrial();

            e.SuppressKeyPress = true;

            MessageBox.Show("Response: " + e.KeyCode); //remove

            _currentTrial.Value.Response = e.KeyCode;

            Thread.Sleep(_experiment.Settings.TimeBetweenTrials * 1000);

            RunTrial(_currentTrial.Next);
        }
        #endregion

        #region Helper Methods
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
        }
        private void RunTrial(LinkedListNode<Trial>? currentTrial)
        {
            if (currentTrial == null)
            {
                EndExperiment();
            }
            else
            {
                _experiment.RunTrial(currentTrial.Value);

                MessageBox.Show("Running trial"); //remove

                KeyDown += CatchResponse; // subscribes KeyDown event
                TimerStart(); // subscribes timer end (Tick) event
                // whichever is triggered first will cancel the other and call RunNextTrial again
            }
        }
        #endregion

        #region Action Methods
        private void Form1_Load(object sender, EventArgs e)
        {
            KeyPreview = true;

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
        //TODO: runs a small training session (creates a small experiment with only two conditions -- 100/0, 0/100)
        private void RunExperiment()
        {
            MessageBox.Show("Running experiment"); //remove

            _experiment.Init();

            _currentTrial = _experiment.Trials.First;

            RunTrial(_currentTrial);
        }
        private void EndExperiment()
        {
            MessageBox.Show("Experiment complete"); //substitute for label, add key event
            Thread.Sleep(1000);
            Application.Exit();
        }
        #endregion
    }
}
