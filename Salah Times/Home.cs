using System.Drawing;
using System.Windows.Forms;
using System;

namespace Salah_Times
{
    public partial class Home : Form
    {
        private Timer timer;
        private DateTime nextPrayerTime;

        // Constructor with prayer times passed in
        public Home(DateTime fajr, DateTime dhuhr, DateTime asr, DateTime maghrib, DateTime isha)
        {
            InitializeComponent();

            // Initialize the timer
            timer = new Timer();
            timer.Interval = 1000; // Set interval to 1 second (1000 ms)
            timer.Tick += Timer_Tick; // Attach the event handler
            timer.Start(); // Start the timer

            // Set the next prayer time initially
            SetNextPrayerTime(fajr, dhuhr, asr, maghrib, isha);
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

            // Update the window title (text) with the time remaining until the next prayer, including seconds
            this.Text = $"Koha e ardhshme edhe: {timeUntilNextPrayer.ToString(@"hh\:mm\:ss")}";
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
}
