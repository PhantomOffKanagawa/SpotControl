using System.Windows;
using SpotControl.Services;

namespace SpotControl.Views
{
    public partial class LoginWindow : Window
    {
        private readonly SpotifyService _spotifyService;

        public LoginWindow()
        {
            InitializeComponent();
            _spotifyService = new SpotifyService();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = await _spotifyService.AuthenticateAsync();

                if (success)
                {
                    var mainWindow = new MainWindow(_spotifyService);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Authentication failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

    }
}
