namespace TappsBaseballWeb.Data;

public interface ITrophy {
    public int Total { get; }
    public string Name { get; set; }

    public List<Year> Stockpile { get; set; }

}