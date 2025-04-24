// ViewModels/PlayerViewModel.cs
using GalaSoft.MvvmLight.CommandWpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SpotControl.Services;
using SpotifyAPI.Web;

namespace SpotControl.ViewModels
{
    public class PlayerViewModel : INotifyPropertyChanged
    {
        private Timer _updateTimer;
        private Timer _seekTimer;
        private const int GENERAL_UPDATE_MS = 3000;
        private const int UPDATE_SEEK_MS = 50;
        private readonly SpotifyService _spotifyService;

        public PlayerViewModel(SpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
            TrackName = string.Empty; // Initialize to avoid nullability warnings  
            ArtistName = string.Empty; // Initialize to avoid nullability warnings  

            // General attribute update
            _updateTimer = new Timer(async _ => await UpdateTrackInfoAsync(), null, 0, GENERAL_UPDATE_MS); // 3s

            // Smooth Track Progress
            _seekTimer = new Timer(_ => smoothMove(), null, 0, UPDATE_SEEK_MS); // 100ms
        }

        /* Handle Track Name and Artist Name */
        public string TrackName { get; set; }
        public string ArtistName { get; set; }

        /* Handle Next and Previous */
        public ICommand NextCommand => new RelayCommand(async () => await _spotifyService.NextTrackAsync());
        public ICommand PreviousCommand => new RelayCommand(async () => await _spotifyService.PreviousTrackAsync());

        /* Handle Play/Pause */
        public ICommand PlayPauseCommand => new RelayCommand(async () => await _spotifyService.PlayPauseAsync());
        public bool Playing { get; set; }

        /* Handle Volume */
        public int Volume { get; set; } = 50; // Bind slider

        public async void SetVolume()
        {
            await _spotifyService.SetVolume(Volume);
        }

        /* Handle Track Progress */
        public double TrackProgress { get; set; } = 0; // Bind progress bar


        private int _trackDurationMs = 0;

        public async void SeekToCurrentPosition()
        {
            await _spotifyService.SeekAsync((int)(TrackProgress / 100 * _trackDurationMs));
        }

        // Make 
        private void smoothMove()
        {
            if (_trackDurationMs != 0 && Playing)
            {

                TrackProgress += ((float)UPDATE_SEEK_MS / (float)_trackDurationMs) * 100.0;
                if (TrackProgress > 100)
                {
                    TrackProgress = 100;
                }

                OnPropertyChanged(nameof(TrackProgress));
            }
        }
           

        /* Handle Album Art */
        private string _albumImageUrl;
        public string AlbumImageUrl
        {
            get => _albumImageUrl;
            set { _albumImageUrl = value; OnPropertyChanged(); }
        }

        /* Add Functionality of Shuffle/Repeat */
        public ICommand ToggleShuffleCommand => new RelayCommand(async () =>
        {
            bool newShuffle = !IsShuffle;
            IsShuffle = newShuffle;
            await _spotifyService.SetShuffleAsync(newShuffle);
        });


        public ICommand CycleRepeatCommand => new RelayCommand(async () =>
        {
            string newMode = RepeatState switch
            {
                "off" => "context",
                "context" => "track",
                _ => "off"
            };

            RepeatState = newMode;
            await _spotifyService.SetRepeatAsync(newMode);
        });

        public string RepeatIcon => RepeatState switch
        {
            "track" => "🔂",
            "context" => "🔁",
            _ => "➖"
        };


        private string _repeatState = "off";
        public string RepeatState
        {
            get => _repeatState;
            set
            {
                _repeatState = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RepeatIcon));
            }
        }

        private bool _isShuffle;
        public bool IsShuffle
        {
            get => _isShuffle;
            set { _isShuffle = value; OnPropertyChanged(); }
        }

        /* Handle Updates */
        public async Task UpdateTrackInfoAsync()
        {
            var track = await _spotifyService.GetCurrentTrackAsync();
            TrackName = track?.Name ?? "No track playing";
            ArtistName = string.Join(", ", track?.Artists.Select(a => a.Name) ?? Enumerable.Empty<string>());
            OnPropertyChanged(nameof(TrackName));
            OnPropertyChanged(nameof(ArtistName));

            var playback = await _spotifyService.GetPlaybackInfoAsync();
            if (playback?.Item is FullTrack fullTrack)
            {
                _trackDurationMs = fullTrack.DurationMs;
            }
            else
            {
                _trackDurationMs = 0;
            }

            TrackProgress = (playback?.ProgressMs ?? 0) * 100.0 / _trackDurationMs;
            OnPropertyChanged(nameof(TrackProgress));

            Playing = playback?.IsPlaying ?? false;
            OnPropertyChanged(nameof(Playing));

            Volume = playback?.Device.VolumePercent ?? 0;
            OnPropertyChanged(nameof(Volume));

            AlbumImageUrl = track?.Album?.Images?.FirstOrDefault()?.Url ?? "";
            OnPropertyChanged(nameof(AlbumImageUrl));

            IsShuffle = playback?.ShuffleState ?? false;
            RepeatState = playback?.RepeatState ?? "off";
            OnPropertyChanged(nameof(IsShuffle));
            OnPropertyChanged(nameof(RepeatState));
        }


        public event PropertyChangedEventHandler? PropertyChanged; // Marked nullable to avoid warning  
        private void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}