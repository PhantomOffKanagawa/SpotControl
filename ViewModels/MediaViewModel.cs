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
        public abstract string TrackName { get; set; }
        public abstract string ArtistName { get; set; }
        public abstract string AlbumImageUrl { get; set; }

        // Commands
        public abstract ICommand PreviousCommand { get; }
        public abstract ICommand PlayPauseCommand { get; }
        public abstract ICommand NextCommand { get; }

        // Progress
        public abstract double TrackProgress { get; set; }
        public abstract int TrackDurationMs { get; set; }
        public abstract ICommand SeekToCurrentPosition { get; }

        // Volume
        public abstract int Volume { get; set; }
        public abstract ICommand SetVolume { get; }

        // Shuffle / Repeat
        public abstract ICommand ToggleShuffleCommand { get; }
        public abstract ICommand CycleRepeatCommand { get; }
        public abstract string RepeatState { get; set; }
        public abstract bool IsShuffle { get; set; }

        // Helper for INotifyPropertyChanged
        public abstract Task UpdateTrackInfoAsync();
    }
}
