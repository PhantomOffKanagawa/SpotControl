// ViewModels/PlayerViewModel.cs
using GalaSoft.MvvmLight.CommandWpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SpotControl.Services;
using SpotifyAPI.Web;

namespace SpotControl.ViewModels
{
    public class PlayerViewModel : MediaViewModel
    {
        private const int GENERAL_UPDATE_MS = 3000;
        private const int UPDATE_SEEK_MS = 100;
        private readonly SpotifyService _spotifyService;

        public PlayerViewModel(SpotifyService spotifyService)
        {
            // Initialize service
            _spotifyService = spotifyService;

            // Start threaded polling
            StartBackgroundPolling();
        }

        /* Handle Track Name and Artist Name */
        public string TrackName { get; set; }
        public string ArtistName { get; set; }

        /* Handle Album Art */
        private string _albumImageUrl;
        public string AlbumImageUrl { get => _albumImageUrl; set { _albumImageUrl = value; OnPropertyChanged(); } }

        /* Handle Next and Previous */
        public ICommand PreviousCommand => new RelayCommand(async () => await _spotifyService.PreviousTrackAsync());
        public ICommand PlayPauseCommand => new RelayCommand(async () => await _spotifyService.PlayPauseAsync());
        public ICommand NextCommand => new RelayCommand(async () => await _spotifyService.NextTrackAsync());

        /* Track Playing */
        public bool Playing { get; set; }

        /* Handle Volume */
        public int Volume { get; set; } = 50; // Bind slider
        public ICommand SetVolume => new RelayCommand(async () => await _spotifyService.SetVolume(Volume));

        /* Handle Track Progress */
        public double TrackProgress { get; set; } = 0; // Bind progress bar
        public int TrackDurationMs = 0;
        public ICommand SeekToCurrentPosition => new RelayCommand(async () => await _spotifyService.SeekAsync((int)(TrackProgress / 100 * TrackDurationMs)));

        // Make the slider move even when not actively updated
        // This is looped in a thread at the bottom
        private void smoothMove()
        {
            if (TrackDurationMs != 0 && Playing)
            {

                TrackProgress += ((float)UPDATE_SEEK_MS / (float)TrackDurationMs) * 100.0;
                if (TrackProgress > 100)
                {
                    TrackProgress = 100;
                }

                OnPropertyChanged(nameof(TrackProgress));
            }
        }

        /* Add Functionality of Shuffle/Repeat */
        public ICommand ToggleShuffleCommand => new RelayCommand(async () =>
        {
            bool newShuffle = !IsShuffle;
            IsShuffle = newShuffle;
            await _spotifyService.SetShuffleAsync(newShuffle);
        });

        private bool _isShuffle;
        public bool IsShuffle
        {
            get => _isShuffle;
            set { _isShuffle = value; OnPropertyChanged(); }
        }

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

        /* Handle Updates */
        public override async Task UpdateTrackInfoAsync()
        {
            var track = await _spotifyService.GetCurrentTrackAsync();
            TrackName = track?.Name ?? "No track playing";
            ArtistName = string.Join(", ", track?.Artists.Select(a => a.Name) ?? Enumerable.Empty<string>());
            OnPropertyChanged(nameof(TrackName));
            OnPropertyChanged(nameof(ArtistName));

            var playback = await _spotifyService.GetPlaybackInfoAsync();
            if (playback?.Item is FullTrack fullTrack)
            {
                TrackDurationMs = fullTrack.DurationMs;
            }
            else
            {
                TrackDurationMs = 0;
            }

            TrackProgress = (playback?.ProgressMs ?? 0) * 100.0 / TrackDurationMs;
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

        // Spin off Updates into Polling Thread

        private CancellationTokenSource _ctsUpdates;
        private CancellationTokenSource _ctsSeeking;

        public void StartBackgroundPolling()
        {
            _ctsUpdates = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_ctsUpdates.Token.IsCancellationRequested)
                {
                    await UpdateTrackInfoAsync();
                    await Task.Delay(GENERAL_UPDATE_MS, _ctsUpdates.Token);
                }
            }, _ctsUpdates.Token);

            _ctsSeeking = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_ctsSeeking.Token.IsCancellationRequested)
                {
                    smoothMove();
                    await Task.Delay(UPDATE_SEEK_MS, _ctsSeeking.Token);
                }
            }, _ctsSeeking.Token);
        }

        public void StopBackgroundPolling()
        {
            _ctsUpdates?.Cancel();
            _ctsSeeking?.Cancel();
        }
    }

}