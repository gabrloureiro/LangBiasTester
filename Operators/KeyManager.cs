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
        
        private void CollectResponse(object sender, KeyEventArgs e)
        {
            // Set the result of the task with the key that was pressed.
            _keyTaskCompletionSource.TrySetResult(e.KeyCode);
        }

        internal async Task<Keys?> WaitForKeyPressAsync(TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(_keyTaskCompletionSource.Task, timeoutTask);

            KeyDown -= CollectResponse;

            if (completedTask == _keyTaskCompletionSource.Task)
            {
                return _keyTaskCompletionSource.Task.Result;
            }
            return null;
        }
    }

}
