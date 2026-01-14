namespace LangBiasTester.View
{
    internal class DemoRunner
    {
        private readonly ProgramView _form;

        internal DemoRunner(ProgramView form)
        {
            _form = form;
        }

        internal void DemoControl(int programStage)
        {
            switch (programStage)
            {
                case 1:
                    StartDemo();
                    break;

                case 2:
                    IDScreenTutorial();
                    _form.GetParticipantID();
                    break;

                case 3:
                    InstructionsTutorial();
                    _form.RunInstructions();
                    break;

                case 4:
                    _form.RunExperiment();
                    break;

                case 5:
                    _form.EndExperiment();
                    break;

                case 6:
                    Application.Exit();
                    break;
            }
        }
        private void StartDemoKey(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            _form.KeyDown -= StartDemoKey;

            _form.Controls.Remove(_form._label);
            if (e.KeyCode == Keys.Escape)
            {
                _form._experiment.Settings.RunDemo = false;
            }

            _form.ProgramControl();
        }
        private void InstructionsTutorialKey(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            _form.KeyDown -= InstructionsTutorialKey;

            _form.Controls.Remove(_form._label);

            _form.RunInstructions();
        }
        private void StartDemo()
        {
            FormManager.GenericLabel
                (
                _form._label,
                """
                You are running this application in "Demo" mode. Text like this will appear throughout to guide you through the program.
                
                To disable this function in following runs, set the property "RunDemo" in Settings.json to false. To disable it now, press the escape key.
                
                To continue, press any other key.
                """
                );

            _form.KeyDown += StartDemoKey;
        }
        private void IDScreenTutorial()
        {
            FormManager.GenericLabel
                (
                _form._label,
                $"""
                In this screen, an identifier for the participant can be inserted in the field below. As is the case here, most screens were purposefully left with minimal text to avoid introducing any language bias.
                This screen can also be used as an override of the standard settings, through the use of a specified ID. Currently, this ID is set to {_form._experiment.Settings.AdminID}, but it can also be modified in Settings.json.
                To proceed, insert some number for the ID and hit the enter key.
                """
                );

            _form._label.TextAlign = ContentAlignment.TopCenter;
        }
        private void InstructionsTutorial()
        {
            var stimuli = _form._experiment.Settings.Stimuli;
            string stimuliString = "";

            foreach (var stimulus in stimuli)
            {
                stimuliString += $"{stimulus[0]}/{stimulus[1]}, ";
            }

            stimuliString = stimuliString.TrimEnd(',', ' ');

            FormManager.GenericLabel
                (
                _form._label,
                $"""
                The next screen presents the participants with simple instructions for the task. The order of the languages displayed is randomized every time the program is run, as is the direction which the participant has to press in order to register an answer as either English or French.
                All of these settings are also configurable within Settings.json, and they are recorded and exported along with the information and responses from the participant.
                
                At this point in the program, the trials have all been initialized with the following settings:
                Stimuli: {stimuliString}
                Time between trials: {_form._experiment.Settings.TimeBetweenTrials} seconds
                Response window: {_form._experiment.Settings.StimulusDuration} seconds
                                
                After the next screen, the trials will run as normal, as if this was already a proper run of the experiment.
                There are "rest screens" every time a round of trials ends.
                
                Press any key to proceed.
                """
                );

            _form.KeyDown += InstructionsTutorialKey;
        }
    }
}
