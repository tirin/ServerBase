using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServerBase.Controllers
{
    public class PointGainRequest
    {
        [Required]
        [JsonPropertyName("point_type")]
        public string PointType { get; set; }

        [Required]
        [JsonPropertyName("quantity")]
        public long Quantity { get; set; }
    }
}