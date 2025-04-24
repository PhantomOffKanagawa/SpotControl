using System.Windows;
using SpotControl.Helpers;
using SpotControl.Services;

namespace SpotControl.Views
{
    public partial class LoginWindow : Window
    {
        // Handle Login
        // Keep Client ID and Secret in AppConfig
        private AppConfig _config;

        public LoginWindow()
        {
            InitializeComponent();
            _config = AppConfig.Load();

            // Load Client ID and Secret from AppConfig
            ClientIdBox.Text = _config.ClientId;
            ClientSecretBox.Password = _config.ClientSecret;

            // Set Remember Me CheckBox
            RememberCheck.IsChecked = !string.IsNullOrWhiteSpace(_config.ClientId);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if Client ID and Secret are empty
            if (string.IsNullOrWhiteSpace(ClientIdBox.Text) || string.IsNullOrWhiteSpace(ClientSecretBox.Password))
            {
                MessageBox.Show("Please enter both Client ID and Client Secret.");
                return;
            }

            // Save Client ID and Secret to AppConfig
            _config.ClientId = ClientIdBox.Text.Trim();
            _config.ClientSecret = ClientSecretBox.Password.Trim();

            // Save AppConfig if Remember Me is checked
            if (RememberCheck.IsChecked == true)
            {
                _config.Save();
            }

            try
            {
                // Authenticate with Spotify
                var spotifyService = new SpotifyService(_config.ClientId, _config.ClientSecret);
                bool success = await spotifyService.AuthenticateAsync();

                // Check if authentication was successful
                if (success)
                {
                    var mainWindow = new MainWindow(spotifyService);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Authentication failed. Please check your credentials or try again.");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during authentication
                MessageBox.Show($"An error occurred during authentication: {ex.Message}");
            }
        }
    }
}
