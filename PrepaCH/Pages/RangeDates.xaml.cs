using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using PrepaCH.Resources.Strings;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace PrepaCH.Pages;

public partial class RangeDatesViewModel : ObservableObject
{
    #region Static Options for pickers
    public class PickerOption
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public PickerOption(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public List<PickerOption> Weapons { get; set; } = new()
    {
        new PickerOption(Strings.Gun, "G"),
        new PickerOption(Strings.Pistol, "P"),
    };

    public List<PickerOption> Programs { get; set; } = new()
    {
        new PickerOption(Strings.ObligatoryProgram, "OP"),
        new PickerOption(Strings.FederalProgram50m, "BP"),
        new PickerOption(Strings.FieldShooting, "FS"),
        new PickerOption(Strings.CompetitionShooting, "SF"),
        new PickerOption(Strings.YoungShooters, "JS"),
        new PickerOption(Strings.Other, "Anderes"),
    };

    public List<PickerOption> Cantons { get; set; } = new()
    {
        new PickerOption("  ", "1"),
        new PickerOption("AG", "1.19"),
        new PickerOption("AI", "1.16"),
        new PickerOption("AR", "1.15"),
        new PickerOption("BE", "1.02"),
        new PickerOption("BL", "1.13"),
        new PickerOption("BS", "1.13"),//Same value in source as well...
		new PickerOption("FR", "1.10"),
        new PickerOption("GE", "1.25"),
        new PickerOption("GL", "1.08"),
        new PickerOption("GR", "1.18"),
        new PickerOption("JU", "1.26"),
        new PickerOption("LU", "1.03"),
        new PickerOption("NE", "1.24"),
        new PickerOption("NW", "1.07"),
        new PickerOption("OW", "1.06"),
        new PickerOption("SG", "1.17"),
        new PickerOption("SH", "1.14"),
        new PickerOption("SO", "1.11"),
        new PickerOption("SZ", "1.05"),
        new PickerOption("TG", "1.20"),
        new PickerOption("TI", "1.21"),
        new PickerOption("UR", "1.04"),
        new PickerOption("VD", "1.22"),
        new PickerOption("VS", "1.23"),
        new PickerOption("ZG", "1.09"),
        new PickerOption("ZH", "1.01"),
    };

    #endregion
    
    public class Stand
    {
        public class RangeDate
        {
            public readonly DateTime Start;
            public readonly DateTime End;
            public readonly string Title;
            public readonly string Location;

            public RangeDate(DateTime start, DateTime end, string title, string location)
            {
                Start = start;
                End = end;
                Title = title;
                Location = location;
            }
        }

        public string Name;
        public Location? Location;
        public List<RangeDate> StandRangeDates;
        public double Distance = -1;

        public Stand(string name)
        {
            Name = name;
            StandRangeDates = new List<RangeDate>();
            StandClubs = new List<Club>();
        }

        //Some bullshit for the stands that do this
        public List<Club> StandClubs;
        public class Club
        {
            public string Name;
            public List<RangeDate> ClubRangeDates;

            public Club(string name)
            {
                Name = name;
                ClubRangeDates = new List<RangeDate>();
            }
        }
    }

    [ObservableProperty]
    public PickerOption weapon;

    [ObservableProperty]
    public PickerOption program;

    [ObservableProperty]
    public PickerOption canton;

    [ObservableProperty]
    public DateTime from = new DateTime(DateTime.Now.Year, 1, 1);

    [ObservableProperty]
    public DateTime to = new DateTime(DateTime.Now.Year, 12, 31);

    FeatureCollection? communes = null;

    private async Task<FeatureCollection> GetCommunes()
    {
        if (communes != null)
        {
            return communes;
        }

        using var stream = await FileSystem.OpenAppPackageFileAsync("output.json");
        using var reader = new StreamReader(stream);
       
        communes = JsonConvert.DeserializeObject<FeatureCollection>(reader.ReadToEnd());
        return communes;
    }

    [RelayCommand]
    public async Task FindNearMe()
    {
        Searching.Invoke(this, true);

        Location? location = await Utils.GetCachedLocation();

        if (location == null)
        {
            await App.PopupService.Alert("Cannot get location", "Make sure you have given the appropriate permissions or that your device's location has been turned on.", "OK");
        }

        Debug.WriteLine($"Location : {location}");

        //Search & filter by closest & earliest
        // first canton is ALL
        //string queryStr = BuildQuery(DateTime.Now, To, Cantons.First(), Program, Weapon);
        string queryStr = BuildQuery(From, To, Cantons.First(), Program, Weapon);
        List<Stand> results = await Query(queryStr, true);

        //Sort by distance etc
        Debug.WriteLine($"Removing nulls (current count {results.Count})");
        List<Stand> withLocation = (from result in results
                           where result.Location != null
                           select result).ToList();
        Debug.WriteLine($"Removed nulls (new count {withLocation.Count})");
        foreach (var q in withLocation)
        {
            q.Distance = location.CalculateDistance2D(q.Location);
        }
        //withLocation.Sort((x, y) => location.CalculateDistance2D(x.Location).CompareTo(location.CalculateDistance2D(y.Location)));
        withLocation.Sort((x, y) => x.Distance.CompareTo(y.Distance));
        Debug.WriteLine("Sorted results by distance");
        DisplayResults(withLocation);
    }

    [RelayCommand]
    public async Task Search()
    {
        Searching.Invoke(this, true);

        string queryStr = BuildQuery(From, To, Canton, Program, Weapon);
        var results = await Query(queryStr);
        DisplayResults(results);
    }

    private async Task<List<Stand>> Query(string queryStr, bool withCommuneSearch = false)
    {
        List<Stand> rangesDates = new List<Stand>();

        try
        {
            HttpClient _client = new HttpClient(); ;

            StringContent query = new StringContent(queryStr, Encoding.UTF8, "text/xml");
            Debug.WriteLine($"Query - {queryStr}");

            Uri uri = new Uri("https://ssv-vva.esport.ch/p2plus/ssv/SAT4Calls.asp");

            HttpResponseMessage response = await _client.PostAsync(uri, query);
            Debug.WriteLine($"Response Code - {response.StatusCode}");
            Debug.WriteLine($"Response Content - {response.Content.ReadAsStringAsync().Result}");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response.Content.ReadAsStringAsync().Result);
            Debug.WriteLine($"Response Root - {doc.DocumentElement.InnerText}");

            string[] rows = doc.DocumentElement.InnerText.Split(new string[] { "|||" }, StringSplitOptions.None);

            int standIndex = 0;
            

            for (int i = 1; i < rows.Length; i++)
            {
                string row = rows[i];
                Debug.WriteLine(row);
                string[] lineSplit = row.Split(new string[] { "|" }, StringSplitOptions.None);

                int col1 = int.Parse(lineSplit[0]);
                int col2 = int.Parse(lineSplit[1]);
                string col3 = lineSplit[2];

                //If the second column is equal to 1, we are at a new stand, we can then go to the next line
                if (col2 == 1)
                {
                    Debug.WriteLine($"Adding stand {col3}");
                    standIndex = col1;
                    rangesDates.Add(new Stand(col3));
                    continue;
                }

                if (standIndex == col2)
                {
                    Debug.WriteLine($"Adding range date for stand {rangesDates.Last().Name}");
                    //We split the 3rd column by it's delimiters
                    string[] col3Split = col3.Split(new string[] { "    " }, StringSplitOptions.None);

                    //Some cantons have nested shit (i.e BS), seemingly where they split it by clubs... for now, ignore
                    if (col3Split.Length == 1)
                    {
                        rangesDates.Last().StandClubs.Add(new Stand.Club(col3));
                        continue;
                    }

                    Debug.WriteLine($"Col3 size - {col3Split.Length}");

                    //Split last, which is name + location
                    string[] nameAndLocation = col3Split[2].Split(new string[] { "+++" }, StringSplitOptions.None);
                    Debug.WriteLine($"Name - {nameAndLocation[0]}");
                    Debug.WriteLine($"Location - {nameAndLocation[1]}");

                    string[] splitTime = col3Split[1].Split(new string[] { "-" }, StringSplitOptions.TrimEntries); ;
                    Debug.WriteLine($"splitTime size - {splitTime.Length}");
                    string startStr = $"{col3Split[0]} {splitTime[0]}";
                    string endStr = $"{col3Split[0]} {splitTime[1]}";
                    DateTime start = DateTime.ParseExact(startStr, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    DateTime end = DateTime.ParseExact(endStr, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                    //Ok, for the love of thank fuck, I think that each stand EITHER just has range dates, OR has clubs with their range dates
                    // no mix, so just need to check if the last range has clubs, and if so add the current range date to the last club

                    //Only update location if there is no location already
                    if (rangesDates.Last().Location == null)
                    {
                        Debug.WriteLine($"Gonna get location");
                        //2 scenarios are possible, either it's a GPS position in lat+long format, or a Commune, NPA Commune format (any other format cannot be extrapolated)

                        string[] gpsSPlitTry = nameAndLocation[1].Split(new string[] { "+" }, StringSplitOptions.TrimEntries);// ("+");
                        Debug.WriteLine($"GPS Split {gpsSPlitTry.Length}");
                        if (gpsSPlitTry.Length == 2)
                        {
                            //On language is set to French, will fail parsing as double is in "46.123" instead of "46,123" (period vs comma), so need to do invariant
                            rangesDates.Last().Location = new Location(double.Parse(gpsSPlitTry[0], CultureInfo.InvariantCulture), double.Parse(gpsSPlitTry[1], CultureInfo.InvariantCulture));
                            Debug.WriteLine($"Location : {rangesDates.Last().Location}");
                        }
                        //Need to search the fucking goddamn GeoJSON
                        //but ONLY if searching for nearby
                        else if (withCommuneSearch)
                        {
                            Debug.WriteLine("Cuck");
                            FeatureCollection fc = await GetCommunes();
                            //https://stackoverflow.com/questions/32589177/find-4-sequence-numbers-in-string-using-c-sharp
                            var regex = new Regex(@"\d{4}");

                            foreach (Match match in regex.Matches(nameAndLocation[1]))
                            {
                                Debug.WriteLine($"Match {match.Value}");
                                foreach (var feature in fc.Features)
                                {
                                    //Some strange casting issues here with defaut int which is 32-bit (? why)
                                    if ((Int64)feature.Properties["PLZ"] == Int64.Parse(match.Value))
                                    {
                                        Debug.WriteLine("Matched PLZ");
                                        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
                                        rangesDates.Last().Location = new Location(point.Coordinates.Latitude, point.Coordinates.Longitude);
                                    }
                                }
                            }
                        }
                    }

                    Stand.RangeDate rd = new Stand.RangeDate(start, end, nameAndLocation[0], nameAndLocation[1]);
                    if (rangesDates.Last().StandClubs.Count > 0)
                    {
                        rangesDates.Last().StandClubs.Last().ClubRangeDates.Add(rd);
                    }
                    else
                    {
                        rangesDates.Last().StandRangeDates.Add(rd);
                    }
                }
            }
        } 
        catch (Exception ex)
        {
            App.PopupService.Alert("Failed Search", "Whoops", "Ok");
            Debug.WriteLine($"Exception thing : {ex.Message}");
            Searching.Invoke(this, false);
            rangesDates.Clear(); //For clarity, remove existing things added before crash...
        }

        //TODO for simplicity, I only want to show this as a 1-dimensional list (no tree or table or whatever)
        // - also allow sorting by date or location or whatever...
        return rangesDates;
    }

    private void DisplayResults(List<Stand> dates)
    {
        Results.Invoke(this, dates);
    }

    private string BuildQuery(DateTime from, DateTime to, PickerOption canton, PickerOption program, PickerOption weapon)
    {
        return $"<call><funcname>getSchiesstage</funcname><p1>{program.Value}</p1><p1>{weapon.Value}</p1><p1>{canton.Value}</p1><p1></p1><p1>{from.ToVVAEsportCHParameter()}</p1><p1>{to.ToVVAEsportCHParameter()}</p1><p1>en</p1></call>";
    }

    /**
     * For reverse binding VM -> View(C#) to be able to work with TableView
     */
    public event EventHandler<List<Stand>> Results;

    /**
     * For reverse binding VM -> View(C#) to be able to clear TableView & show activity
     */
    public event EventHandler<bool> Searching;

    public RangeDatesViewModel()
    {
        program = Programs.First();
        canton = Cantons.First();
        weapon = Weapons.First();
    }
}

public partial class RangeDates : ContentPage
{
    public RangeDates(RangeDatesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        vm.Results += ViewModel_Search_Results;
        vm.Searching += ViewModel_Searching;
    }

    private void ViewModel_Searching(object sender, bool searching)
    {
        FindNearBtn.IsEnabled = !searching;
        SearchBtn.IsEnabled = !searching;
        ActivityIndicator.IsRunning = searching;

        ResultsTableView.Root.Clear();
    }

    private void ViewModel_Search_Results(object sender, List<RangeDatesViewModel.Stand> stands)
    {
        FindNearBtn.IsEnabled = true;
        SearchBtn.IsEnabled = true;

        ActivityIndicator.IsRunning = false;
        //Here, do shiz with the TableView
        // as cannot access View from ViewModel
        Debug.WriteLine("Triggered Results event");

        ResultsTableView.Root.Clear();

        //https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/tableview
        //https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.tableview?view=net-maui-7.0
        TableRoot root = new TableRoot("Range Dates");

        foreach (var stand in stands)
        {
            string title = stand.Name;

            if (stand.Distance > 0)
            {
                title += " - " + stand.Distance.ToString();
            }

            TableSection section = new TableSection(title) {   };

            if (stand.StandRangeDates != null || stand.StandRangeDates.IsNotEmpty())
            {
                foreach (RangeDatesViewModel.Stand.RangeDate date in stand.StandRangeDates)
                {
                    section.Add(new TextCell
                    {
                        Text = $"{date.Start} - {date.End}"
                    });
                }
            }

            root.Add(section);
        }

        ResultsTableView.Root = root;
    }
}
