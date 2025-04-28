// ViewModels/PlayerViewModel.cs
using GalaSoft.MvvmLight.CommandWpf;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SpotControl.Services;
using SpotifyAPI.Web;
using Swan;

namespace SpotControl.ViewModels
{
    public class PlayerViewModel : MediaViewModel
    {
        private const int GENERAL_UPDATE_MS = 3000;
        private const int UPDATE_SEEK_MS = 100;
        private readonly SpotifyService _spotifyService;

        public PlayerViewModel(SpotifyService spotifyService)
        {
            // Set defaults
            TrackName = "";
            ArtistName = "";

            // Initialize service
            _spotifyService = spotifyService;

            // Start threaded polling
            StartBackgroundPolling();
        }

        /* Handle Track Name and Artist Name */
        public override string TrackName { get; set; }
        public override string ArtistName { get; set; }

        /* Handle Album Art */
        private string _albumImageUrl = "";
        public override string AlbumImageUrl
        {
            get => _albumImageUrl;
            set { _albumImageUrl = value; OnPropertyChanged(); }
        }

        /* Handle Next and Previous */
        public override ICommand PreviousCommand => new RelayCommand(async () => await _spotifyService.PreviousTrackAsync());
        public override ICommand PlayPauseCommand => new RelayCommand(async () =>
        {
            await _spotifyService.PlayPauseAsync();
            Playing = !Playing;
        });

        public override ICommand NextCommand => new RelayCommand(async () => await _spotifyService.NextTrackAsync());

        /* Track Playing */
        private bool _playing = false;
        public bool Playing
        {
            get { return _playing; }
            set { _playing = value; OnPropertyChanged(nameof(PlayPauseIcon)); }
        }
        public string PlayPauseIcon => Playing ? "⏸" : "⏯";

        /* Handle Volume */
        private int _volume = 50;
        public override int Volume
        {
            get => _volume;
            set { _volume = value; OnPropertyChanged(); }
        }
        public override ICommand SetVolume => new RelayCommand(async () => await _spotifyService.SetVolume(Volume));

        /* Handle Track Progress */
        private double _trackProgress = 0;
        public override double TrackProgress
        {
            get => _trackProgress;
            set { _trackProgress = value; OnPropertyChanged(); }
        }
        public override int TrackDurationMs { get; set; }
        public override ICommand SeekToCurrentPosition => new RelayCommand(async () => await _spotifyService.SeekAsync((int)(TrackProgress / 100 * TrackDurationMs)));

        // Make the slider move even when not actively updated
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
        public override ICommand ToggleShuffleCommand => new RelayCommand(async () =>
        {
            bool newShuffle = !IsShuffle;
            IsShuffle = newShuffle;
            await _spotifyService.SetShuffleAsync(newShuffle);
        });

        private bool _isShuffle;
        public override bool IsShuffle
        {
            get => _isShuffle;
            set { _isShuffle = value; OnPropertyChanged(); }
        }

        public override ICommand CycleRepeatCommand => new RelayCommand(async () =>
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
        public override string RepeatState
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
            var playback = await _spotifyService.GetPlaybackInfoAsync();

            if (playback?.Item is FullTrack fullTrack)
            {
                TrackName = fullTrack.Name;
                ArtistName = string.Join(", ", fullTrack.Artists.Select(a => a.Name));
                TrackDurationMs = fullTrack.DurationMs;
                AlbumImageUrl = fullTrack.Album?.Images?.FirstOrDefault()?.Url ?? "";
            }
            else
            {
                TrackName = "No track playing";
                ArtistName = "";
                TrackDurationMs = 0;
                AlbumImageUrl = "";
            }

            OnPropertyChanged(nameof(TrackName));
            OnPropertyChanged(nameof(ArtistName));
            OnPropertyChanged(nameof(AlbumImageUrl));

            TrackProgress = (playback?.ProgressMs ?? 0) * 100.0 / TrackDurationMs;
            OnPropertyChanged(nameof(TrackProgress));

            Playing = playback?.IsPlaying ?? false;
            OnPropertyChanged(nameof(Playing));
            OnPropertyChanged(nameof(PlayPauseIcon));

            Volume = playback?.Device.VolumePercent ?? 0;
            OnPropertyChanged(nameof(Volume));

            IsShuffle = playback?.ShuffleState ?? false;
            RepeatState = playback?.RepeatState ?? "off";
            OnPropertyChanged(nameof(IsShuffle));
            OnPropertyChanged(nameof(RepeatState));
        }

        private CancellationTokenSource? _ctsUpdates;
        private CancellationTokenSource? _ctsSeeking;

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