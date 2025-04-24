using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SpotControl.ViewModels
{
    public abstract class MediaViewModel : BaseViewModel
    {
        // Properties
        public string TrackName;
        public string ArtistName;
        public string AlbumImageURL;

        // Commands
        public ICommand PreviousCommand;
        public ICommand PlayPauseCommand;
        public ICommand NextCommand;

        // Volume
        public int Volume;
        public ICommand SetVolume;

        // Track Progress
        public double TrackProgress;
        public int TrackDurationMs;
        public ICommand SeekToCurrentPosition;

        // Shuffle & Repeat
        public ICommand ToggleShuffleCommand;
        public bool IsShuffle;
        public ICommand CycleRepeatCommand;
        public string RepeatIcon;

        // Update Method
        public abstract Task UpdateTrackInfoAsync();
    }
}
