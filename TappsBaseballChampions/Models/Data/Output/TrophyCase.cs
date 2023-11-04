namespace TappsBaseballChampions.Models.Data.Output;

public sealed class TrophyCase
{
    public TrophyCase()
    {
        FirstPlaceFinishes = new FirstPlace();
        SecondPlaceFinishes = new SecondPlace();
        ThirdPlaceFinishes = new ThirdPlace();
        FourthPlaceFinishes = new FourthPlace();
    }

    public FirstPlace? FirstPlaceFinishes { get; set; }
    public SecondPlace? SecondPlaceFinishes { get; set; }
    public ThirdPlace? ThirdPlaceFinishes { get; set; }
    public FourthPlace? FourthPlaceFinishes { get; set; }
}