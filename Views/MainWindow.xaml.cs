using System.Windows;
using System.Windows.Input;
using SpotControl.Services;
using SpotControl.ViewModels;

namespace SpotControl.Views;
public partial class MainWindow : Window
{
    public MainWindow(SpotifyService spotifyService)
    {
        InitializeComponent();
        DataContext = new PlayerViewModel(spotifyService);
        Loaded += async (_, _) =>
        {
            await ((PlayerViewModel)DataContext).UpdateTrackInfoAsync();

        };



    }

    private void ProgressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PlayerViewModel vm)
        {
            vm.SeekToCurrentPosition.Execute(null);
        }
    }

    private void VolumeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PlayerViewModel vm)
        {
            vm.SetVolume.Execute(null);
        }
    }

    private void OnWindowclose(object sender, EventArgs e)
    {
        if (DataContext is PlayerViewModel vm)
        {
            vm.StopBackgroundPolling();
        }
    }
}
