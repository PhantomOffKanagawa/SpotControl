using System.IO;
using System.Text.Json;

namespace SpotControl.Helpers
{
    public class AppConfig
    {
        // Set Values in App Config
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";

        // Set Location of Config
        private static readonly string ConfigPath = "config.json";

        // Set Load Function
        public static AppConfig Load()
        {
            // If File Exists try to Parse it
            if (File.Exists(ConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();

                }
                catch (Exception e)
                {
                    // Handle the exception
                    Console.WriteLine($"Error loading config: {e.Message}");
                    return new AppConfig();
                }
            }

            return new AppConfig();
        }

        // Set Save Function
        public void Save()
        {
            // Write JSON to File
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
    }
}
