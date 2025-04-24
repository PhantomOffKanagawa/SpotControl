using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;

namespace SpotControl.Services
{
    /// <summary>
    /// Service for handling Spotify authentication and playback control.
    /// </summary>
    public class SpotifyService
    {
        private SpotifyClient? _spotify;
        public bool IsAuthenticated => _spotify != null;

        public SpotifyClient? Client => _spotify;

        public async Task<bool> AuthenticateAsync()
        {
            var config = SpotifyClientConfig.CreateDefault();

            var request = new LoginRequest(
                new Uri("http://localhost:5000/callback"),
                "d99bf089499c43549000c19e58b400e3",
                LoginRequest.ResponseType.Code
            )
            {
                Scope = new[]
                {
                Scopes.UserReadPlaybackState,
                Scopes.UserModifyPlaybackState,
                Scopes.UserReadCurrentlyPlaying
            }
            };

            var server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
            var tcs = new TaskCompletionSource<bool>();

            await server.Start();

            server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await server.Stop();

                try
                {
                    var token = await new OAuthClient().RequestToken(
                        new AuthorizationCodeTokenRequest("d99bf089499c43549000c19e58b400e3", "19cecc4bae9446e7b9b4e0bf9da18d83", response.Code, new Uri("http://localhost:5000/callback"))
                    );

                    _spotify = new SpotifyClient(config.WithToken(token.AccessToken));
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Authentication failed: " + ex.Message);
                    tcs.SetResult(false);
                }
            };

            BrowserUtil.Open(request.ToUri());
            return await tcs.Task; // Wait until authentication completes
        }

        public async Task<FullTrack?> GetCurrentTrackAsync()
        {
            if (_spotify == null)
                return null;

            var playback = await _spotify.Player.GetCurrentPlayback();
            return playback?.Item as FullTrack;
        }


        public async Task PlayPauseAsync()
        {
            if (_spotify == null)
                return;

            var playback = await _spotify.Player.GetCurrentPlayback();

            if (playback?.IsPlaying ?? false)
                await _spotify.Player.PausePlayback();
            else
                await _spotify.Player.ResumePlayback();
        }


        // Handle Volume Interactions
        public async Task SetVolume(int volumePercent) => await _spotify?.Player.SetVolume(new PlayerVolumeRequest(volumePercent));
        public async Task NextTrackAsync() => await _spotify?.Player.SkipNext()!;
        public async Task PreviousTrackAsync() => await _spotify?.Player.SkipPrevious()!;
        public async Task SeekAsync(int positionMs) => await _spotify?.Player.SeekTo(new PlayerSeekToRequest(positionMs))!;
        public async Task<CurrentlyPlayingContext?> GetPlaybackInfoAsync()
    => await _spotify?.Player.GetCurrentPlayback();

        public async Task SetShuffleAsync(bool isOn)
            => await _spotify?.Player.SetShuffle(new PlayerShuffleRequest(isOn))!;

        public async Task SetRepeatAsync(string mode) // mode: "off", "context", "track"
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

        // Replace the method signature and return type to resolve the CS0246 error.  
        //public async Task<CurrentlyPlayingContext?> GetPlaybackInfoAsync()
        //{
        //    if (_spotify == null)
        //        throw new InvalidOperationException("Spotify client is not initialized.");

        //    var playback = await _spotify.Player.GetCurrentPlayback();
        //    return playback;
        //}

    }

}