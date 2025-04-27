using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SpotControl.Services;
using SpotControl.ViewModels;

namespace SpotControl.Views;
public partial class MainWindow : Window
{
    public MainWindow(SpotifyService spotifyService)
    {
        InitializeComponent();
        // Set the DataContext to the PlayerViewModel
        DataContext = new PlayerViewModel(spotifyService);
        Loaded += async (_, _) =>
        {
            // On Load Add Event Handlers
            await ((PlayerViewModel)DataContext).UpdateTrackInfoAsync();

        };
    }

    // Event Handlers
    // When Progress Slider Done Update Seek
    private void ProgressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PlayerViewModel vm)
        {
            vm.SeekToCurrentPosition.Execute(null);
        }
    }

    // When Volume Slider Done Update Volume
    private void VolumeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PlayerViewModel vm)
        {
            vm.SetVolume.Execute(null);
        }
    }

    // When Window is Closed Handle Thread Close
    private void OnWindowclose(object sender, EventArgs e)
    {
        if (DataContext is PlayerViewModel vm)
        {
            vm.StopBackgroundPolling();
        }
    }
}
