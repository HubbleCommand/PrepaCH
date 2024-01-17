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

[Serializable]
public class SATQueryResultItem
{
    public string disciplineId { get; set; }

    public string disciplineShortNameGerman { get; set; }
    public string disciplineShortNameFrench { get; set; }
    public string disciplineShortNameItalian { get; set; }

    public string disciplineLongNameGerman { get; set; }
    public string disciplineLongNameFrench { get; set; }
    public string disciplineLongNameItalian { get; set; }

    public DateTime from { get; set; }
    public DateTime to { get; set; }

    public string location { get; set; }
    public string combinedLocationString { get; set; }
    public string coordinates { get; set; }
    public string canton { get; set; }

    public string organizationId { get; set; }
    public string organizationName { get; set; }

    public override string ToString()
    {
        //return $"{{disciplineId: {disciplineId}, disciplineShortNameFrench: {disciplineShortNameFrench}, from: {from}, location: {location}, coordinates: {coordinates}}}";
        return $"{{name: {organizationName}, location: {location}, coordinates: {coordinates}}}";
    }
}

[Serializable]
public class SATQueryResults
{
    public List<SATQueryResultItem> items { get; set; }
}

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
        new PickerOption(Strings.ObligatoryProgram, "OP"), //1
        new PickerOption(Strings.FederalProgram50m, "BP"),  //It's TR now? Also 5
        new PickerOption(Strings.FieldShooting, "FS"),  //2
        new PickerOption(Strings.CompetitionShooting, "SF"), //4
        new PickerOption(Strings.YoungShooters, "JS"), //3
        new PickerOption(Strings.Other, "Anderes"), //6
    };

    public static int GetSATAdminProgramKey(PickerOption option)
    {
        switch (option.Value)
        {
            case "OP": return 1;
            case "BP": return 5;
            case "FS": return 2;
            case "SF": return 4;
            case "JS": return 3;
            case "Anderes": return 6;
        }
        return -1;
    }

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
#nullable enable
        public Location? Location;
#nullable disable
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

#nullable enable
    static FeatureCollection? communes = null;
#nullable disable
    public static async Task<FeatureCollection> GetCommunes()
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

#nullable enable
        Location? location = await Utils.GetCachedLocation();
#nullable disable
        if (location == null)
        {
            await App.PopupService.Alert("Cannot get location", "Make sure you have given the appropriate permissions or that your device's location has been turned on.", "OK");
        }

        RangeDateQuery query = new(DateTime.Now, To, new HashSet<PickerOption>() { Cantons.First() }, Program, Weapon);
        query.WithCommuneSearch = true;
        List<Stand> results = await query.Query();

        List<Stand> withLocation = (from result in results
                           where result.Location != null
                           select result).ToList();
        Debug.WriteLine($"Removed nulls (removed {results.Count - withLocation.Count})");
        foreach (var q in withLocation)
        {
            q.Distance = location.CalculateDistance2D(q.Location);
        }
        withLocation.Sort((x, y) => x.Distance.CompareTo(y.Distance));
        Debug.WriteLine("Sorted results by distance");
        DisplayResults(withLocation);
    }

    class RangeDateQuery
    {
        public readonly DateTime From;
        public readonly DateTime To;
        public readonly HashSet<PickerOption> Canton;
        public readonly PickerOption Program;
        public readonly PickerOption Weapon;
        public bool WithCommuneSearch = false;

        public static readonly DateTime APISPlitDate = new DateTime(2024, 1, 1);

        public RangeDateQuery(DateTime from, DateTime to, HashSet<PickerOption> canton, PickerOption program, PickerOption weapon)
        {
            From = from;
            To = to;
            Program = program;
            Weapon = weapon;

            if (From < APISPlitDate)
            {
                Canton = new HashSet<PickerOption>() { canton.First() };
            }

            if (From < APISPlitDate && To > APISPlitDate)
            {
                //This is to avoid having to do a query on both APIs & merging the results
                From = APISPlitDate;
            }
        }

        private Uri GetEndpointURI()
        {
            if (From < APISPlitDate)
                return new Uri("https://ssv-vva.esport.ch/p2plus/ssv/SAT4Calls.asp");
            else
                return new Uri("https://www.sat.admin.ch/api/shootingDayDeclaration/search");
        }

        private string BuildQuery()
        {
            if (From < APISPlitDate)
                return $"<call><funcname>getSchiesstage</funcname><p1>{Program.Value}</p1><p1>{Weapon.Value}</p1><p1>{Canton.First().Value}</p1><p1></p1><p1>{From.ToVVAEsportCHParameter()}</p1><p1>{To.ToVVAEsportCHParameter()}</p1><p1>en</p1></call>";
            else
            {
                string qry = "{\"startRow\":0,\"endRow\":200,\"filterModels\":{";
                qry += $"\"from\":{{\"filterType\":\"date\",\"variant\":\"inRange\",\"filter\":\"{From.ToSATAdminParameter()}\",\"filterTo\":\"{To.ToSATAdminParameter()}\"}},";

                if (Weapon.Value == "G")
                {
                    qry += "\"disciplineId\":{\"filterType\":\"multi-select\",\"variant\":\"singleTargetInListGuid\",\"filter\":[\"4c1f8d60-2dd2-406b-8fcf-b3323619abe1\",\"5bf1f6f5-c201-479b-bb21-f2dab042cf11\",\"a38d8f3f-2860-4f23-bba3-e38d5412c707\",\"b013fd1a-6e24-4ac0-bba4-a8255c70817e\"]},";
                }
                else
                {
                    qry += "\"disciplineId\":{\"filterType\":\"multi-select\",\"variant\":\"singleTargetInListGuid\",\"filter\":[\"03f033b9-0379-4eb7-a721-620189fe8a6c\",\"32e26f77-e0f6-4af9-bafa-e67e958d21ed\",\"4b059f82-4fab-4c4e-abe9-084305fdac4e\",\"cfb57703-470e-43c3-beec-faafb6c6357c\"]},";
                }

                //Program
                qry += $"\"type\":{{\"filterType\":\"number\",\"variant\":\"equals\",\"filter\":{GetSATAdminProgramKey(Program)}}},";
                qry += "},\"includeCount\":false,\"sortModel\":[{\"columnId\":\"from\",\"sort\":\"asc\"}]}";    //Trailing
                return qry;
            }
        }

#nullable enable
        private async Task<Location?> GetCommune(string location)
#nullable disable
        {
            FeatureCollection fc = await GetCommunes();
            //https://stackoverflow.com/questions/32589177/find-4-sequence-numbers-in-string-using-c-sharp
            var regex = new Regex(@"\d{4}");

            foreach (Match match in regex.Matches(location))
            {
                Debug.WriteLine($"Match {match.Value}");
                foreach (var feature in fc.Features)
                {
                    //Some strange casting issues here with defaut int which is 32-bit (? why)
                    if ((Int64)feature.Properties["PLZ"] == Int64.Parse(match.Value))
                    {
                        Debug.WriteLine("Matched PLZ");
                        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
                        return new Location(point.Coordinates.Latitude, point.Coordinates.Longitude);
                    }
                }
            }

            return null;
        }


        public async Task<List<Stand>> Query()
        {
            HttpClient _client = new HttpClient();
            List<Stand> rangesDates = new List<Stand>();
            string queryStr = BuildQuery();

            try {
                if (From < APISPlitDate)
                {
                    StringContent query = new StringContent(queryStr, Encoding.UTF8, "text/xml");
                    Debug.WriteLine($"Query - {queryStr}");

                    Uri uri = new Uri("https://ssv-vva.esport.ch/p2plus/ssv/SAT4Calls.asp");

                    HttpResponseMessage response = await _client.PostAsync(GetEndpointURI(), query);
                    Debug.WriteLine($"Response Code - {response.StatusCode}");

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response.Content.ReadAsStringAsync().Result);
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

                        //If the second column is equal to 1, at a new stand, so can go to the next line
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
                                //2 scenarios are possible, either it's a GPS position in lat+long format, or a Commune, NPA Commune format (any other format cannot be extrapolated)

                                string[] gpsSPlitTry = nameAndLocation[1].Split(new string[] { "+" }, StringSplitOptions.TrimEntries);// ("+");
                                Debug.WriteLine($"GPS Split {gpsSPlitTry.Length}");
                                if (gpsSPlitTry.Length == 2)
                                {
                                    //On language is set to French, will fail parsing as double is in "46.123" instead of "46,123" (period vs comma), so need to do invariant
                                    rangesDates.Last().Location = new Location(double.Parse(gpsSPlitTry[0], CultureInfo.InvariantCulture), double.Parse(gpsSPlitTry[1], CultureInfo.InvariantCulture));
                                    Debug.WriteLine($"Location : {rangesDates.Last().Location}");
                                }
                                else if (WithCommuneSearch)
                                {
                                    var location = await GetCommune(nameAndLocation[1]);
                                    if (location != null)
                                    {
                                        rangesDates.Last().Location = location;
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

                //Post-2024 dates
                else
                {
                    StringContent query = new StringContent(queryStr, Encoding.UTF8, "application/json");
                    Debug.WriteLine($"Query - {queryStr}");
                    HttpResponseMessage response = await _client.PostAsync(GetEndpointURI(), query);
                    string responseString = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response Code - {response.StatusCode}");
                    Debug.WriteLine($"Response Content - {responseString}");

                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    SATQueryResults resultParsed = JsonConvert.DeserializeObject<SATQueryResults>(responseString, settings);
                    Debug.WriteLine($"Response deserialized count - {resultParsed.items.Count()}");
                    Debug.WriteLine($"Response deserialized - {string.Join(", ", resultParsed.items)}");

                    foreach (SATQueryResultItem item in resultParsed.items)
                    {
#nullable enable
                        Stand? stand = null;
#nullable disable
                        foreach (Stand _stand in rangesDates)
                        {
                            if (_stand.Name == item.organizationName)
                            {
                                stand = _stand;
                                break;
                            }
                        }

                        //Ugh, stuff is different here is well, doesn't group by stand... doing it myself for consistent UI...
                        if (stand == null)
                        {
                            stand = new Stand(item.organizationName);
                            rangesDates.Add(stand);

                            //Another difference here is that the coordinates are in a Swiss CRS (see CHGeoJsonMerger for more details abt how much of a problem that is)
                            // unfortunately this means that for range dates starting in 2024, distancees will be calculated based on the canton they are in...
                            if (WithCommuneSearch && item.combinedLocationString.Length > 0)
                            {
                                var location = await GetCommune(item.combinedLocationString);
                                if (location != null)
                                {
                                    stand.Location = location;
                                }
                            }
                        }
                        Stand.RangeDate rd = new Stand.RangeDate(item.from, item.to, item.organizationName, item.location);
                        stand.StandRangeDates.Add(rd);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = App.PopupService.Alert(Strings.FailedSearch, "", "Ok");
                Debug.WriteLine($"Exception while searching : {ex.Message}");
                rangesDates.Clear(); //For clarity, remove existing things added before crash...
            }

            return rangesDates;
        }
    }

    [RelayCommand]
    public async Task Search()
    {
        Searching.Invoke(this, true);

        RangeDateQuery query = new RangeDateQuery(From, To, new HashSet<PickerOption>() { Canton }, Program, Weapon);
        var results = await query.Query();
        DisplayResults(results);
    }

    private void DisplayResults(List<Stand> dates)
    {
        Results.Invoke(this, dates);
    }

    /**
     * For reverse binding VM -> View(C#) to be able to work with TableView
     */
    public event EventHandler<List<Stand>> Results;

    /**
     * For reverse binding VM -> View(C#) to be able to clear TableView and show activity
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
        ResultsTableView.Root.Clear();

        //https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/tableview
        //https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.tableview?view=net-maui-7.0
        TableRoot root = new TableRoot("Range Dates");

        foreach (var stand in stands)
        {
            string title = stand.Name;

            if (stand.Distance > 0)
            {
                title += " - " + stand.Distance.ToString("N1") + " km";
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
