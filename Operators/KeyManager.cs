using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Operators
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public class KeyManager
    {
        // A TaskCompletionSource to represent the asynchronous operation.
        private readonly TaskCompletionSource<Keys?> _keyTaskCompletionSource = new ();
        private readonly Form _form;

        internal KeyManager(Form form)
        {
            _form = form;
            // Start listening for key presses on the specified form.
            _form.KeyDown += Form_KeyDown;
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            // Set the result of the task with the key that was pressed.
            _keyTaskCompletionSource.TrySetResult(e.KeyCode);
        }

        internal async Task<Keys?> WaitForKeyPressAsync(TimeSpan timeout)
        {
            // Create a task that completes when the timeout expires.
            var timeoutTask = Task.Delay(timeout);

            // Wait for either the key press task or the timeout task to complete.
            var completedTask = await Task.WhenAny(_keyTaskCompletionSource.Task, timeoutTask);

            // Stop listening for key presses after the task is complete.
            _form.KeyDown -= Form_KeyDown;

            // Check which task completed first.
            if (completedTask == _keyTaskCompletionSource.Task)
            {
                // The key was pressed, check the result.
                return _keyTaskCompletionSource.Task.Result;
            }
            // Timeout or invalid key.
            return null;
        }
    }

}
