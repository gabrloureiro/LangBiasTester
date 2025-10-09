using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using LangBiasTester.Operators.DataHandlers;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace LangBiasTester
{
    public partial class Form1 : Form
    {
        private KeyManager KeyManager;
        private ExperimentManager Experiment;
        private ParticipantInfo ParticipantInfo;
        private Util Util;
        private TextBox ParticipantIDBox;
        private Label Label;

        internal Form1(ExperimentManager experiment, Util util)
        {
            InitializeComponent();
            KeyPreview = true;
            KeyManager = new KeyManager(this);
            ParticipantInfo = new ParticipantInfo();
            ParticipantIDBox = new TextBox();
            Label = new Label();

            Experiment = experiment;
            Util = util;
            ActiveControl = null;
        }

        #region Control methods
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

            if (e.KeyCode == Keys.Enter && int.TryParse(ParticipantIDBox.Text, out participantID))
            {
                if (participantID != 42)
                {
                    ParticipantInfo.ID = participantID;

                    // Optionally, prevent the default "ding" sound or prevent the form from being submitted if it's the default accept button.
                    e.SuppressKeyPress = true;

                    ParticipantIDBox.KeyDown -= ParticipantIDBox_KeyDown;
                    Controls.Remove(ParticipantIDBox);

                    MessageBox.Show("Participant ID registered: " + ParticipantInfo.ID.ToString()); //change when settings

                    RunInstructions();
                }
                //TODO: settings menu
                else MessageBox.Show("Settings menu will go here");
            }
        }

        private void Instructions_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            Controls.Remove(Label);
            KeyDown -= Instructions_KeyDown;

            RunTraining();
        }
        #endregion

        #region Action methods
        private void Form1_Load(object sender, EventArgs e)
        {
            GetParticipantID();
        }

        private void GetParticipantID()
        {
            SetUpControl
                (
                ParticipantIDBox,
                "ParticipantIDBox",
                new Size(200, 15),
                Color.DarkSlateGray,
                new Font("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                ""
                );

            ParticipantIDBox.PlaceholderText = "Participant ID";
            ParticipantIDBox.BorderStyle = BorderStyle.None;

            ParticipantIDBox.KeyDown += ParticipantIDBox_KeyDown;
        }

        //TODO: displays instructional messages. Don't forget to shuffle language settings. Calls RunTraining()
        private void RunInstructions()
        {
            SetUpControl
                (
                Label,
                "Instructions",
                new Size(1920, 1080),
                Color.Black,
                new Font("Consolas", 48F, FontStyle.Bold, GraphicsUnit.Point, 0),
                """
                FR INSTRUCTIONS
                
                EN INSTRUCTIONS
                """
                );
            
            Label.TextAlign = ContentAlignment.MiddleCenter;

            KeyDown += Instructions_KeyDown;
        }

        //TODO: runs a small training session (creates a small experiment with only two conditions -- 100/0, 0/100)
        private void RunTraining()
        {
            MessageBox.Show("Starting Training"); //remove
            ExperimentManager training = new()
            {
                // Change parameters so they come from settings.json
                TimeBetweenTrials = 1,
                ResponseWindow = 2,
                Rounds = 2,
                TrialsPerCondition = 5
            };

            training.Conditions.Add(new Condition(Lang.EN, Lang.FR, 1, 0));
            training.Conditions.Add(new Condition(Lang.EN, Lang.FR, 0, 1));

            KeyDown += CollectResponse;
        }

        //TODO: runs experiment, exports data. Maybe runs a "concluding" function.
        private void RunMainExperiment() { }
        #endregion

        //TODO: gets keyboard/mouse input, sets Response.LanguageIdentified.
        //Skips automatically after time specified by responseWindow.
        //Might want to use "await Task.Delay()".
        //Has to break as soon as it receives a response, or once time is up.
        private async void CollectResponse(object sender, KeyEventArgs e)
        {
            Keys? responseKey = await KeyManager.WaitForKeyPressAsync(TimeSpan.FromSeconds(Experiment.ResponseWindow));
            if (responseKey == Keys.Right || responseKey == Keys.Left)
            {
                RegisterResponse(responseKey);
            }
            else RegisterResponse(null);
        }

        private void RegisterResponse(Keys? responseKey)
        {

        }
    }
}
