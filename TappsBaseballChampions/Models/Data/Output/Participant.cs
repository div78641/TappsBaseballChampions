namespace TappsBaseballChampions.Models.Data.Output;

public class Participant
{
    public Participant()
    {
        TrophyCase = new TrophyCase();
    }

    public string? Id { get; set; }
    public string? Name { get; set; }
    public TrophyCase? TrophyCase { get; set; }
}