using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Salah_Times
{
    public partial class Home : Form
    {
        private static readonly Color BackgroundColor = Color.FromArgb(3, 10, 18);
        private static readonly Color PanelColor = Color.FromArgb(23, 36, 54);
        private static readonly Color PanelHoverColor = Color.FromArgb(30, 47, 68);
        private static readonly Color AccentBlue = Color.FromArgb(15, 135, 241);
        private static readonly Color SoftBlue = Color.FromArgb(104, 188, 255);
        private static readonly Color MainText = Color.White;
        private static readonly Color MutedText = Color.FromArgb(170, 186, 205);
        private const int FocusedContentWidth = 720;
        private const int FocusedContentHeight = 500;
        private const int MaxPageHeight = 330;
        private const int DesiredRowHeight = 52;
        private const int RowGap = 10;

        private Timer timer;
        private DateTime nextPrayerTime;
        private Label countdownLabel;
        private Panel tabSwitchPanel;
        private TabSwitchButton namaziTabButton;
        private TabSwitchButton extraTabButton;
        private Panel normalPage;
        private Panel extraPage;
        private Label time10;
        public Label clock10;

        // Constructor with prayer times passed in
        public Home(DateTime fajr, DateTime dhuhr, DateTime asr, DateTime maghrib, DateTime isha)
        {
            InitializeComponent();
            ApplyModernTheme();

            // Initialize the timer
            timer = new Timer();
            timer.Interval = 1000; // Set interval to 1 second (1000 ms)
            timer.Tick += Timer_Tick; // Attach the event handler
            timer.Start(); // Start the timer

            // Set the next prayer time initially
            SetNextPrayerTime(fajr, dhuhr, asr, maghrib, isha);
            Timer_Tick(this, EventArgs.Empty);
        }

        private void ApplyModernTheme()
        {
            DoubleBuffered = true;
            BackColor = BackgroundColor;
            ForeColor = MainText;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            ClientSize = new Size(920, 600);
            MinimumSize = Size;
            MaximumSize = Size;

            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.BackColor = Color.Transparent;
            logo.Size = new Size(52, 52);

            logoName.Font = new Font("Segoe UI Semibold", 28F, FontStyle.Bold, GraphicsUnit.Point);
            logoName.ForeColor = MainText;
            logoName.BackColor = Color.Transparent;
            logoName.TextAlign = ContentAlignment.MiddleLeft;
            logoName.AutoSize = false;

            countdownLabel = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                ForeColor = SoftBlue,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(countdownLabel);

            label1.Visible = false;
            timeDiffTxt.Font = new Font("Segoe UI", 8.7F, FontStyle.Regular, GraphicsUnit.Point);
            timeDiffTxt.ForeColor = MutedText;
            timeDiffTxt.BackColor = Color.Transparent;
            timeDiffTxt.AutoSize = false;
            timeDiffTxt.UseMnemonic = false;

            PrepareCustomTabs();

            normalPage = CreatePagePanel();
            extraPage = CreatePagePanel();
            Controls.Add(normalPage);
            Controls.Add(extraPage);

            time10 = new Label { Text = "Mesi i Nates" };
            clock10 = new Label { Text = "--:--" };

            CreatePrayerRows(
                normalPage,
                new[] { time1, time2, time3, time4, time5 },
                new[] { clock1, clock2, clock3, clock4, clock5 });

            CreatePrayerRows(
                extraPage,
                new[] { time6, time7, time10, time8, time9 },
                new[] { clock6, clock7, clock10, clock8, clock9 });

            Resize += (sender, args) => LayoutHome();
            ShowPage(normalPage);
            LayoutHome();
        }

        private void PrepareCustomTabs()
        {
            tabControl1.Controls.Remove(normalTab);
            tabControl1.Controls.Remove(tabPage2);
            Controls.Remove(tabControl1);

            tabSwitchPanel = new Panel
            {
                BackColor = BackgroundColor
            };

            namaziTabButton = CreateTabButton("Namazi");
            extraTabButton = CreateTabButton("Extra");

            namaziTabButton.Click += (sender, args) => ShowPage(normalPage);
            extraTabButton.Click += (sender, args) => ShowPage(extraPage);

            tabSwitchPanel.Controls.Add(namaziTabButton);
            tabSwitchPanel.Controls.Add(extraTabButton);
            Controls.Add(tabSwitchPanel);
        }

        private TabSwitchButton CreateTabButton(string text)
        {
            return new TabSwitchButton
            {
                Text = text,
                AccentColor = AccentBlue,
                BackColor = BackgroundColor,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point)
            };
        }

        private Panel CreatePagePanel()
        {
            return new Panel
            {
                BackColor = BackgroundColor,
                ForeColor = MainText,
                Padding = new Padding(14),
                Visible = false
            };
        }

        private void CreatePrayerRows(Panel page, Label[] names, Label[] values)
        {
            page.Controls.Clear();

            for (int i = 0; i < names.Length; i++)
            {
                RoundedPanel row = new RoundedPanel
                {
                    BackColor = Color.Transparent,
                    FillColor = PanelColor,
                    HoverColor = PanelHoverColor,
                    Radius = 14,
                    Tag = i
                };

                Label name = names[i];
                Label value = values[i];

                StyleNameLabel(name);
                StyleValueLabel(value);

                row.Controls.Add(name);
                row.Controls.Add(value);
                page.Controls.Add(row);

                row.Resize += (sender, args) => LayoutPrayerRow(row, name, value);
                row.MouseEnter += (sender, args) => row.IsHovered = true;
                row.MouseLeave += (sender, args) => row.IsHovered = false;
            }

            LayoutPrayerRows(page);
        }

        private void StyleNameLabel(Label label)
        {
            label.AutoSize = false;
            label.BackColor = Color.Transparent;
            label.ForeColor = MutedText;
            label.Font = new Font("Segoe UI Semibold", 12.5F, FontStyle.Bold, GraphicsUnit.Point);
            label.TextAlign = ContentAlignment.MiddleLeft;
        }

        private void StyleValueLabel(Label label)
        {
            label.AutoSize = false;
            label.BackColor = Color.Transparent;
            label.ForeColor = AccentBlue;
            label.Font = new Font("Segoe UI", 17F, FontStyle.Bold, GraphicsUnit.Point);
            label.TextAlign = ContentAlignment.MiddleRight;
        }

        private void LayoutHome()
        {
            int contentWidth = Math.Max(320, Math.Min(FocusedContentWidth, ClientSize.Width - 80));
            int contentHeight = Math.Max(430, Math.Min(FocusedContentHeight, ClientSize.Height - 54));
            int left = (ClientSize.Width - contentWidth) / 2;
            int top = Math.Max(22, (ClientSize.Height - contentHeight) / 2);

            int headerWidth = Math.Min(390, contentWidth);
            int headerLeft = left + (contentWidth - headerWidth) / 2;

            logo.Location = new Point(headerLeft, top + 4);
            logoName.Location = new Point(headerLeft + 66, top - 1);
            logoName.Size = new Size(headerWidth - 66, 44);

            countdownLabel.Location = new Point(left, top + 58);
            countdownLabel.Size = new Size(contentWidth, 28);

            int switchWidth = Math.Min(306, contentWidth);
            tabSwitchPanel.Location = new Point(left + (contentWidth - switchWidth) / 2, top + 104);
            tabSwitchPanel.Size = new Size(switchWidth, 40);
            LayoutTabSwitcher();

            int pageTop = tabSwitchPanel.Bottom + 14;
            int footerHeight = 44;
            int footerTop = top + contentHeight - footerHeight;
            int availablePageHeight = Math.Max(248, footerTop - pageTop - 14);
            int pageHeight = Math.Min(MaxPageHeight, availablePageHeight);
            Rectangle pageBounds = new Rectangle(left, pageTop, contentWidth, pageHeight);
            normalPage.Bounds = pageBounds;
            extraPage.Bounds = pageBounds;

            timeDiffTxt.Location = new Point(left, footerTop);
            timeDiffTxt.Size = new Size(contentWidth, footerHeight);

            LayoutPrayerRows(normalPage);
            LayoutPrayerRows(extraPage);
            Invalidate();
        }

        private void LayoutTabSwitcher()
        {
            if (tabSwitchPanel == null)
            {
                return;
            }

            int gap = 10;
            int buttonWidth = (tabSwitchPanel.Width - gap) / 2;
            namaziTabButton.Bounds = new Rectangle(0, 0, buttonWidth, tabSwitchPanel.Height);
            extraTabButton.Bounds = new Rectangle(buttonWidth + gap, 0, buttonWidth, tabSwitchPanel.Height);
        }

        private void LayoutPrayerRows(Panel page)
        {
            if (page.Controls.Count == 0)
            {
                return;
            }

            int gap = RowGap;
            int usableHeight = page.ClientSize.Height - page.Padding.Vertical;
            int maxRowHeight = (usableHeight - gap * (page.Controls.Count - 1)) / page.Controls.Count;
            int rowHeight = Math.Max(42, Math.Min(DesiredRowHeight, maxRowHeight));
            int totalRowsHeight = rowHeight * page.Controls.Count + gap * (page.Controls.Count - 1);
            int y = page.Padding.Top + Math.Max(0, (usableHeight - totalRowsHeight) / 2);

            foreach (Control control in page.Controls)
            {
                control.Location = new Point(page.Padding.Left, y);
                control.Size = new Size(page.ClientSize.Width - page.Padding.Horizontal, rowHeight);
                y += rowHeight + gap;
            }
        }

        private void LayoutPrayerRow(Control row, Label name, Label value)
        {
            int padding = 22;
            int valueWidth = Math.Max(160, row.Width / 3);

            name.Location = new Point(padding, 0);
            name.Size = new Size(row.Width - valueWidth - padding * 2, row.Height);

            value.Location = new Point(row.Width - valueWidth - padding, 0);
            value.Size = new Size(valueWidth, row.Height);
        }

        private void ShowPage(Panel selectedPage)
        {
            bool showNormal = selectedPage == normalPage;

            normalPage.Visible = showNormal;
            extraPage.Visible = !showNormal;
            selectedPage.BringToFront();
            tabSwitchPanel.BringToFront();

            if (namaziTabButton != null && extraTabButton != null)
            {
                namaziTabButton.IsSelected = showNormal;
                extraTabButton.IsSelected = !showNormal;
            }

            LayoutPrayerRows(selectedPage);
        }

        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabs = (TabControl)sender;
            Rectangle bounds = e.Bounds;
            bool selected = tabs.SelectedIndex == e.Index;

            using (SolidBrush brush = new SolidBrush(selected ? AccentBlue : Color.FromArgb(18, 27, 41)))
            using (GraphicsPath path = RoundedPanel.GetRoundedPath(Rectangle.Inflate(bounds, -4, -5), 10))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            }

            TextRenderer.DrawText(
                e.Graphics,
                tabs.TabPages[e.Index].Text,
                tabs.Font,
                bounds,
                selected ? Color.White : MutedText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush brush = new SolidBrush(BackgroundColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            using (Pen accentPen = new Pen(Color.FromArgb(90, AccentBlue), 2))
            {
                e.Graphics.DrawLine(accentPen, 0, 0, ClientSize.Width, 0);
            }

            base.OnPaint(e);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Calculate time remaining until the next prayer
            TimeSpan timeUntilNextPrayer = nextPrayerTime - DateTime.Now;

            if (timeUntilNextPrayer < TimeSpan.Zero)
            {
                // If the next prayer time is in the past, set the next prayer time to the next day's Fajr
                // You may need to update this logic based on your requirements
                nextPrayerTime = nextPrayerTime.AddDays(1);
                timeUntilNextPrayer = nextPrayerTime - DateTime.Now;
            }

            string remaining = timeUntilNextPrayer.ToString(@"hh\:mm\:ss");
            Text = $"Koha e ardhshme edhe: {remaining}";

            if (countdownLabel != null)
            {
                countdownLabel.Text = $"Koha e ardhshme edhe: {remaining}";
            }
        }

        // Public methods to set the text of the clock labels
        public void SetClock1Text(string text)
        {
            clock1.Text = text;  // Ensure clock1 is a label or control with a Text property
        }

        public void SetClock2Text(string text)
        {
            clock2.Text = text;
        }

        public void SetClock3Text(string text)
        {
            clock3.Text = text;
        }

        public void SetClock4Text(string text)
        {
            clock4.Text = text;
        }

        public void SetClock5Text(string text)
        {
            clock5.Text = text;
        }

        public void SetClock6Text(string text)
        {
            clock6.Text = text;
        }

        public void SetClock7Text(string text)
        {
            clock7.Text = text;
        }

        public void SetClock8Text(string text)
        {
            clock8.Text = text;
        }

        public void SetClock9Text(string text)
        {
            clock9.Text = text;
        }

        public void SetClock10Text(string text)
        {
            clock10.Text = text;
        }

        // Method to set the next prayer time
        private void SetNextPrayerTime(DateTime fajr, DateTime dhuhr, DateTime asr, DateTime maghrib, DateTime isha)
        {
            DateTime now = DateTime.Now;

            // Determine the next prayer time
            if (now < fajr)
                nextPrayerTime = fajr;
            else if (now < dhuhr)
                nextPrayerTime = dhuhr;
            else if (now < asr)
                nextPrayerTime = asr;
            else if (now < maghrib)
                nextPrayerTime = maghrib;
            else if (now < isha)
                nextPrayerTime = isha;
            else
                nextPrayerTime = fajr.AddDays(1); // Set to the next day's Fajr if past Isha
        }
    }

    internal class RoundedPanel : Panel
    {
        public Color FillColor { get; set; }
        public Color HoverColor { get; set; }
        public int Radius { get; set; }

        private bool isHovered;

        public bool IsHovered
        {
            get { return isHovered; }
            set
            {
                isHovered = value;
                Invalidate();
            }
        }

        public RoundedPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            FillColor = Color.FromArgb(24, 34, 48);
            HoverColor = Color.FromArgb(30, 47, 68);
            Radius = 12;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = GetRoundedPath(bounds, Radius))
            using (SolidBrush brush = new SolidBrush(IsHovered ? HoverColor : FillColor))
            using (Pen border = new Pen(Color.FromArgb(45, 104, 188, 255)))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);
            }
        }

        internal static GraphicsPath GetRoundedPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return path;
            }

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    internal class TabSwitchButton : Button
    {
        private bool isHovered;
        private bool isSelected;

        public Color AccentColor { get; set; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                Invalidate();
            }
        }

        public TabSwitchButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            AccentColor = Color.FromArgb(15, 135, 241);
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            ForeColor = Color.White;
            TabStop = false;
            UseVisualStyleBackColor = false;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(BackColor);

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fill = IsSelected
                ? AccentColor
                : (isHovered ? Color.FromArgb(31, 44, 62) : Color.FromArgb(17, 26, 40));
            Color borderColor = IsSelected
                ? Color.FromArgb(180, 104, 188, 255)
                : Color.FromArgb(70, 104, 188, 255);

            using (GraphicsPath path = RoundedPanel.GetRoundedPath(bounds, 14))
            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen border = new Pen(borderColor))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);
            }

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                bounds,
                IsSelected ? Color.White : Color.FromArgb(190, 207, 226),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }
    }
}
