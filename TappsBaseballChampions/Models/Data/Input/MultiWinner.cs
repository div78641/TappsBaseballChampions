using System.Text.Json.Serialization;

namespace TappsBaseballChampions.Models.Data.Input
{
    public abstract class MultiWinner
	{
        [JsonPropertyName("T")]
        public List<string>? AllTapps { get; set; }
        [JsonPropertyName("1a")]
        public List<string>? OneA { get; set; }
        [JsonPropertyName("1a/2a")]
        public List<string>? OneAndTwoA { get; set; }
        [JsonPropertyName("2a")]
        public List<string>? TwoA { get; set; }
        [JsonPropertyName("3a")]
        public List<string>? ThreeA { get; set; }
        [JsonPropertyName("4a")]
        public List<string>? FourA { get; set; }
        [JsonPropertyName("5a")]
        public List<string>? FiveA { get; set; }
        [JsonPropertyName("6a")]
        public List<string>? SixA { get; set; }
        [JsonPropertyName("dI")]
        public List<string>? DivisionOne { get; set; }
        [JsonPropertyName("dII")]
        public List<string>? DivisionTwo { get; set; }
        [JsonPropertyName("dIII")]
        public List<string>? DivisionThree { get; set; }
        [JsonPropertyName("dIV")]
        public List<string>? DivisionFour { get; set; }
        [JsonPropertyName("dV")]
        public List<string>? DivisionFive { get; set; }
    }
}

