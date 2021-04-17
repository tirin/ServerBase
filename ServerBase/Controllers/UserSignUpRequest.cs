using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServerBase.Controllers
{
    public class UserSignUpRequest
    {
        [Required]
        [JsonPropertyName("username")]
        public string Name { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
