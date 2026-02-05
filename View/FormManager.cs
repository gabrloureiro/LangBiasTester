using System.Windows.Forms;

namespace LangBiasTester.View
{
    internal class FormManager
    {
        internal static void LinkControls(Control parentControl, List<Control> controls)
        {
            foreach (var control in controls)
            {
                parentControl.Controls.Add(control);
                control.Parent = parentControl;
            }
        }
        /// <summary>
        /// Configures the specified control with the provided properties and adds it to the form's control collection.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to configure and add to the form.</param>
        /// <param name="name">The name to assign to the control.</param>
        /// <param name="size">The size of the control, specified as a <see cref="Size"/>.</param>
        /// <param name="backColor">The background color to apply to the control.</param>
        /// <param name="font">The font to use for the control's text.</param>
        /// <param name="text">The text to display on the control.</param>
        internal static void SetUpControl(Control control, Color backColor, Font font, Size size = default, string? name = null)
        {
            if (name != null) control.Name = name;

            // Position and size
            control.Anchor = AnchorStyles.None;
            if (size != default) control.Size = size;
            CenterControl(control);

            // Text and appearance
            control.BackColor = backColor;
            control.ForeColor = Color.AntiqueWhite;
            control.Font = font;

            control.TabIndex = 0;
        }
        internal static void SetUpControl(Control control, string? name = null)
        {
            var backColor = Color.Black;
            var size = new Size(1440, 1080);
            var font = new Font("Arial", 32F, FontStyle.Bold, GraphicsUnit.Point, 0);

            SetUpControl(control, backColor, font, size, name);
        }
        /// <summary>
        /// Displays a grid of labels within the specified parent control, using the provided grid data.
        /// </summary>
        /// <remarks>The method creates a TableLayoutPanel that fills the parent control and populates it
        /// with labels based on the contents of gridData. The number of columns is determined by the longest row in
        /// gridData. Existing controls in the parent are not removed.</remarks>
        /// <param name="parent">The parent control to which the generated grid will be added. Must not be null.</param>
        /// <param name="gridData">A two-dimensional list of strings representing the text for each cell in the grid. Each inner list
        /// corresponds to a row; cells beyond the length of a row are left empty.</param>
        internal static Control LabelGrid(string header, string footer, List<List<string>> gridData)
        {
            var container = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            var headerFooterFont = new Font("Arial", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            var headerFooterSize = 275f;
            var middleSize = 500f;

            // Define rows: Top/Bottom fit to content, Middle takes remaining space
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, headerFooterSize));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, middleSize));
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, headerFooterSize));

            // Top Row
            container.Controls.Add(new Label { Text = header, Font = headerFooterFont, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter }, 0, 0);

            // Middle Row: Add the 2D grid logic here (or call your method)
            var middleGrid = InnerLabelGrid(gridData);
            container.Controls.Add(middleGrid, 0, 1);

            // Bottom Row
            container.Controls.Add(new Label { Text = footer, Font = headerFooterFont, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter }, 0, 2);

            return container;
        }
        internal static Control InnerLabelGrid(List<List<string>> gridData)
        {
            int rows = gridData.Count;
            int cols = gridData.Max(r => r.Count);

            var grid = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                RowCount = rows,
                ColumnCount = cols
            };

            for (int i = 0; i < rows; i++)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
                // Determine font size based on row position
                float fontSize = (i == rows / 2) ? 64f : 18f;

                for (int j = 0; j < gridData[i].Count; j++)
                {
                    if (i == 0) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));

                    grid.Controls.Add(new Label
                    {
                        Text = gridData[i][j],
                        Font = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Point, 0),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill
                    }, j, i);
                }
            }
            return grid;
        }
        /// <summary>
        /// Centers the specified control within its parent container.
        /// </summary>
        /// <param name="controlToCenter">The control to be centered. Must have a parent container.</param>
        internal static void CenterControl(Control controlToCenter)
        {
            if (controlToCenter.Parent != null)
            {
                var containerWidth = controlToCenter.Parent.ClientSize.Width;
                var containerHeight = controlToCenter.Parent.ClientSize.Height;
                
                controlToCenter.Location = new Point(
                    (containerWidth / 2) - (controlToCenter.Width / 2),
                    (containerHeight / 2) - (controlToCenter.Height / 2)
                    );
                
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
        internal static void GenericLabel(Control parentControl, Label label, string text, Size size = default)
        {
            if (parentControl == null) return;

            parentControl.Controls.Add(label);

            SetUpControl
                (
                label,
                Color.Black,
                new Font("Arial", 24F, FontStyle.Bold, GraphicsUnit.Point),
                size,
                "GenericLabel"
                );

            label.Text = text;

            label.TextAlign = ContentAlignment.MiddleCenter;
        }
    }
}
