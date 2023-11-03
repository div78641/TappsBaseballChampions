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
                ProcessWinner(Places.First, seasonYear, winner);
                break;
            case SecondPlaceIn:
                ProcessWinner(Places.Second, seasonYear, winner);
                break;
            case ThirdPlaceIn:
                ProcessWinner(Places.Third, seasonYear, winner);
                break;
            case FourthPlaceIn:
                ProcessWinner(Places.Fourth, seasonYear, winner);
                break;
            default:
                _logger.LogWarning("Trophy Winner Unknown {winner}", winner.GetType().ToString());
                break;
        }
    }

    private void ProcessWinner<T>(Places place, int? seasonYear, T winner) where T : Winner
    {
        throw new NotImplementedException();
    }

    private static T DeserializeJsonFromFile<T>(string fullyQualifiedFileName)
    {
        T tObject = JsonSerializer.Deserialize<T>(LoadTextFromFile(fullyQualifiedFileName));
        return tObject;
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