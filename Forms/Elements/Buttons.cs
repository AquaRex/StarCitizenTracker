using System.Drawing;
using System.Windows.Forms;

namespace StarCitizenTracker.Elements
{
    public class Buttons : Button
    {
        public Buttons()
        {
            Text = "Lock";
            Name = "lockButton";
            Size = new Size(40, 22);
            Anchor = AnchorStyles.Top | AnchorStyles.Right;
            FlatStyle = FlatStyle.Flat;
            BackColor = Color.FromArgb(80, 80, 80);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            FlatAppearance.BorderSize = 0;
        }
    }

    public class CloseButton : Button
    {
        public CloseButton()
        {
            Text = "X";
            Name = "closeButton";
            Size = new Size(22, 22);
            Anchor = AnchorStyles.Top | AnchorStyles.Right;
            FlatStyle = FlatStyle.Flat;
            BackColor = Color.FromArgb(80, 80, 80);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            FlatAppearance.BorderSize = 0;
        }
    }
}
