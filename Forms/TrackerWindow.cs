using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using StarCitizenTracker.Elements;

namespace StarCitizenTracker.Forms
{
    public class TrackerWindow : Form
    {
        private bool isLocked = false;
        private Button lockButton;
        private Button closeButton;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public TrackerWindow()
        {
            InitializeWindow();
            AddButtons();
        }

        //------------------------ INITIALIZE WINDOW PARAMETERS ------------------------

        private void InitializeWindow()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.Black;
            this.Opacity = 0.75;
            this.StartPosition = FormStartPosition.Manual;
            this.MouseDown += OnMouseDown;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        //------------------------ ADD WINDOW BUTTONS ------------------------

        private void AddButtons()
        {
            lockButton = new Buttons();
            lockButton.Location = new Point(this.Width - 60, 5);
            lockButton.Click += LockButton_Click;
            this.Controls.Add(lockButton);

            closeButton = new CloseButton();
            closeButton.Location = new Point(lockButton.Location.X - closeButton.Width - 5, 5);
            closeButton.Click += CloseButton_Click;
            this.Controls.Add(closeButton);
        }

        //------------------------ BUTTON EVENTS ------------------------

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        protected virtual void LockButton_Click(object sender, EventArgs e)
        {
            isLocked = true;
            closeButton.Hide();
            this.Opacity = 1.0;
            this.TransparencyKey = Color.Black;
            this.Controls.Remove(lockButton);
            lockButton.Dispose();
            ClearWIndow();
            Invalidate();
        }

        //------------------------ MOVE WINDOW ------------------------

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (!isLocked && e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        //------------------------ CLEAR WINDOW ------------------------

        protected virtual void ClearWIndow()
        {
            // Override from subclasses
            Invalidate();
        }

        //------------------------ DISPOSE ------------------------

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lockButton?.Dispose();
                closeButton?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}