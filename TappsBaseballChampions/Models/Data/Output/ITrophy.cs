namespace TappsBaseballChampions.Models.Data.Output;

public interface ITrophy {
    public int Total { get; }
    public string Name { get; set; }

    public List<Year> Stockpile { get; set; }

}