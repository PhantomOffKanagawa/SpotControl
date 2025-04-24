using System.Configuration;
using System.Data;
using System.Windows;

namespace SpotControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
        }

    }

}
