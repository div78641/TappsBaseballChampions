namespace TappsBaseballChampions.Models.Data.Output
{
    public class DivisionHistory
	{
        public DivisionHistory()
        {
            DivisionWinners = new List<DivisionParticipant>();
        }
        public string? DivisionId { get; set; }
        public List<DivisionParticipant>? DivisionWinners { get; set; }
    }
}

