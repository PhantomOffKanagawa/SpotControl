using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;

namespace SpotControl.Services
{
    /// <summary>
    /// Service for handling Spotify authentication and playback control.
    /// </summary>
    public class SpotifyService : MediaController
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private SpotifyClient? _spotify;

        // Helper variable
        public bool IsAuthenticated => _spotify != null;

        public SpotifyService(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        // Handle Authentication
        public async Task<bool> AuthenticateAsync()
        {
            // Get the config
            var config = SpotifyClientConfig.CreateDefault();

            // Send the login request
            var request = new LoginRequest(
                new Uri("http://localhost:5000/callback"),
                _clientId,
                LoginRequest.ResponseType.Code
            )
            {
                // Define scope to include needed permissions
                Scope = new[]
                {
                Scopes.UserReadPlaybackState,
                Scopes.UserModifyPlaybackState,
                Scopes.UserReadCurrentlyPlaying
            }
            };

            // Start the server to listen for the callback
            var server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            var tcs = new TaskCompletionSource<bool>();
            await server.Start();

            // Handle the authorization code received
            server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await server.Stop();

                try
                {
                    // Exchange the authorization code for an access token
                    var token = await new OAuthClient().RequestToken(
                        new AuthorizationCodeTokenRequest(
                            _clientId,
                            _clientSecret,
                            response.Code,
                            new Uri("http://localhost:5000/callback"))
                    );

                    // Create a new Spotify client with the access token
                    _spotify = new SpotifyClient(config.WithToken(token.AccessToken));
                    tcs.SetResult(true);
                }
                catch
                {
                    tcs.SetResult(false);
                }
            };

            // Open the login URL in the default browser
            BrowserUtil.Open(request.ToUri());
            return await tcs.Task;
        }

        // Get General Current Playing Context
        public override async Task<CurrentlyPlayingContext?> GetPlaybackInfoAsync()
    => await _spotify?.Player.GetCurrentPlayback();

        // Handle Next, Play/Pause, and Previous Track
        public override async Task NextTrackAsync() => await _spotify?.Player.SkipNext()!;
        public override async Task PlayPauseAsync()
        {
            if (_spotify == null)
                return;

            var playback = await _spotify.Player.GetCurrentPlayback();

            if (playback?.IsPlaying ?? false)
                await _spotify.Player.PausePlayback();
            else
                await _spotify.Player.ResumePlayback();
        }
        public override async Task PreviousTrackAsync() => await _spotify?.Player.SkipPrevious()!;

        // Handle Volume Interactions
        public override async Task SetVolume(int volumePercent) => await _spotify?.Player.SetVolume(new PlayerVolumeRequest(volumePercent));

        // Handle Syncing Seek
        public override async Task SeekAsync(int positionMs) => await _spotify?.Player.SeekTo(new PlayerSeekToRequest(positionMs))!;

        // Handle Shuffle and Repeat
        public override async Task SetShuffleAsync(bool isOn)
            => await _spotify?.Player.SetShuffle(new PlayerShuffleRequest(isOn))!;

        public override async Task SetRepeatAsync(string mode) // mode: "off", "context", "track"
        {
            if (_spotify == null)
                return;

            PlayerSetRepeatRequest.State repeatState = mode.ToLower() switch
            {
                "track" => PlayerSetRepeatRequest.State.Track,
                "context" => PlayerSetRepeatRequest.State.Context,
                "off" => PlayerSetRepeatRequest.State.Off,
                _ => throw new ArgumentException($"Invalid repeat mode: {mode}")
            };

            await _spotify.Player.SetRepeat(new PlayerSetRepeatRequest(repeatState));
        }

    }

}