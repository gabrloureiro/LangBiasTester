using LangBiasTester.Operators;
using LangBiasTester.Operators.DataHandlers;

namespace LangBiasTester.View
{
    internal class SettingsForm : Form
    {
        private readonly Button _audioJsonButton;
        private readonly Button _createStaticMixButton;
        private readonly GroupBox _utilButtons;

        private readonly Settings _settings;
        private readonly Util _util;

        internal SettingsForm(Settings settings, Util util)
        {
            _settings = settings;
            _util = util;

            _audioJsonButton = new()
            {
                Text = "Collect Audio Metadata",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            _createStaticMixButton = new()
            {
                Text = "Create Static Audio Mixes",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            _utilButtons = new()
            {
                Text = "Program setup",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };


            _utilButtons.Controls.Add(_audioJsonButton);
            _utilButtons.Controls.Add(_createStaticMixButton);
            FormManager.CenterControl([_audioJsonButton, _createStaticMixButton], true);

            Controls.Add(_utilButtons);
            //FormManager.CenterControl(_utilButtons.Controls, true);
        }
        internal void Run()
        {
            KeyPreview = true;

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            _audioJsonButton.Click += (s, e) =>
            {
                _util.ExtractRawAudioMetadata(_settings, true);
                MessageBox.Show("AudioMetadata JSON generated or rewritten.");
            };

            _createStaticMixButton.Click += (s, e) =>
            {
                _util.CreateStaticAudioMixes(_settings);
            };

            Show();
        }
    }
}
