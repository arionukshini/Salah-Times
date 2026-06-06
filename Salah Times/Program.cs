using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Salah_Times
{
    static class Program
    {
        private const string DatabaseFileName = "takvimi.sqlite";
        private static readonly string[] CityOrder =
        {
            "Prishtina",
            "Ferizaj",
            "Gjilani",
            "Presheva",
            "Podujeva",
            "Sharri",
            "Vushtrria"
        };

        private static readonly Dictionary<string, int> CityOffsets = new Dictionary<string, int>
        {
            { "Prishtina", -1 },
            { "Ferizaj", -1 },
            { "Gjilani", -1 },
            { "Presheva", -2 },
            { "Podujeva", -1 },
            { "Sharri", 2 },
            { "Vushtrria", -1 }
        };

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
                                    DateTime baseFajrTime = DateTime.Parse(reader["imsaku"].ToString()).AddMinutes(18);
                                    DateTime baseDhuhrTime = DateTime.Parse(reader["dreka"].ToString());
                                    DateTime baseAsrTime = DateTime.Parse(reader["ikindia"].ToString());
                                    DateTime baseMaghribTime = DateTime.Parse(reader["akshami"].ToString());
                                    DateTime baseIshaTime = DateTime.Parse(reader["jacia"].ToString());
                                    DateTime baseSunriseTime = DateTime.Parse(reader["lindja_diellit"].ToString());

                                    string selectedCity = Properties.Settings.Default.SelectedCity;
                                    if (!CityOffsets.ContainsKey(selectedCity))
                                    {
                                        selectedCity = "Prishtina";
                                    }

                                    DateTime initialFajrTime = ApplyCityOffset(baseFajrTime, selectedCity);
                                    DateTime initialDhuhrTime = ApplyCityOffset(baseDhuhrTime, selectedCity);
                                    DateTime initialAsrTime = ApplyCityOffset(baseAsrTime, selectedCity);
                                    DateTime initialMaghribTime = ApplyCityOffset(baseMaghribTime, selectedCity);
                                    DateTime initialIshaTime = ApplyCityOffset(baseIshaTime, selectedCity);

                                    Home homeForm = new Home(initialFajrTime, initialDhuhrTime, initialAsrTime, initialMaghribTime, initialIshaTime);
                                    homeForm.ConfigureCities(CityOrder, selectedCity);

                                    void UpdateTimesForCity(string city)
                                    {
                                        DateTime fajrTime = ApplyCityOffset(baseFajrTime, city);
                                        DateTime dhuhrTime = ApplyCityOffset(baseDhuhrTime, city);
                                        DateTime asrTime = ApplyCityOffset(baseAsrTime, city);
                                        DateTime maghribTime = ApplyCityOffset(baseMaghribTime, city);
                                        DateTime ishaTime = ApplyCityOffset(baseIshaTime, city);
                                        DateTime sunriseTime = ApplyCityOffset(baseSunriseTime, city);

                                        homeForm.UpdatePrayerTimes(fajrTime, dhuhrTime, asrTime, maghribTime, ishaTime);
                                        homeForm.SetClock1Text(fajrTime.ToString("HH:mm"));
                                        homeForm.SetClock2Text(dhuhrTime.ToString("HH:mm"));
                                        homeForm.SetClock3Text(asrTime.ToString("HH:mm"));
                                        homeForm.SetClock4Text(maghribTime.ToString("HH:mm"));
                                        homeForm.SetClock5Text(ishaTime.ToString("HH:mm"));

                                        DateTime duhaSDateTime = sunriseTime.AddMinutes(15);
                                        DateTime duhaEDateTime = dhuhrTime.AddMinutes(-10);
                                        homeForm.SetClock6Text(sunriseTime.ToString("HH:mm"));
                                        homeForm.SetClock7Text($"{duhaSDateTime:HH:mm} - {duhaEDateTime:HH:mm}");

                                        DateTime nightStart = maghribTime;
                                        DateTime nightEnd = fajrTime;

                                        if (nightEnd < nightStart)
                                        {
                                            nightEnd = nightEnd.AddDays(1);
                                        }

                                        TimeSpan nightDuration = nightEnd - nightStart;
                                        DateTime midnightTime = nightStart.Add(TimeSpan.FromTicks(nightDuration.Ticks / 2));
                                        TimeSpan oneThird = TimeSpan.FromTicks(nightDuration.Ticks / 3);
                                        TimeSpan twoThirds = TimeSpan.FromTicks(oneThird.Ticks * 2);
                                        DateTime oneThirdTime = nightStart.Add(oneThird);
                                        DateTime twoThirdsTime = nightStart.Add(twoThirds);

                                        homeForm.SetClock8Text(oneThirdTime.ToString("HH:mm"));
                                        homeForm.SetClock9Text(twoThirdsTime.ToString("HH:mm"));
                                        homeForm.SetClock10Text(midnightTime.ToString("HH:mm"));
                                    }

                                    homeForm.CityChanged += city =>
                                    {
                                        Properties.Settings.Default.SelectedCity = city;
                                        Properties.Settings.Default.Save();
                                        UpdateTimesForCity(city);
                                    };

                                    UpdateTimesForCity(selectedCity);
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

        private static DateTime ApplyCityOffset(DateTime time, string city)
        {
            return time.AddMinutes(CityOffsets[city]);
        }
    }
}
