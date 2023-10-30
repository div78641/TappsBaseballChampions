using System.Text.Json.Serialization;

namespace TappsBaseballChampions.Models.Data.Input
{
    public class Year
	{
		[JsonPropertyName("Year")]
		public int? ID { get; set; }
		public Champion? Champion { get; set; }
		public SecondPlace? SecondPlace { get; set; }
		public ThirdPlace? ThirdPlace { get; set; }
		public FourthPlace? FourthPlace { get; set; }
	}
}

