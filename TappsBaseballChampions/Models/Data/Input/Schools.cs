using System.Text.Json.Serialization;

namespace TappsBaseballChampions.Models.Data.Input
{
    public class Schools
	{
		public Schools()
		{
			Items = new List<School>();
		}

		[JsonPropertyName("Schools")]
		public List<School> Items { get; set; }
	}
}

