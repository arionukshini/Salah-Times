using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace Salah_Times
{
    static class Program
    {
        private const string DatabaseFileName = "takvimi.sqlite";

        [STAThread]
        static void Main()
        {
            try
            {
                string databasePath = EnsureDatabaseExists();
                string connectionString = $"Data Source={databasePath};Version=3;";

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

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
                                    DateTime midnightTime = nightStart.Add(TimeSpan.FromTicks(nightDuration.Ticks / 2));

                                    // Calculate one-third and two-thirds of the night duration
                                    TimeSpan oneThird = TimeSpan.FromTicks(nightDuration.Ticks / 3);
                                    TimeSpan twoThirds = TimeSpan.FromTicks(oneThird.Ticks * 2);

                                    // Calculate the times for one-third and two-thirds by adding to Maghrib time
                                    DateTime oneThirdTime = nightStart.Add(oneThird);
                                    DateTime twoThirdsTime = nightStart.Add(twoThirds);

                                    // Format the times as needed
                                    string oneThirdString = oneThirdTime.ToString("HH:mm");
                                    string twoThirdsString = twoThirdsTime.ToString("HH:mm");
                                    string midnightString = midnightTime.ToString("HH:mm");

                                    // Update the text fields
                                    homeForm.SetClock8Text(oneThirdString);
                                    homeForm.SetClock9Text(twoThirdsString);
                                    homeForm.SetClock10Text(midnightString);

                                    Application.Run(homeForm);
                                }
                            }
                            else
                            {
                                MessageBox.Show(
                                    "No prayer times were found for today's date.",
                                    "Salah Times",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load the prayer times database.\n\n" + ex.Message,
                    "Salah Times",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string EnsureDatabaseExists()
        {
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Salah Times");
            string appDataDatabasePath = Path.Combine(appDataFolder, DatabaseFileName);

            if (File.Exists(appDataDatabasePath))
            {
                return appDataDatabasePath;
            }

            Directory.CreateDirectory(appDataFolder);

            string sourceDatabasePath = FindBundledDatabase();
            if (sourceDatabasePath == null)
            {
                throw new FileNotFoundException(
                    $"Could not find {DatabaseFileName}. Make sure it is included next to the program when building the exe.");
            }

            File.Copy(sourceDatabasePath, appDataDatabasePath);
            return appDataDatabasePath;
        }

        private static string FindBundledDatabase()
        {
            string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
            string[] possiblePaths =
            {
                Path.Combine(baseFolder, DatabaseFileName),
                Path.Combine(baseFolder, "..", DatabaseFileName),
                Path.Combine(baseFolder, "..", "..", DatabaseFileName),
                Path.Combine(baseFolder, "..", "..", "..", DatabaseFileName)
            };

            foreach (string possiblePath in possiblePaths)
            {
                string fullPath = Path.GetFullPath(possiblePath);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }
    }
}
