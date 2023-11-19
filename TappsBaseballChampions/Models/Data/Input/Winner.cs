using System.Text.Json.Serialization;

namespace TappsBaseballChampions.Models.Data.Input
{
    public abstract class Winner : IWinner
	{
        [JsonPropertyName("T")]
        public string? AllTapps { get; set; }
        [JsonPropertyName("1a")]
        public string? OneA { get; set; }
        [JsonPropertyName("1a/2a")]
        public string? OneAndTwoA { get; set; }
        [JsonPropertyName("2a")]
        public string? TwoA { get; set; }
        [JsonPropertyName("3a")]
        public string? ThreeA { get; set; }
        [JsonPropertyName("4a")]
        public string? FourA { get; set; }
        [JsonPropertyName("5a")]
        public string? FiveA { get; set; }
        [JsonPropertyName("6a")]
        public string? SixA { get; set; }
        [JsonPropertyName("dI")]
        public string? DivisionOne { get; set; }
        [JsonPropertyName("dII")]
        public string? DivisionTwo { get; set; }
        [JsonPropertyName("dIII")]
        public string? DivisionThree { get; set; }
        [JsonPropertyName("dIV")]
        public string? DivisionFour { get; set; }
        [JsonPropertyName("dV")]
        public string? DivisionFive { get; set; }
    }
}

