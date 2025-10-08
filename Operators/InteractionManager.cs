using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Operators
{
    internal class InteractionHelper
    {
        internal Form1 form;
        internal Keys? keyPress;

        internal InteractionHelper()
        {
            
        }

        internal async void CollectInput(int responseWindow)
        {
            Keys? responseKey = await this.form.keyManager.WaitForKeyPressAsync(TimeSpan.FromSeconds(responseWindow));
            if (responseKey == Keys.Right || responseKey == Keys.Left)
            {
                this.keyPress = responseKey;
            }
            else this.keyPress = null;
        }
    }
}
