using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace Salah_Times
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Absolute path to your SQLite database file
            string databasePath = @"E:\Other\Coding\Salah Times\takvimi.sqlite";
            string connectionString = $"Data Source={databasePath};";

            Console.WriteLine($"Using database at: {databasePath}");

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    conn.Open();
                    Console.WriteLine("Connection to the database was successful.");

                    // Get today's date but change the year to 2016
                    DateTime today = DateTime.Now;
                    DateTime targetDate = new DateTime(2016, today.Month, today.Day);

                    // Format the date to match the database format (e.g., "dd-MM-yyyy")
                    string formattedDate = targetDate.ToString("dd-MM-yyyy");

                    // Query to fetch records for the specific date
                    string query = "SELECT * FROM kosova WHERE data = @date";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@date", formattedDate);

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    // Retrieve column values by column name
                                    DateTime fajrTime = DateTime.Parse(reader["imsaku"].ToString()).AddMinutes(18);
                                    DateTime dhuhrTime = DateTime.Parse(reader["dreka"].ToString());
                                    DateTime asrTime = DateTime.Parse(reader["ikindia"].ToString());
                                    DateTime maghribTime = DateTime.Parse(reader["akshami"].ToString());
                                    DateTime ishaTime = DateTime.Parse(reader["jacia"].ToString());

                                    // Convert times to string format if needed
                                    string updatedFajrTime = fajrTime.ToString("HH:mm");
                                    string dhuhrTimeString = dhuhrTime.ToString("HH:mm");
                                    string asrTimeString = asrTime.ToString("HH:mm");
                                    string maghribTimeString = maghribTime.ToString("HH:mm");
                                    string ishaTimeString = ishaTime.ToString("HH:mm");

                                    // Update text fields on the form using the public methods
                                    Home homeForm = new Home(fajrTime, dhuhrTime, asrTime, maghribTime, ishaTime);
                                    homeForm.SetClock1Text(updatedFajrTime);
                                    homeForm.SetClock2Text(dhuhrTimeString);
                                    homeForm.SetClock3Text(asrTimeString);
                                    homeForm.SetClock4Text(maghribTimeString);
                                    homeForm.SetClock5Text(ishaTimeString);

                                    // Extras
                                    string sunriseTime = reader["lindja_diellit"].ToString();
                                    DateTime duhaSDateTime = DateTime.Parse(sunriseTime).AddMinutes(15);
                                    DateTime duhaEDateTime = DateTime.Parse(dhuhrTimeString).AddMinutes(-10);
                                    string duhaSTime = duhaSDateTime.ToString("HH:mm");
                                    string duhaETime = duhaEDateTime.ToString("HH:mm");

                                    homeForm.SetClock6Text(sunriseTime);
                                    homeForm.SetClock7Text($"{duhaSTime} - {duhaETime}");

                                    // Parse the times from strings
                                    DateTime nightStart = DateTime.Parse(maghribTimeString);
                                    DateTime nightEnd = DateTime.Parse(updatedFajrTime);

                                    // If Fajr time is earlier in the day than Maghrib time, it means Fajr is on the next day
                                    if (nightEnd < nightStart)
                                    {
                                        nightEnd = nightEnd.AddDays(1);
                                    }

                                    // Calculate the duration between the two times
                                    TimeSpan nightDuration = nightEnd - nightStart;

                                    // Calculate one-third and two-thirds of the night duration
                                    TimeSpan oneThird = TimeSpan.FromTicks(nightDuration.Ticks / 3);
                                    TimeSpan twoThirds = TimeSpan.FromTicks(oneThird.Ticks * 2);

                                    // Calculate the times for one-third and two-thirds by adding to Maghrib time
                                    DateTime oneThirdTime = nightStart.Add(oneThird);
                                    DateTime twoThirdsTime = nightStart.Add(twoThirds);

                                    // Format the times as needed
                                    string oneThirdString = oneThirdTime.ToString("HH:mm");
                                    string twoThirdsString = twoThirdsTime.ToString("HH:mm");

                                    // Update the text fields
                                    homeForm.SetClock8Text(oneThirdString);
                                    homeForm.SetClock9Text(twoThirdsString);

                                    Application.Run(homeForm);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No data found for the given date.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }
    }
}
