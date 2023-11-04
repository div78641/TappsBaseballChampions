namespace TappsBaseballChampions.Models.Data.Output;

public abstract class Trophy : ITrophy
{
    public Trophy()
    {
        Name = string.Empty;
        Stockpile = new List<Year>();
    }
    public int Total => Stockpile.Count;
    public string Name { get; set; }
    public List<Year> Stockpile { get; set; }
}