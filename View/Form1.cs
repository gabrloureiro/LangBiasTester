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
        internal readonly KeyManager KeyManager;
        internal ExperimentManager? Experiment;
        internal ParticipantInfo ParticipantInfo;
        internal Util Util;

        internal Form1(ExperimentManager experiment, Util util)
        {
            InitializeComponent();
            KeyPreview = true;
            KeyManager = new KeyManager(this);
            ParticipantInfo = new ParticipantInfo();

            Experiment = experiment;
            Util = util;
            ActiveControl = null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CenterControl(participantIDBox);
        }

        internal void CenterControl(Control controlToCenter)
        {
            if (controlToCenter.Parent != null)
            {
                controlToCenter.Location = new Point(
                    (controlToCenter.Parent.ClientSize.Width / 2) - (controlToCenter.Width / 2),
                    (controlToCenter.Parent.ClientSize.Height / 2) - (controlToCenter.Height / 2));
            }
        }

        private void ParticipantIDBox_KeyDown(object sender, KeyEventArgs e)
        {
            int participantID;

            if (e.KeyCode == Keys.Enter && int.TryParse(participantIDBox.Text, out participantID))
            {
                if (participantID != 42)
                {
                    ParticipantInfo.ID = participantID;

                    // Optionally, prevent the default "ding" sound or prevent the form from being submitted if it's the default accept button.
                    e.SuppressKeyPress = true;

                    this.Controls.Remove(participantIDBox);
                    MessageBox.Show("Participant ID registered: " + ParticipantInfo.ID.ToString());

                    RunInstructions();
                }
                //TODO: settings menu
                else MessageBox.Show("Settings menu will go here");
            }
        }

        //TODO: displays instructional messages. Don't forget to shuffle language settings. Calls RunTraining()
        private void RunInstructions() { }

        //TODO: runs a small training session (creates a small experiment with only two conditions -- 100/0, 0/100)
        private void RunTraining() { }

        //TODO: runs experiment, exports data. Maybe runs a "concluding" function.
        private void RunMainExperiment() { }

        //TODO: gets keyboard/mouse input, sets Response.LanguageIdentified.
        //Skips automatically after time specified by responseWindow.
        //Might want to use "await Task.Delay()".
        //Has to break as soon as it receives a response, or once time is up.
        internal async Task<Keys?> CollectResponse(int responseWindow)
        {
            Keys? responseKey = await this.KeyManager.WaitForKeyPressAsync(TimeSpan.FromSeconds(responseWindow));
            if (responseKey == Keys.Right || responseKey == Keys.Left)
            {
                return responseKey;
            }
            else return null;
        }
    }
}
