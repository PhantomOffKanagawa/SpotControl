using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Threading.Tasks;

namespace SpotControl.Helpers
{
    public class OAuthHelper
    {
        private const string ClientId = "d99bf089499c43549000c19e58b400e3";
        private const string RedirectUri = "http://localhost:5000/callback";
        private static readonly string[] Scopes = new[] {
               SpotifyAPI.Web.Scopes.UserReadPlaybackState,
               SpotifyAPI.Web.Scopes.UserModifyPlaybackState,
               SpotifyAPI.Web.Scopes.UserReadCurrentlyPlaying
           };

        public async Task<SpotifyClient> AuthenticateAsync()
        {
            var loginRequest = new LoginRequest(new Uri(RedirectUri), ClientId, LoginRequest.ResponseType.Token)
            {
                Scope = new List<string>(Scopes)
            };

            var server = new EmbedIOAuthServer(new Uri(RedirectUri), 5000);
            await server.Start();

            ImplictGrantResponse? token = null;
            server.ImplictGrantReceived += async (sender, response) =>
            {
                token = response;
                await server.Stop();
            };

            // Wait for the token to be received  
            while (token == null)
            {
                await Task.Delay(100);
            }

            var spotify = new SpotifyClient(token.AccessToken);
            return spotify;
        }
    }
}