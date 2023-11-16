namespace TappsBaseballChampions.Models.Data.Output
{
    public class DivisionParticipant
    {
        public DivisionParticipant()
        {
            ChampionshipYears = new List<string>();
            SecondPlaceYears = new List<string>();
            ThirdPlaceYears = new List<string>();
            FourthPlaceYears = new List<string>();
        }
        public string? SchoolId { get; set; }
        public string? SchoolName { get; set; }

        public List<string>? ChampionshipYears { get; set; }
        public int? Championships => ChampionshipYears?.Count;

        public List<string>? SecondPlaceYears { get; set; }
        public int? SecondPlaceFinishes => SecondPlaceYears?.Count;

        public List<string>? ThirdPlaceYears { get; set; }
        public int? ThirdPlaceFinishes => ThirdPlaceYears?.Count;

        public List<string>? FourthPlaceYears { get; set; }
        public int? FourthPlaceFinishes => FourthPlaceYears?.Count;
    }
}

