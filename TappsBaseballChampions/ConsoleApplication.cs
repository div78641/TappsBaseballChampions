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
                    AddTrophiesToCase(year.ThirdPlace, year.ID);
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
            case ThirdPlaceIn:
                ProcessWinner(Place.Third, seasonYear, winner);
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
                    UpdateParticipant(freshParticipant, place, seasonYear, schoolId);
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
        _logger.LogInformation("Saving Output File");

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
                _logger.LogInformation("Output File Saved");
            }
            else
            {
                _logger.LogError("No Participants Found");
            }
        }
        else
        {
            _logger.LogError("Output File Could Not be Saved");
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