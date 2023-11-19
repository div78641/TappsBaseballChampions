using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using TappsBaseballChampions.Models.Configuration;
using TappsBaseballChampions.Models.Constants;
using TappsBaseballChampions.Models.Data.Input;
using TappsBaseballChampions.Models.Data.Output;
using static System.IO.Directory;
using FourthPlaceIn = TappsBaseballChampions.Models.Data.Input.FourthPlace;
using SecondPlaceIn = TappsBaseballChampions.Models.Data.Input.SecondPlace;
using ThirdPlaceIn = TappsBaseballChampions.Models.Data.Input.ThirdPlace;
using YearIn = TappsBaseballChampions.Models.Data.Input.Year;
using YearOut = TappsBaseballChampions.Models.Data.Output.Year;

public sealed class ConsoleApplication
{
    private readonly ILogger<ConsoleApplication> _logger;
    private readonly IConfiguration _configuration;

    private FileInfo[]? _inputFiles { get; set; }
    private Options? _options { get; set; }
    private List<YearIn>? _yearsOfCompetition { get; set; }
    private Schools? _schools { get; set; }
    private List<Participant>? _participants { get; set; }
    private List<Annum>? _annos { get; set; }
    private List<DivisionHistory>? _partes { get; set; }

    public ConsoleApplication(ILogger<ConsoleApplication> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        PrepareForWork();
    }

    public void Run(string[] args)
    {
        _logger.LogInformation("Start Running");
        BuildChampionshipHistory();
        BuildAnnualHistory();
        BuildDivisionHistory();
        SaveHistoryToFile();
        _logger.LogInformation("End Run");
    }

    #region (------ Private Methods ------)
    private void PrepareForWork()
    {
        GatherOptions();
        LoadInputFiles();
        LoadYearsOfCompetition();
        LoadSchoolInformation();
        if (_participants == null)
        {
            _participants = new List<Participant>();
        }
        if (_annos == null)
        {
            _annos = new List<Annum>();
        }
        if (_partes == null)
        {
            _partes = new List<DivisionHistory>();
        }
    }

    private void LoadYearsOfCompetition()
    {
        _logger.LogInformation("Loading Years of Competition");
        if (_inputFiles != null)
        {
            if (_yearsOfCompetition == null)
            {
                _yearsOfCompetition = new List<YearIn>();
            }
            else
            {
                _yearsOfCompetition.Clear();
            }

            Regex regex = new(@"\d{4}.json");
            var yearFiles = _inputFiles.Where(fi => regex.IsMatch(fi.Name)).ToArray();
            _logger.LogDebug("Number of Input Files: [{files}]", yearFiles.Length);
            foreach (var file in yearFiles)
            {
                _logger.LogDebug("Fully qualified path: {path}", file.FullName);
                // parse json file into object 
                var year = DeserializeJsonFromFile<YearIn>(file.FullName);
                _logger.LogDebug("Year of Competition {year}", year.ID.ToString());
                _yearsOfCompetition.Add(year);
            }
            _logger.LogDebug("Number of Year Objects Created: [{years}]", _yearsOfCompetition.Count);
            _logger.LogInformation("Years of Competition Loaded");
        }
        else
        {
            _logger.LogError("Years of Competition Failed to Load");
        }
    }

    private void LoadInputFiles()
    {
        _logger.LogInformation("Loading Input Files");
        var inputDirectory = (_options != null)
            ? new DirectoryInfo($"{GetCurrentDirectory()}{_options.InputFolder}")
            : null;

        if (inputDirectory != null && inputDirectory.Exists)
        {
            _inputFiles = inputDirectory.GetFiles("*.json");
            _logger.LogInformation("Input Files Loaded");
        }
        else
        {
            _logger.LogError("Input Files Failed to Load");
        }
    }

    private void GatherOptions()
    {
        _logger.LogInformation("Gathering Options");
        _options = new Options() { InputFolder = "", OutputFolder = "" };
        _configuration.GetSection("Options").Bind(_options);
        _logger.LogDebug("Input Folder {input}", _options.InputFolder);
        _logger.LogDebug("Output Folder {output}", _options.OutputFolder);
        _logger.LogInformation("Options Gathered");
    }

    private void LoadSchoolInformation()
    {
        _logger.LogInformation("Loading Schools");
        if (_inputFiles != null)
        {
            var schoolFile = _inputFiles.Where(fi => fi.Name == "Schools.json").FirstOrDefault();
            if (schoolFile != null)
            {
                _schools = DeserializeJsonFromFile<Schools>(schoolFile.FullName);
                _logger.LogDebug("Schools loaded: {schools}", _schools.Items.Count);
                _logger.LogInformation("Schools Loaded");
            }
            else
            {
                _logger.LogError("Schools not Loaded, School File Missing");
            }
        }
        else
        {
            _logger.LogError("Schools not Loaded, No Input File");
        }
    }

    private void BuildChampionshipHistory()
    {
        _logger.LogInformation("Start Building History");
        // use list to create report file or files
        if (_yearsOfCompetition != null)
        {
            foreach (var year in _yearsOfCompetition)
            {
                if (year.Champion != null)
                {
                    AddTrophiesToCase(year.Champion, year.ID);
                    _logger.LogInformation("Champions added");
                }
                if (year.SecondPlace != null)
                {
                    AddTrophiesToCase(year.SecondPlace, year.ID);
                    _logger.LogInformation("Second Place added");
                }
                if (year.ThirdPlace != null)
                {
                    AddThirdPlacesToCase(year.ThirdPlace, year.ID);
                    _logger.LogInformation("Third Place added");
                }
                if (year.FourthPlace != null)
                {
                    AddTrophiesToCase(year.FourthPlace, year.ID);
                    _logger.LogInformation("Fourth Place added");
                }
            }
        }
        else
        {
            _logger.LogError("No Years of Competition from which to Build History");
        }

        // for each year in input directory
        _logger.LogInformation("End Building History");
    }

    private void BuildAnnualHistory()
    {
        _logger.LogInformation("Start Building Annual History");
        if (_participants != null)
        {
            foreach (var participant in _participants)
            {
                if (participant.TrophyCase?.FirstPlaceFinishes?.Stockpile != null && participant.TrophyCase?.FirstPlaceFinishes?.Stockpile.Count() > 0)
                    ProcessStockpile(participant.Id, participant.Name, participant.TrophyCase.FirstPlaceFinishes.Stockpile, Place.First);

                if (participant.TrophyCase?.SecondPlaceFinishes?.Stockpile != null && participant.TrophyCase?.SecondPlaceFinishes?.Stockpile.Count() > 0)
                    ProcessStockpile(participant.Id, participant.Name, participant.TrophyCase.SecondPlaceFinishes.Stockpile, Place.Second);

                if (participant.TrophyCase?.ThirdPlaceFinishes?.Stockpile != null && participant.TrophyCase?.ThirdPlaceFinishes?.Stockpile.Count() > 0)
                    ProcessStockpile(participant.Id, participant.Name, participant.TrophyCase.ThirdPlaceFinishes.Stockpile, Place.Third);

                if (participant.TrophyCase?.FourthPlaceFinishes?.Stockpile != null && participant.TrophyCase?.FourthPlaceFinishes?.Stockpile.Count() > 0)
                    ProcessStockpile(participant.Id, participant.Name, participant.TrophyCase.FourthPlaceFinishes.Stockpile, Place.Fourth);
            }
        }
        else
        {
            _logger.LogError("No participants from which to build an annual history");
        }
        _logger.LogInformation("End Building Annual History");
    }

    private void BuildDivisionHistory()
    {
        _logger.LogInformation("Start Building Division History");
        if (_participants != null)
        {
            foreach (var participant in _participants)
            {
                ProcessStockpilesForDivisions(participant, Division.AllTapps);
                ProcessStockpilesForDivisions(participant, Division.OneA);
                ProcessStockpilesForDivisions(participant, Division.OneAndTwoA);
                ProcessStockpilesForDivisions(participant, Division.TwoA);
                ProcessStockpilesForDivisions(participant, Division.ThreeA);
                ProcessStockpilesForDivisions(participant, Division.FourA);
                ProcessStockpilesForDivisions(participant, Division.FiveA);
                ProcessStockpilesForDivisions(participant, Division.DivisionOne);
                ProcessStockpilesForDivisions(participant, Division.DivisionTwo);
                ProcessStockpilesForDivisions(participant, Division.DivisionThree);
                ProcessStockpilesForDivisions(participant, Division.DivisionFour);
                ProcessStockpilesForDivisions(participant, Division.DivisionFive);
            }
        }
        else
        {
            _logger.LogError("No participants from which to build the division history");
        }
        _logger.LogInformation("End Building Division History");
    }

    private void ProcessStockpilesForDivisions(Participant participant, string divisionId)
    {
        bool shouldAddAfterUpdate = false;
        _partes ??= new List<DivisionHistory>();
        var workingDivision = _partes.Where(dh => dh.DivisionId == divisionId).FirstOrDefault();
        if (workingDivision == null)
        {
            workingDivision = new DivisionHistory() { DivisionId = divisionId };
            shouldAddAfterUpdate = true;
        }

        if (participant.TrophyCase?.FirstPlaceFinishes?.Stockpile != null)
        {
            if (participant.TrophyCase?.FirstPlaceFinishes?.Stockpile.Count() > 0)
            {
                if(participant.TrophyCase.FirstPlaceFinishes.Stockpile.Any(y => y.Division == divisionId))
                {
                    ProcessStockpileForDivision(workingDivision, participant.Id, participant.Name, participant.TrophyCase.FirstPlaceFinishes.Stockpile, divisionId, Place.First);
                };
            }
        }

        if (participant.TrophyCase?.SecondPlaceFinishes?.Stockpile != null)
        {
            if (participant.TrophyCase.SecondPlaceFinishes.Stockpile.Count() > 0)
            {
                if (participant.TrophyCase.SecondPlaceFinishes.Stockpile.Any(y => y.Division == divisionId))
                {
                    ProcessStockpileForDivision(workingDivision, participant.Id, participant.Name, participant.TrophyCase.SecondPlaceFinishes.Stockpile, divisionId, Place.Second);
                };
            }
        }

        if (participant.TrophyCase?.ThirdPlaceFinishes?.Stockpile != null)
        {
            if (participant.TrophyCase.ThirdPlaceFinishes.Stockpile.Count() > 0)
            {
                if (participant.TrophyCase.ThirdPlaceFinishes.Stockpile.Any(y => y.Division == divisionId))
                {
                    ProcessStockpileForDivision(workingDivision, participant.Id, participant.Name, participant.TrophyCase.ThirdPlaceFinishes.Stockpile, divisionId, Place.Third);
                };
            }
        }

        if (participant.TrophyCase?.FourthPlaceFinishes?.Stockpile != null)
        {
            if (participant.TrophyCase.FourthPlaceFinishes.Stockpile.Count() > 0)
            {
                if (participant.TrophyCase.FourthPlaceFinishes.Stockpile.Any(y => y.Division == divisionId))
                {
                    ProcessStockpileForDivision(workingDivision, participant.Id, participant.Name, participant.TrophyCase.FourthPlaceFinishes.Stockpile, divisionId, Place.Fourth);
                };
            }
        }

        if (shouldAddAfterUpdate)
        {
            _partes.Add(workingDivision);
        }
    }

    private void ProcessStockpileForDivision(DivisionHistory workingDivision, string? schoolId, string? schoolName, List<YearOut> stockpile, string divisionId, Place finish)
    {
        bool shouldAddAfterUpdate = false;
        var workingSchool = workingDivision.DivisionWinners?.Where(dp => dp.SchoolId == schoolId).FirstOrDefault();
        if (workingSchool == null)
        {
            workingSchool = new DivisionParticipant() { SchoolId = schoolId, SchoolName = schoolName };
            shouldAddAfterUpdate = true;
        }

        foreach (var yearOut in stockpile)
        {
            if (yearOut.Division == divisionId)
            {
                switch (finish)
                {
                    case Place.First:
                        workingSchool.ChampionshipYears?.Add($"{yearOut.ID}");
                        break;
                    case Place.Second:
                        workingSchool.SecondPlaceYears?.Add($"{yearOut.ID}");
                        break;
                    case Place.Third:
                        workingSchool.ThirdPlaceYears?.Add($"{yearOut.ID}");
                        break;
                    case Place.Fourth:
                        workingSchool.FourthPlaceYears?.Add($"{yearOut.ID}");
                        break;
                }
            }
        }

        if (shouldAddAfterUpdate)
            workingDivision.DivisionWinners?.Add(workingSchool);
    }

    private void ProcessStockpile(string? id, string? name, List<YearOut> stockpile, Place finish)
    {
        foreach (var trophyYear in stockpile)
        {
            var annum = new Annum
            {
                SchoolId = id,
                SchoolName = name,
                Finish = finish,
                SeasonYear = trophyYear.ID.ToString(),
                Division = trophyYear.Division
            };

            _annos ??= new List<Annum>();

            _annos.Add(annum);
        }
    }

    private void AddThirdPlacesToCase(ThirdPlaceIn thirdPlace, int? seasonYear)
    {
        if (thirdPlace.AllTapps != null)
        {
            foreach (string winner in thirdPlace.AllTapps)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.AllTapps);
            }
        }
        if (thirdPlace.OneA != null)
        {
            foreach (string winner in thirdPlace.OneA)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.OneA);
            }
        }
        if (thirdPlace.OneAndTwoA != null)
        {
            foreach (string winner in thirdPlace.OneAndTwoA)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.OneAndTwoA);
            }
        }
        if (thirdPlace.TwoA != null)
        {
            foreach (string winner in thirdPlace.TwoA)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.TwoA);
            }
        }
        if (thirdPlace.ThreeA != null)
        {
            foreach (string winner in thirdPlace.ThreeA)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.ThreeA);
            }
        }
        if (thirdPlace.FourA != null)
        {
            foreach (string winner in thirdPlace.FourA)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.FourA);
            }
        }
        if (thirdPlace.FiveA != null)
        {
            foreach (string winner in thirdPlace.FiveA)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.FiveA);
            }
        }
        if (thirdPlace.SixA != null)
        {
            foreach (string winner in thirdPlace.SixA)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.SixA);
            }
        }
        if (thirdPlace.DivisionOne != null)
        {
            foreach (string winner in thirdPlace.DivisionOne)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.DivisionOne);
            }
        }
        if (thirdPlace.DivisionTwo != null)
        {
            foreach (string winner in thirdPlace.DivisionTwo)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.DivisionTwo);
            }
        }
        if (thirdPlace.DivisionThree != null)
        {
            foreach (string winner in thirdPlace.DivisionThree)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.DivisionThree);
            }
        }
        if (thirdPlace.DivisionFour != null)
        {
            foreach (string winner in thirdPlace.DivisionFour)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.DivisionFour);
            }
        }
        if (thirdPlace.DivisionFive != null)
        {
            foreach (string winner in thirdPlace.DivisionFive)
            {
                AddToStockpile(Place.Third, seasonYear, winner, Division.DivisionFive);
            }
        }
    }

    private void AddTrophiesToCase<T>(T winner, int? seasonYear) where T : Winner 
    {
        switch (winner)
        {
            case Champion:
                ProcessWinner(Place.First, seasonYear, winner);
                break;
            case SecondPlaceIn:
                ProcessWinner(Place.Second, seasonYear, winner);
                break;
            case FourthPlaceIn:
                ProcessWinner(Place.Fourth, seasonYear, winner);
                break;
            default:
                _logger.LogWarning("Trophy Winner Unknown {winner}", winner.GetType().ToString());
                break;
        }
    }

    private void ProcessWinner<T>(Place place, int? seasonYear, T winner) where T : Winner
    {
        if (winner.AllTapps != null)
        {
            AddToStockpile(place, seasonYear, winner.AllTapps, Division.AllTapps);
        }
        if (winner.OneA != null)
        {
            AddToStockpile(place, seasonYear, winner.OneA, Division.OneA);
        }
        if (winner.OneAndTwoA != null)
        {
            AddToStockpile(place, seasonYear, winner.OneAndTwoA, Division.OneAndTwoA);
        }
        if (winner.TwoA != null)
        {
            AddToStockpile(place, seasonYear, winner.TwoA, Division.TwoA);
        }
        if (winner.ThreeA != null)
        {
            AddToStockpile(place, seasonYear, winner.ThreeA, Division.ThreeA);
        }
        if (winner.FourA != null)
        {
            AddToStockpile(place, seasonYear, winner.FourA, Division.FourA);
        }
        if (winner.FiveA != null)
        {
            AddToStockpile(place, seasonYear, winner.FiveA, Division.FiveA);
        }
        if (winner.SixA != null)
        {
            AddToStockpile(place, seasonYear, winner.SixA, Division.SixA);
        }
        if (winner.DivisionOne != null)
        {
            AddToStockpile(place, seasonYear, winner.DivisionOne, Division.DivisionOne);
        }
        if (winner.DivisionTwo != null)
        {
            AddToStockpile(place, seasonYear, winner.DivisionTwo, Division.DivisionTwo);
        }
        if (winner.DivisionThree != null)
        {
            AddToStockpile(place, seasonYear, winner.DivisionThree, Division.DivisionThree);
        }
        if (winner.DivisionFour != null)
        {
            AddToStockpile(place, seasonYear, winner.DivisionFour, Division.DivisionFour);
        }
        if (winner.DivisionFive != null)
        {
            AddToStockpile(place, seasonYear, winner.DivisionFive, Division.DivisionFive);
        }
    }

    private void AddToStockpile(Place place, int? seasonYear, string schoolId, string division)
    {
        if (_participants != null)
        {
            var participant = _participants.SingleOrDefault(p => p.Id == schoolId);
            if (participant != null)
            {
                UpdateParticipant(participant, place, seasonYear, division);
            }
            else
            {
                _logger.LogError("No Participant for {id}", schoolId);
                Participant? freshParticipant = CreateParticipant(schoolId);
                if (freshParticipant != null)
                {
                    UpdateParticipant(freshParticipant, place, seasonYear, division);
                    _participants.Add(freshParticipant);
                }
                else
                {
                    _logger.LogError("Participant for {id} could not be created", schoolId);
                    _logger.LogError("No Added Trophy for {year} in {division}", seasonYear, division);
                }
            }
        }
        else
        {
            _logger.LogError("Participants Not Found");
        }
    }

    private Participant? CreateParticipant(string schoolId)
    {
        _logger.LogInformation("Creating New Participant {schoolid}", schoolId);
        if (_schools != null)
        {
            var school = _schools.Items.SingleOrDefault(s => s.Id == schoolId);
            if (school != null)
            {
                Participant participant = new() { Id = schoolId, Name = school.Name };
                return participant;
            }
            else
            {
                _logger.LogError("School {id} Missing", schoolId);
                return null;
            }
        }
        else
        {
            _logger.LogError("Schools Do Not Exist");
            return null;
        }
    }

    private void UpdateParticipant(Participant participant, Place place, int? seasonYear, string division)
    {
        YearOut year = new() { ID = seasonYear, Division = division };
        switch (place)
        {
            case Place.First:
                participant.TrophyCase!.FirstPlaceFinishes!.Stockpile.Add(year);
                break;
            case Place.Second:
                participant.TrophyCase!.SecondPlaceFinishes!.Stockpile.Add(year);
                break;
            case Place.Third:
                participant.TrophyCase!.ThirdPlaceFinishes!.Stockpile.Add(year);
                break;
            case Place.Fourth:
                participant.TrophyCase!.FourthPlaceFinishes!.Stockpile.Add(year);
                break;
            default:
                _logger.LogWarning("Finish {place} in {year} Unknown", place, seasonYear);
                break;
        }
    }

    private void SaveHistoryToFile()
    {
        _logger.LogInformation("Saving Output Files");

        var outputDirectory = (_options != null)
            ? new DirectoryInfo($"{GetCurrentDirectory()}{_options.OutputFolder}")
            : null;

        if (outputDirectory != null)
        {
            if (!outputDirectory.Exists)
                outputDirectory.Create();

            if (_participants != null)
            {
                var fileNamePath = $"{outputDirectory.FullName}/TappsBaseballHistory.json";
                SerializeJsonToFile<List<Participant>>(_participants, fileNamePath);
                _logger.LogInformation("Overall History File Saved");
            }
            else
            {
                _logger.LogError("No Participants Found");
            }

            if (_annos != null)
            {
                var fileNamePath = $"{outputDirectory.FullName}/TappsBaseballHistoryByYear.json";
                SerializeJsonToFile<List<Annum>>(_annos, fileNamePath);
                _logger.LogInformation("Annual History File Saved");
            }
            else
            {
                _logger.LogError("No Annos Found");
            }

            if (_partes != null)
            {
                var fileNamePath = $"{outputDirectory.FullName}/TappsBaseballHistoryByDivision.json";
                SerializeJsonToFile<List<DivisionHistory>>(_partes, fileNamePath);
                _logger.LogInformation("Division History File Saved");
            }
            else
            {
                _logger.LogError("No Partes Found");
            }
        }
        else
        {
            _logger.LogError("Output Files Could Not be Saved");
        }
    }

    private static T DeserializeJsonFromFile<T>(string fullyQualifiedFileName)
    {
        T tObject = JsonSerializer.Deserialize<T>(LoadTextFromFile(fullyQualifiedFileName));
        return tObject;
    }

    private static void SerializeJsonToFile<T>(T data, string fullyQualifiedFileName)
    {
        string json = JsonSerializer.Serialize(data);
        File.WriteAllText(fullyQualifiedFileName, json);
    }

    private static string LoadTextFromFile(string fullyQualifiedFileName)
    {
        if (File.Exists(fullyQualifiedFileName))
        {
            return File.ReadAllText(fullyQualifiedFileName);
        }
        else
        {
            return string.Empty;
        }
    }
    #endregion
}