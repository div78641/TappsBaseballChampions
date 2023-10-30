using System.Text.Json.Serialization;

namespace TappsBaseballChampions.Models.Data.Input
{
    public class School
	{
		[JsonPropertyName("id")]
        public string? Id { get; set; }
		[JsonPropertyName("name")]
		public string? Name { get; set; }
	}
}

