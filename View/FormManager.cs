using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LangBiasTester.View
{
    internal class FormManager
    {
        /// <summary>
        /// Configures the specified control with the provided properties and adds it to the form's control collection.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to configure and add to the form.</param>
        /// <param name="name">The name to assign to the control.</param>
        /// <param name="size">The size of the control, specified as a <see cref="Size"/>.</param>
        /// <param name="backColor">The background color to apply to the control.</param>
        /// <param name="font">The font to use for the control's text.</param>
        /// <param name="text">The text to display on the control.</param>
        internal static void SetUpControl(Control parentControl, Control control, string name, Size size, Color backColor, Font font, string text)
        {
            parentControl.Controls.Add(control);
            parentControl.SuspendLayout();

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
        /// <summary>
        /// Centers the specified control within its parent container.
        /// </summary>
        /// <param name="controlToCenter">The control to be centered. Must have a parent container.</param>
        internal static void CenterControl(Control controlToCenter)
        {
            if (controlToCenter.Parent != null)
            {
                controlToCenter.Location = new Point(
                    (controlToCenter.Parent.ClientSize.Width / 2) - (controlToCenter.Width / 2),
                    (controlToCenter.Parent.ClientSize.Height / 2) - (controlToCenter.Height / 2));
            }
        }
        /// <summary>
        /// Centers multiple controls within their parent container and aligns them either vertically or horizontally.
        /// </summary>
        /// <param name="controlsToCenter">The controls to be centered and aligned.</param>
        /// <param name="alignVertically">If true, aligns the controls vertically; otherwise, aligns them horizontally.</param>
        internal static void CenterControl(IEnumerable<Control> controlsToCenter, bool alignVertically)
        {
            if (controlsToCenter == null || !controlsToCenter.Any()) return;

            var parent = controlsToCenter.First().Parent;
            if (parent == null) return;

            int totalWidth = 0, totalHeight = 0;
            foreach (var control in controlsToCenter)
            {
                totalWidth += control.Width;
                totalHeight += control.Height;
            }

            int startX = (parent.ClientSize.Width / 2) - (alignVertically ? controlsToCenter.First().Width / 2 : totalWidth / 2);
            int startY = (parent.ClientSize.Height / 2) - (alignVertically ? totalHeight / 2 : controlsToCenter.First().Height / 2);

            foreach (var control in controlsToCenter)
            {
                control.Location = new Point(startX, startY);

                if (alignVertically)
                {
                    startY += control.Height;
                }
                else
                {
                    startX += control.Width;
                }
            }
        }
        internal static void GenericLabel(Label label, string text)
        {
            SetUpControl
                (
                label.Parent,
                label,
                "GenericLabel",
                new Size(1440, 1080),
                Color.Black,
                new Font("Arial", 24F, FontStyle.Bold, GraphicsUnit.Point),
                text
                );

            label.TextAlign = ContentAlignment.MiddleCenter;
        }
    }
}
