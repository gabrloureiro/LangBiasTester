using System.Runtime.InteropServices;

namespace LangBiasTester.Operators.InputHandlers
{
    internal enum KeyboardSide { Left, Right }
    internal partial class KeyboardManager
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        // Scan codes for the left side (standard QWERTY physical positions)
        private static readonly HashSet<uint> LeftSideScanCodes =
        [
            0x01, // Esc
            0x29, 0x02, 0x03, 0x04, 0x05, 0x06, // ~, 1, 2, 3, 4, 5
            0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, // Tab, Q, W, E, R, T
            0x3A, 0x1E, 0x1F, 0x20, 0x21, 0x22, // Caps, A, S, D, F, G
            0x2A, 0x2C, 0x2D, 0x2E, 0x2F, 0x30  // L-Shift, Z, X, C, V, B
        ];

        public static bool IsLeftSided(Keys key)
        {
            // 1. Explicitly check for directional intent
            if (key == Keys.Left) return true;
            if (key == Keys.Right) return false;

            // 2. Check for Left/Right modifier variants
            if (key == Keys.LShiftKey || key == Keys.LControlKey) return true;
            if (key == Keys.RShiftKey || key == Keys.RControlKey) return false;

            // 3. Fall back to physical scan code for the main cluster
            uint scanCode = MapVirtualKey((uint)key, 0);
            return LeftSideScanCodes.Contains(scanCode);
        }

        public static Dictionary<KeyboardSide, List<Keys>> AssignListsToUI(List<Keys> listA, List<Keys> listB)
        {
            // Calculate how many "Left" keys are in each list
            int scoreA = listA.Count(k => IsLeftSided(k));
            int scoreB = listB.Count(k => IsLeftSided(k));

            // If List B has more left-side keys than List A, return listB as being left and listA as right, otherwise do the opposite
            if (scoreB > scoreA)
            {
                return new()
                {
                    { KeyboardSide.Left, listB },
                    { KeyboardSide.Right, listA }
                };
            }
            else
            {
                return new()
                {
                    { KeyboardSide.Left, listA },
                    { KeyboardSide.Right, listB }
                };
            }
        }
    }
}
