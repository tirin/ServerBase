using System.Text.Json.Serialization;

namespace ServerBase.Controllers
{
    public class UserLoginResponse
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("username")]
        public string Name { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}